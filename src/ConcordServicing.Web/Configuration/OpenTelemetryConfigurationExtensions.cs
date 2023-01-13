using System.Diagnostics.Tracing;
using System.Reflection;
using Foundatio.Extensions.Hosting.Startup;
using OpenTelemetry.Extensions.Hosting.Implementation;
using OpenTelemetry.Internal;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace OpenTelemetry;

public static class OpenTelemetryExtensions
{
    public static WebApplicationBuilder AddOpenTelemetry(this WebApplicationBuilder builder)
    {
        var config = new OpenTelemetryConfig(builder.Configuration, "web", false);
        var services = builder.Services;

        string apiKey = config.ApiKey;
        if (!String.IsNullOrEmpty(apiKey) && apiKey.Length > 6)
            apiKey = String.Concat(apiKey.AsSpan(0, 6), "***");

        services.AddStartupAction("ShowOpenTelemetryConfig", sp =>
        {
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("OpenTelemetry");
            logger.LogInformation("Configuring OpenTelemetry: Endpoint={Endpoint} ApiKey={ApiKey} EnableTracing={EnableTracing} EnableLogs={EnableLogs} FullDetails={FullDetails} EnableRedis={EnableRedis} SampleRate={SampleRate}",
                config.Endpoint, apiKey, config.EnableTracing, config.EnableLogs, config.FullDetails, config.EnableRedis, config.SampleRate);
        });

        var resourceBuilder = ResourceBuilder.CreateDefault().AddService(config.ServiceName).AddAttributes(new[] {
            new KeyValuePair<string, object>("service.namespace", config.ServiceNamespace),
            new KeyValuePair<string, object>("service.environment", config.ServiceEnvironment),
            new KeyValuePair<string, object>("service.version", Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown")
        });

        services.AddSingleton(config);
        services.AddHostedService(sp => new SelfDiagnosticsLoggingHostedService(sp.GetRequiredService<ILoggerFactory>(), config.Debug ? EventLevel.Verbose : null));

        services.AddOpenTelemetry().WithMetrics(b =>
        {
            b.SetResourceBuilder(resourceBuilder);

            b.AddHttpClientInstrumentation();
            b.AddAspNetCoreInstrumentation();
            b.AddMeter("ConcordServicing", "Foundatio", "Wolverine:ConcordServicing.Web");
            b.AddRuntimeInstrumentation();

            if (config.Console)
                b.AddConsoleExporter((exporterOptions, metricReaderOptions) =>
                {
                    // The ConsoleMetricExporter defaults to a manual collect cycle.
                    // This configuration causes metrics to be exported to stdout on a 10s interval.
                    metricReaderOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 10000;
                });

            b.AddPrometheusExporter();

            if (!String.IsNullOrEmpty(config.Endpoint))
                b.AddOtlpExporter((c, o) =>
                {
                    // needed for newrelic compatibility until they support cumulative
                    o.TemporalityPreference = MetricReaderTemporalityPreference.Delta;

                    if (!String.IsNullOrEmpty(config.Endpoint))
                        c.Endpoint = new Uri(config.Endpoint);
                    if (!String.IsNullOrEmpty(config.ApiKey))
                        c.Headers = $"api-key={config.ApiKey}";
                });
        }).StartWithHost();

        if (config.EnableTracing)
            services.AddOpenTelemetry().WithTracing(b =>
            {
                b.SetResourceBuilder(resourceBuilder);

                b.AddAspNetCoreInstrumentation(o =>
                {
                    o.Filter = context =>
                    {
                        return !context.Request.Headers.UserAgent.ToString().Contains("HealthChecker");
                    };
                });

                b.AddHttpClientInstrumentation();
                b.AddSqlClientInstrumentation(o =>
                {
                    o.EnableConnectionLevelAttributes = true;
                    o.RecordException = true;
                    o.SetDbStatementForText = true;
                    o.SetDbStatementForStoredProcedure = true;
                });
                b.AddSource("Foundatio");
                b.AddSource("Wolverine");

                if (config.EnableRedis)
                    b.AddRedisInstrumentation(null, c =>
                    {
                        c.EnrichActivityWithTimingEvents = false;
                        c.SetVerboseDatabaseStatements = config.FullDetails;
                    });

                b.SetSampler(new TraceIdRatioBasedSampler(config.SampleRate));

                if (config.Console)
                    b.AddConsoleExporter();

                if (!String.IsNullOrEmpty(config.Endpoint))
                {
                    if (config.MinDurationMs > 0)
                    {
                        // filter out insignificant activities
                        b.AddFilteredOtlpExporter(c =>
                        {
                            if (!String.IsNullOrEmpty(config.Endpoint))
                                c.Endpoint = new Uri(config.Endpoint);
                            if (!String.IsNullOrEmpty(config.ApiKey))
                                c.Headers = $"api-key={config.ApiKey}";

                            c.Filter = a => a.Duration > TimeSpan.FromMilliseconds(config.MinDurationMs) || a.GetTagItem("db.system") != null;
                        });
                    }
                    else
                    {
                        b.AddOtlpExporter(c =>
                        {
                            if (!String.IsNullOrEmpty(config.Endpoint))
                                c.Endpoint = new Uri(config.Endpoint);
                            if (!String.IsNullOrEmpty(config.ApiKey))
                                c.Headers = $"api-key={config.ApiKey}";
                        });
                    }
                }
            });

        if (config.EnableLogs)
        {
            builder.Logging.AddOpenTelemetry(o =>
            {
                o.SetResourceBuilder(resourceBuilder);
                o.IncludeScopes = true;
                o.ParseStateValues = true;
                o.IncludeFormattedMessage = true;

                if (config.Console)
                    o.AddConsoleExporter();

                if (!String.IsNullOrEmpty(config.Endpoint))
                {
                    o.AddOtlpExporter(c =>
                    {
                        if (!String.IsNullOrEmpty(config.Endpoint))
                            c.Endpoint = new Uri(config.Endpoint);
                        if (!String.IsNullOrEmpty(config.ApiKey))
                            c.Headers = $"api-key={config.ApiKey}";
                    });
                }
            });
        }

        return builder;
    }
}

public class OpenTelemetryConfig
{
    private readonly IConfiguration _config;

