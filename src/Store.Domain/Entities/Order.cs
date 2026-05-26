using Store.Domain.Enums;
using Store.Domain.Exceptions;

namespace Store.Domain.Entities;

public class Order : BaseEntity
{
    private readonly List<OrderItem> _items = new();

    public Guid BuyerId { get; private set; }
    public Buyer Buyer { get; private set; } = null!;
    public OrderStatus Status { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public decimal TotalAmount => _items.Sum(i => i.TotalPrice);

    // EF Core constructor
    private Order() { }

    public Order(Guid buyerId, IEnumerable<OrderItem> items)
    {
        if (buyerId == Guid.Empty)
            throw new ArgumentException("BuyerId cannot be empty.", nameof(buyerId));

        BuyerId = buyerId;
        Status = OrderStatus.Initiated;

        List<OrderItem> itemList = items?.ToList() ?? [];

        if (itemList.Count == 0)
            throw new OrderMustHaveProductsException();

        foreach (OrderItem item in itemList)
        {
            item.SetOrderId(Id);
            _items.Add(item);
        }
    }

    public void Process()
    {
        EnsureStatus(OrderStatus.Initiated, OrderStatus.Processed);
        Status = OrderStatus.Processed;
        MarkUpdated();
    }

    public void Ship()
    {
        EnsureStatus(OrderStatus.Processed, OrderStatus.Shipped);
        Status = OrderStatus.Shipped;
        MarkUpdated();
    }

    public void Cancel()
    {
        if (Status != OrderStatus.Initiated && 
            Status != OrderStatus.Processed)
            throw new InvalidOrderTransitionException(Status.ToString(), OrderStatus.Cancelled.ToString());

        Status = OrderStatus.Cancelled;
        MarkUpdated();
    }

    public void UpdateItems(IEnumerable<OrderItem> newItems)
    {
        if (Status != OrderStatus.Initiated)
            throw new OrderCannotBeModifiedException();

        List<OrderItem> itemList = newItems?.ToList() ?? [];

        if (itemList.Count == 0)
            throw new OrderMustHaveProductsException();

        _items.Clear();

        foreach (OrderItem item in itemList)
        {
            item.SetOrderId(Id);
            _items.Add(item);
        }

        MarkUpdated();
    }

    private void EnsureStatus(OrderStatus required, OrderStatus target)
    {
        if (Status != required)
            throw new InvalidOrderTransitionException(Status.ToString(), target.ToString());
    }
}
