using ConcordServicing.Data;
using Microsoft.EntityFrameworkCore;
using Oakton.Resources;
using Wolverine;
using Wolverine.EntityFrameworkCore;
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
        }).UseResourceSetupOnStartup(StartupAction.ResetState);

        return builder;
    }
}
