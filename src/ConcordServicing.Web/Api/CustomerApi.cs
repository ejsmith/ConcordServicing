using ConcordServicing.Data.Messages;
using Microsoft.AspNetCore.Http.HttpResults;
using Wolverine;

namespace ConcordServicing.Web.Api;

public static class CustomerApi
{
    public static void MapCustomerApi(this IEndpointRouteBuilder app)
    {
        var customer = app.MapGroup("/api/customer");

        customer.MapGet("/", GetCustomerAsync)
            .WithOpenApi(o => new(o) { Summary = "Get customer" });

        customer.MapPost("/address", UpdateAddressAsync)
            .WithOpenApi(o => new(o) { Summary = "Update customer address" });

        customer.WithParameterValidation();
    }

    public static async Task<Results<NotFound, Ok<Customer>>> GetCustomerAsync(IMessageBus bus)
    {
        var customer = await bus.InvokeAsync<Customer>(new GetCustomer { Id = "123" });

        if (!customer.Found)
            return TypedResults.NotFound();

        return TypedResults.Ok(customer);
    }

    public static async Task<Results<NotFound, Ok<Customer>>> UpdateAddressAsync(UpdateCustomerAddress cmd, IMessageBus bus)
    {
        var customer = await bus.InvokeAsync<Customer>(cmd);

        if (!customer.Found)
            return TypedResults.NotFound();

        return TypedResults.Ok(customer);
    }
}