    public OpenTelemetryConfig(IConfigurationRoot config, string processName, bool enableRedis)
    {
        _config = config.GetSection("OpenTelemetry");
        processName = processName.StartsWith("-") ? processName : "-" + processName;

        ServiceName = _config.GetValue<string>("ServiceName") + processName;
        if (ServiceName.StartsWith("-"))
            ServiceName = ServiceName.Substring(1);

        ServiceEnvironment = _config.GetValue<string>("ServiceEnvironment") ?? String.Empty;
        ServiceNamespace = _config.GetValue<string>("ServiceNamespace") ?? ServiceName;
        EnableRedis = enableRedis;
    }

    public bool EnableTracing => _config.GetValue("EnableTracing", _config.GetValue("Enabled", false));
    public bool EnableLogs => _config.GetValue("EnableLogs", false);
    public string ServiceName { get; }
    public string ServiceEnvironment { get; }
    public string ServiceNamespace { get; }
    public string Endpoint => _config.GetValue<string>("Endpoint") ?? String.Empty;
    public string ApiKey => _config.GetValue<string>("ApiKey") ?? String.Empty;
    public bool FullDetails => _config.GetValue("FullDetails", false);
    public double SampleRate => _config.GetValue("SampleRate", 1.0);
    public int MinDurationMs => _config.GetValue<int>("MinDurationMs", -1);
    public bool EnableRedis { get; }
    public bool Debug => _config.GetValue("Debug", false);
    public bool Console => _config.GetValue("Console", false);
}
