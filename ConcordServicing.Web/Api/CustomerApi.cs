using ConcordServicing.Data.Messages;
using Wolverine;

namespace ConcordServicing.Web.Api;

public static class CustomerApi
{
    public static void MapCustomerApi(this IEndpointRouteBuilder app)
    {
        var customer = app.MapGroup("/api/customer");

        customer.MapGet("/", async (IMessageBus bus) =>
        {
            var address = await bus.InvokeAsync<CustomerAddress>(new GetCustomerProfile { Id = "123" });
            if (address.Success == false)
                return Results.NotFound();

            return Results.Ok(address);
        })
            .Produces<CustomerAddress>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi(o => new(o) { Summary = "Get customer" });
        
        customer.MapPost("/address", async (UpdateCustomerAddress cmd, HttpRequest request, IMessageBus bus) =>
        {
            // TODO: figure out why the cmd parameter isn't getting bound
            var requestCmd = await request.ReadFromJsonAsync<UpdateCustomerAddress>();

            // TODO: This should be an invoke that returns the customer, but there is a bug with Wolverine
            await bus.SendAsync(requestCmd);

            return Results.Accepted();
        })
            .Accepts<UpdateCustomerAddress>("application/json")
            .Produces(StatusCodes.Status202Accepted)
            .WithOpenApi(o =>
            {
                var p1 = o.Parameters;
                return new(o) { Summary = "Update customer address" };
            });
    }
}
