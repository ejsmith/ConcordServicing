using ConcordServicing.Data.Messages;
using Microsoft.Extensions.Logging;

namespace ConcordServicing.CSS.Handlers;

public class CustomerHandler
{
    public static void Handle(UpdateCustomerAddress command, ILogger<CustomerHandler> logger)
    {
        logger.LogInformation("Calling CSS to update customer {CustomerId}", command.Id);
        
        // CSS is an external system and is the source of truth for any data writes while we are migrating all of the apps over to SQL Server
        
        // call the CSS action first, if it fails, it will not update SQL Server
        // this can use retry policies to handle transient errors
        // this entire CSS project will eventually just be deleted when we get everything migrated to SQL Server
        // during local dev, we won't register any of the CSS handlers and the app will be just a normal SQL Server app

        Thread.Sleep(1000);

        // TODO: do CSS action to update the address, if it fails, don't continue to the next handler

        // emulate a transient error
        if (Random.Shared.Next(100) < 80)
        {
            logger.LogError("Error calling CSS to update customer {CustomerId}", command.Id);
            throw new ApplicationException("Unable to process CSS action.");
        }
    }
}
