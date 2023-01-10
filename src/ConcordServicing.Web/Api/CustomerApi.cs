using ConcordServicing.Data.Messages;
using Microsoft.AspNetCore.Http.HttpResults;
using Wolverine;

namespace ConcordServicing.Web.Api;

public static class CustomerApi
{
    public static void MapCustomerApi(this IEndpointRouteBuilder app)
    {
        var customer = app.MapGroup("/api/customer");

        customer.MapGet("/", GetCustomer)
            .WithOpenApi(o => new(o) { Summary = "Get customer" });
        
        customer.MapPost("/address", UpdateAddress)
            .WithOpenApi(o => new(o) { Summary = "Update customer address" });

        customer.WithParameterValidation();
    }

    public static async Task<Results<NotFound, Ok<Customer>>> GetCustomer(IMessageBus bus)
    {
        var address = await bus.InvokeAsync<Customer>(new GetCustomer { Id = "123" });
        if (address.Success == false)
            return TypedResults.NotFound();

        return TypedResults.Ok(address);
    }

    public static async Task<Results<NotFound, Ok<Customer>>> UpdateAddress(UpdateCustomerAddress cmd, IMessageBus bus)
    {
        var address = await bus.InvokeAsync<Customer>(cmd);

        if (address.Success == false)
            return TypedResults.NotFound();

        return TypedResults.Ok(address);
    }
}
