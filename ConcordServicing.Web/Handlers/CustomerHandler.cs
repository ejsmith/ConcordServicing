using ConcordServicing.Data.Messages;

namespace ConcordServicing.Web.Handlers;

public class CustomerHandler
{
    public static void Handle(CustomerAddressUpdated command, ILogger<CustomerHandler> logger)
    {
        logger.LogInformation("Customer address was updated {CustomerId}", command.Id);

        // TODO: send websocket message to all appropriate clients to enable real-time updates
    }
}
