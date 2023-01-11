using Microsoft.Extensions.Options;
using OpenTelemetry.Exporter;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace OpenTelemetry.Internal;

public sealed class CustomFilterProcessor : CompositeProcessor<Activity>
{
    private readonly Func<Activity, bool> _filter;

    public CustomFilterProcessor(BaseProcessor<Activity> processor, Func<Activity, bool> filter) : base(new[] { processor })
    {
        _filter = filter;
    }

    public override void OnEnd(Activity activity)
    {
        if (_filter == null || _filter(activity))
            base.OnEnd(activity);
    }
}

public static class CustomFilterProcessorExtensions
{
    public static TracerProviderBuilder AddFilteredOtlpExporter(this TracerProviderBuilder builder, Action<FilteredOtlpExporterOptions> configure = null)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        if (builder is IDeferredTracerProviderBuilder deferredTracerProviderBuilder)
        {
            return deferredTracerProviderBuilder.Configure((sp, builder) => {
                var oltpOptions = sp.GetService<IOptions<FilteredOtlpExporterOptions>>()?.Value ?? new FilteredOtlpExporterOptions();
                AddFilteredOtlpExporter(builder, oltpOptions, configure, sp);
            });
        }

        return AddFilteredOtlpExporter(builder, new FilteredOtlpExporterOptions(), configure, serviceProvider: null);
    }

    internal static TracerProviderBuilder AddFilteredOtlpExporter(
        TracerProviderBuilder builder,
        FilteredOtlpExporterOptions exporterOptions,
        Action<FilteredOtlpExporterOptions> configure,
        IServiceProvider serviceProvider,
        Func<BaseExporter<Activity>, BaseExporter<Activity>> configureExporterInstance = null)
    {

        configure?.Invoke(exporterOptions);

        exporterOptions.TryEnableIHttpClientFactoryIntegration(serviceProvider, "OtlpTraceExporter");

        BaseExporter<Activity> otlpExporter = new OtlpTraceExporter(exporterOptions);

        if (configureExporterInstance != null)
            otlpExporter = configureExporterInstance(otlpExporter);

        if (exporterOptions.ExportProcessorType == ExportProcessorType.Simple)
        {
            return builder.AddProcessor(new CustomFilterProcessor(new SimpleActivityExportProcessor(otlpExporter), exporterOptions.Filter));
        }
        else
        {
            var batchOptions = exporterOptions.BatchExportProcessorOptions ?? new();

            return builder.AddProcessor(new CustomFilterProcessor(new BatchActivityExportProcessor(
                otlpExporter,
                batchOptions.MaxQueueSize,
                batchOptions.ScheduledDelayMilliseconds,
                batchOptions.ExporterTimeoutMilliseconds,
                batchOptions.MaxExportBatchSize), exporterOptions.Filter));
        }
    }

    public static void TryEnableIHttpClientFactoryIntegration(this OtlpExporterOptions options, IServiceProvider serviceProvider, string httpClientName)
    {
        // use reflection to call the method

        var exporterExtensionsType = typeof(OtlpExporterOptions).Assembly.GetType("OpenTelemetry.Exporter.OtlpExporterOptionsExtensions");
        exporterExtensionsType
            .GetMethod("TryEnableIHttpClientFactoryIntegration")
            .Invoke(null, new object[] { options, serviceProvider, httpClientName });
    }
}

public class FilteredOtlpExporterOptions : OtlpExporterOptions
{
    public Func<Activity, bool> Filter { get; set; }
}
