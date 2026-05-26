using FluentAssertions;
using NSubstitute;
using Store.Application.Orders;
using Store.Application.Orders.DTOs;
using Store.Domain.Entities;
using Store.Domain.Exceptions;
using Store.Domain.Interfaces;
using Xunit;

namespace Store.UnitTests.Application;

public class OrderServiceTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly IBuyerRepository _buyerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _buyerRepository = Substitute.For<IBuyerRepository>();
        _productRepository = Substitute.For<IProductRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _sut = new OrderService(
            _orderRepository,
            _buyerRepository,
            _productRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task GetOrderByIdAsync_WhenOrderNotFound_ShouldThrowOrderNotFoundException()
    {
        Guid id = Guid.NewGuid();

        _orderRepository.GetByIdAsync(id, default).Returns((Order?)null);

        var act = async () => await _sut.GetOrderByIdAsync(id);

        await act.Should().ThrowAsync<OrderNotFoundException>();
    }

    [Fact]
    public async Task CancelOrderAsync_WhenOrderNotFound_ShouldThrowOrderNotFoundException()
    {
        Guid id = Guid.NewGuid();

        _orderRepository.GetByIdAsync(id, default).Returns((Order?)null);

        var act = async () => await _sut.CancelOrderAsync(id);

        await act.Should().ThrowAsync<OrderNotFoundException>();
    }

    [Fact]
    public async Task DeleteOrderAsync_WhenOrderExists_ShouldCallDeleteAndSave()
    {
        Guid productId = Guid.NewGuid();
        Guid buyerId = Guid.NewGuid();

        List<OrderItem> items = [
            new OrderItem(productId, 10m, 1)
        ];

        Order order = new(buyerId, items);

        _orderRepository.GetByIdAsync(order.Id, default).Returns(order);

        await _sut.DeleteOrderAsync(order.Id);

        _orderRepository.Received(1).Delete(order);

        await _unitOfWork.Received(1).SaveChangesAsync(default);
    }

    [Fact]
    public async Task GetAllOrdersAsync_ShouldReturnPaginatedResponse()
    {
        OrderFilterRequest filter = new(null, null, 1, 10);

        _orderRepository.GetAllAsync(null, null, 1, 10, default)
            .Returns((new List<Order>(), 0));

        OrderListResponse? result = await _sut.GetAllOrdersAsync(filter);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(0);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(0);
    }
}
