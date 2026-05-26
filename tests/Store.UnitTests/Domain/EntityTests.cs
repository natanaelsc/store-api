using FluentAssertions;
using Store.Domain.Entities;
using Xunit;

namespace Store.UnitTests.Domain;

public class OrderItemTests
{
    private static readonly Guid ValidProductId = Guid.NewGuid();

    [Fact]
    public void Constructor_WithValidData_ShouldCreateOrderItem()
    {
        OrderItem item = new(ValidProductId, 49.99m, 3);

        item.ProductId.Should().Be(ValidProductId);
        item.UnitPrice.Should().Be(49.99m);
        item.Quantity.Should().Be(3);
        item.TotalPrice.Should().Be(149.97m);
    }

    [Fact]
    public void Constructor_WithZeroQuantity_ShouldThrowArgumentException()
    {
        var act = () => new OrderItem(ValidProductId, 49.99m, 0);

        act.Should().Throw<ArgumentException>().WithMessage("*Quantity*");
    }

    [Fact]
    public void Constructor_WithNegativePrice_ShouldThrowArgumentException()
    {
        var act = () => new OrderItem(ValidProductId, -1m, 1);

        act.Should().Throw<ArgumentException>().WithMessage("*price*");
    }
}

public class BuyerTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateBuyer()
    {
        Buyer buyer = new("John Doe", "john@example.com");

        buyer.Name.Should().Be("John Doe");
        buyer.Email.Should().Be("john@example.com");
        buyer.Id.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("", "email@test.com")]
    [InlineData("  ", "email@test.com")]
    public void Constructor_WithEmptyName_ShouldThrowArgumentException(string name, string email)
    {
        var act = () => new Buyer(name, email);

        act.Should().Throw<ArgumentException>().WithMessage("*name*");
    }

    [Theory]
    [InlineData("John", "")]
    [InlineData("John", "   ")]
    public void Constructor_WithEmptyEmail_ShouldThrowArgumentException(string name, string email)
    {
        var act = () => new Buyer(name, email);

        act.Should().Throw<ArgumentException>().WithMessage("*email*");
    }
}

public class ProductTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateProduct()
    {
        Product product = new("Laptop", 2999.99m);

        product.Name.Should().Be("Laptop");
        product.Price.Should().Be(2999.99m);
        product.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_WithZeroPrice_ShouldThrowArgumentException()
    {
        var act = () => new Product("Laptop", 0m);

        act.Should().Throw<ArgumentException>().WithMessage("*price*");
    }

    [Fact]
    public void Constructor_WithEmptyName_ShouldThrowArgumentException()
    {
        var act = () => new Product("", 100m);

        act.Should().Throw<ArgumentException>().WithMessage("*name*");
    }
}
