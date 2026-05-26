using Store.Domain.Entities;
using Store.Domain.Enums;
using Store.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace Store.UnitTests.Domain;

public class OrderTests
{
    private static readonly Guid ValidBuyerId = Guid.NewGuid();
    private static readonly Guid ValidProductId = Guid.NewGuid();

    private static List<OrderItem> CreateItems(int count = 1)
        => Enumerable.Range(1, count)
            .Select(_ => new OrderItem(ValidProductId, 99.99m, 1))
            .ToList();

    [Fact]
    public void Constructor_WithValidData_ShouldCreateOrderWithInitiatedStatus()
    {
        var order = new Order(ValidBuyerId, CreateItems());

        order.Should().NotBeNull();
        order.Status.Should().Be(OrderStatus.Initiated);
        order.BuyerId.Should().Be(ValidBuyerId);
        order.Items.Should().HaveCount(1);
    }

    [Fact]
    public void Constructor_WithEmptyBuyerId_ShouldThrowArgumentException()
    {
        var act = () => new Order(Guid.Empty, CreateItems());

        act.Should().Throw<ArgumentException>().WithMessage("*BuyerId*");
    }

    [Fact]
    public void Constructor_WithNoItems_ShouldThrowOrderMustHaveProductsException()
    {
        var act = () => new Order(ValidBuyerId, new List<OrderItem>());

        act.Should().Throw<OrderMustHaveProductsException>();
    }

    [Fact]
    public void Process_WhenInitiated_ShouldChangeStatusToProcessed()
    {
        var order = new Order(ValidBuyerId, CreateItems());

        order.Process();

        order.Status.Should().Be(OrderStatus.Processed);
        order.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Process_WhenNotInitiated_ShouldThrowInvalidOrderTransitionException()
    {
        var order = new Order(ValidBuyerId, CreateItems());
        order.Process();

        var act = () => order.Process();

        act.Should().Throw<InvalidOrderTransitionException>();
    }

    [Fact]
    public void Ship_WhenProcessed_ShouldChangeStatusToShipped()
    {
        var order = new Order(ValidBuyerId, CreateItems());
        order.Process();

        order.Ship();

        order.Status.Should().Be(OrderStatus.Shipped);
    }

    [Fact]
    public void Ship_WhenNotProcessed_ShouldThrowInvalidOrderTransitionException()
    {
        var order = new Order(ValidBuyerId, CreateItems());

        var act = () => order.Ship();

        act.Should().Throw<InvalidOrderTransitionException>();
    }

    [Fact]
    public void Cancel_WhenInitiated_ShouldChangeStatusToCancelled()
    {
        var order = new Order(ValidBuyerId, CreateItems());

        order.Cancel();

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenProcessed_ShouldChangeStatusToCancelled()
    {
        var order = new Order(ValidBuyerId, CreateItems());
        order.Process();

        order.Cancel();

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenShipped_ShouldThrowInvalidOrderTransitionException()
    {
        var order = new Order(ValidBuyerId, CreateItems());
        order.Process();
        order.Ship();

        var act = () => order.Cancel();

        act.Should().Throw<InvalidOrderTransitionException>();
    }

    [Fact]
    public void UpdateItems_WhenInitiated_ShouldReplaceItems()
    {
        var order = new Order(ValidBuyerId, CreateItems(1));
        var newItems = CreateItems(3);

        order.UpdateItems(newItems);

        order.Items.Should().HaveCount(3);
    }

    [Fact]
    public void UpdateItems_WhenNotInitiated_ShouldThrowOrderCannotBeModifiedException()
    {
        var order = new Order(ValidBuyerId, CreateItems());
        order.Process();

        var act = () => order.UpdateItems(CreateItems(2));

        act.Should().Throw<OrderCannotBeModifiedException>();
    }

    [Fact]
    public void TotalAmount_ShouldSumAllItemTotalPrices()
    {
        var items = new List<OrderItem>
        {
            new OrderItem(ValidProductId, 10.00m, 2),  // 20.00
            new OrderItem(ValidProductId, 5.50m, 4)    // 22.00
        };

        var order = new Order(ValidBuyerId, items);

        order.TotalAmount.Should().Be(42.00m);
    }
}
