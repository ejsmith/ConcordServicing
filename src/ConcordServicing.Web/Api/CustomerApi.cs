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
        var customer = await bus.InvokeAsync<Customer>(new GetCustomer { Id = "123" });
        
        if (!customer.Found)
            return TypedResults.NotFound();

        return TypedResults.Ok(customer);
    }

    public static async Task<Results<NotFound, Ok<Customer>>> UpdateAddress(UpdateCustomerAddress cmd, IMessageBus bus)
    {
        var customer = await bus.InvokeAsync<Customer>(cmd);

        if (!customer.Found)
            return TypedResults.NotFound();

        return TypedResults.Ok(customer);
    }
}
