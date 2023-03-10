using Microsoft.AspNetCore.SignalR;

namespace ConcordServicing.Web.Api;

public class ClientAppEventsHub : Hub
{
}

public class ClientAppEventDispatcher
{
    private readonly IHubContext<ClientAppEventsHub> _hub;

    public ClientAppEventDispatcher(IHubContext<ClientAppEventsHub> hub)
    {
        _hub = hub;
    }

    public Task DispatchMessageAsync(object message)
    {
        return _hub.Clients.All.SendAsync("dispatch", new { type = message.GetType().FullName, body = message });
    }

    public Task DispatchMessageAsync(string type, object message)
    {
        return _hub.Clients.All.SendAsync("dispatch", new { type = type, body = message });
    }
}
