using ConcordServicing.Data.Messages;
using ConcordServicing.Web.Api;

namespace ConcordServicing.Web.Handlers;

public class CustomerHandler
{
    public static Task HandleAsync(CustomerAddressUpdated command, ClientAppEventDispatcher clientDispatcher, ILogger<CustomerHandler> logger)
    {
        logger.LogInformation("Customer address was updated {CustomerId}", command.Id);

        return clientDispatcher.DispatchMessageAsync(command);
    }
}
