namespace ConcordServicing.Data.Messages;

public class GetCustomerAddress
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


public class CustomerAddress
{
    public bool Success { get; private set; } = true;
    public required string Id { get; set; }
    public required string Address { get; set; }

    public static CustomerAddress NotFound => new() { Success = false, Id = String.Empty, Address = String.Empty };
}