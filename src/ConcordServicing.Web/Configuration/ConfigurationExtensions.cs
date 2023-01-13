using System.Text.Json;
using ConcordServicing.Data;
using Foundatio.Extensions.Hosting.Startup;
using JasperFx.Core;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Oakton;
using Oakton.Resources;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.ErrorHandling;
using Wolverine.SqlServer;

namespace ConcordServicing.Web.Configuration;

public static class ConfigurationExtensions
{
    public static WebApplicationBuilder AddConcordDbContext(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("SqlServer");

        builder.Services.AddDbContextWithWolverineIntegration<ConcordDbContext>(x =>
            {
                if (connectionString != null)
                    x.UseSqlServer(connectionString, o => o.MigrationsAssembly("ConcordServicing.Web"));
                else
                    x.UseInMemoryDatabase("Concord");
            });

        return builder;
    }

    public static WebApplicationBuilder UseConcordWolverine(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("SqlServer");

        builder.Host.ApplyOaktonExtensions();

        builder.Host.UseWolverine(opts =>
        {
            if (connectionString != null)
            {
                opts.PersistMessagesWithSqlServer(connectionString);
                opts.UseEntityFrameworkCoreTransactions();
            }

            opts.Node.CodeGeneration.TypeLoadMode = JasperFx.CodeGeneration.TypeLoadMode.Auto;

            opts.Policies.UseDurableLocalQueues();

            opts.Handlers.OnException<ApplicationException>()
                .RetryWithCooldown(50.Milliseconds(), 100.Milliseconds(), 250.Milliseconds());

            opts.Handlers.Discovery(x =>
             {
                 // turn CSS handlers off in dev mode
                 //if (!builder.Environment.IsDevelopment())
                 x.IncludeAssembly(typeof(CSS.Handlers.CustomerHandler).Assembly);

                 x.IncludeAssembly(typeof(Data.Handlers.CustomerHandler).Assembly);
             });
        });

        builder.Host.UseResourceSetupOnStartup();

        return builder;
    }

    public static WebApplicationBuilder AddCreateSampleDataStartupAction(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("SqlServer");
        var isDevelopment = builder.Environment.IsDevelopment();

        builder.Services.AddStartupAction("ConfigureDatabase", async sp =>
        {
            if (!isDevelopment)
                return;

            // add some sample data if there is none
            var db = sp.GetRequiredService<ConcordDbContext>();

            if (await db.Customers.FindAsync("123") == null)
            {
                db.Customers.Add(new Data.Models.Customer
                {
                    Id = "123",
                    Address = "123 Main St"
                });

                await db.SaveChangesAsync();
            }
        });

        return builder;
    }

    public static void MapHealthChecksWithJsonResponse(this IEndpointRouteBuilder endpoints, PathString path)
    {
        var options = new HealthCheckOptions
        {
            ResponseWriter = async (httpContext, healthReport) =>
            {
                httpContext.Response.ContentType = "application/json";

                var result = JsonSerializer.Serialize(new
                {
                    status = healthReport.Status.ToString(),
                    totalDurationInSeconds = healthReport.TotalDuration.TotalSeconds,
                    entries = healthReport.Entries.Select(e => new
                    {
                        key = e.Key,
                        status = e.Value.Status.ToString(),
                        description = e.Value.Description,
                        data = e.Value.Data
                    })
                }, new JsonSerializerOptions { WriteIndented = true });
                await httpContext.Response.WriteAsync(result);
            }
        };
        endpoints.MapHealthChecks(path, options);
    }
}
