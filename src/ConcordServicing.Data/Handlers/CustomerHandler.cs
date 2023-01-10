using Wolverine.Attributes;
using ConcordServicing.Data.Messages;
using Wolverine;
using Microsoft.Extensions.Logging;

namespace ConcordServicing.Data.Handlers;

public class CustomerHandler
{
    private readonly ILogger<CustomerHandler> _logger;

    public CustomerHandler(ILogger<CustomerHandler> logger)
    {
        _logger = logger;
    }

    // nice thing about these handlers is that they are very easy to test
    public async Task<Customer> Handle(GetCustomer command, ConcordDbContext db)
    {
        _logger.LogInformation("Getting customer address {CustomerId}", command.Id);

        var customer = await db.Customers.FindAsync(command.Id);
        if (customer == null)
            return Customer.NotFound;

        return new Customer
        {
            Id = customer.Id,
            Address = customer.Address
        };
    }

    // transactional means that it will only send the customer updated message if the SQL Server update succeeds
    [Transactional]
    public async Task<Customer> Handle(UpdateCustomerAddress command, ConcordDbContext db, IMessageContext context)
    {
        _logger.LogInformation("Updating customer address in SQL {CustomerId}", command.Id);

        // this happens after the CSS action has succeeded
        // CSS is the source of truth
        // but we can still read our writes and act like a normal application

        var customer = await db.Customers.FindAsync(command.Id);
        if (customer == null)
            return Customer.NotFound;

        customer.Address = command.Address;

        await context.PublishAsync(new CustomerAddressUpdated
        {
            Id = customer.Id,
            Address = customer.Address
        });

        await db.SaveChangesAsync();

        return new Customer
        {
            Id = customer.Id,
            Address = customer.Address
        };
    }
}
