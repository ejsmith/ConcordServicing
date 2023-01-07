using ConcordServicing.Data.Messages;
using Wolverine;

namespace ConcordServicing.Web.Api;

public static class CustomerApi
{
    public static void MapCustomerApi(this IEndpointRouteBuilder app)
    {
        var customer = app.MapGroup("/customer");

        customer.MapGet("/{id}/address", async (string id, IMessageBus bus) => TypedResults.Ok(await bus.InvokeAsync<CustomerAddress>(new GetCustomerAddress { Id = id })))
            .WithOpenApi(o => new(o) { Summary = "Get customer address" });
        
        customer.MapPut("{id}/address", async (UpdateCustomerAddress cmd, IMessageBus bus) => await bus.SendAsync(cmd))
            .WithOpenApi(o => new(o) { Summary = "Update customer address" });
    }
}
