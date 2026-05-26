namespace Store.Domain.Exceptions;

public abstract class DomainException(string message) : Exception(message) {}

public class OrderNotFoundException(Guid id) : DomainException($"Order with ID '{id}' was not found.") {}

public class InvalidOrderTransitionException(string from, string to) : DomainException($"Cannot transition order from '{from}' to '{to}'.") {}

public class OrderCannotBeModifiedException : DomainException
{
    public OrderCannotBeModifiedException()
        : base("Only orders with status 'Initiated' can be modified.") { }
}

public class OrderMustHaveProductsException : DomainException
{
    public OrderMustHaveProductsException()
        : base("An order must have at least one product.") { }
}
