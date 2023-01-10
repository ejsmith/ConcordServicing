using ConcordServicing.Data;
using Foundatio.Extensions.Hosting.Startup;
using JasperFx.Core;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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

        builder.Services.AddDbContext<ConcordDbContext>(x =>
            {
                if (connectionString != null)
                    x.UseSqlServer(connectionString);
                else
                    x.UseInMemoryDatabase("Concord");
            },
            optionsLifetime: ServiceLifetime.Singleton);

        return builder;
    }

    public static WebApplicationBuilder UseConcordWolverine(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("SqlServer");

        builder.Host.UseWolverine(opts =>
        {
             if (connectionString != null)
             {
                 opts.PersistMessagesWithSqlServer(connectionString);
                 opts.UseEntityFrameworkCoreTransactions();
             }

             opts.Handlers.Discovery(x =>
             {
                 // turn CSS handlers off in dev mode
                 //if (!builder.Environment.IsDevelopment())
                 x.IncludeAssembly(typeof(CSS.Handlers.CustomerHandler).Assembly);

                 x.IncludeAssembly(typeof(Data.Handlers.CustomerHandler).Assembly);
             });

            opts.Handlers.OnException<ApplicationException>()
                .RetryWithCooldown(50.Milliseconds(), 100.Milliseconds(), 250.Milliseconds());
        }).UseResourceSetupOnStartup(StartupAction.ResetState);

        return builder;
    }

    public static WebApplicationBuilder AddSampleDataStartupAction(this WebApplicationBuilder builder)
    {
        builder.Services.AddStartupAction("ConfigureIndexes", async sp =>
        {
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

}
