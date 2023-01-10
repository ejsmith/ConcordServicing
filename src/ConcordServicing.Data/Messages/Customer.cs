namespace ConcordServicing.Data.Messages;

public class GetCustomer
{
    public required string Id { get; set; }
}

public class UpdateCustomerAddress
{
    public required string Id { get; set; }
    public required string Address { get; set; }
}

public class CustomerAddressUpdated
{
    public required string Id { get; set; }
    public required string Address { get; set; }
}


public class Customer
{
    public bool Success { get; private set; } = true;
    public required string Id { get; set; }
    public required string Address { get; set; }

    public static Customer NotFound => new() { Success = false, Id = String.Empty, Address = String.Empty };
}
