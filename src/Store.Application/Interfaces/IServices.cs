using Store.Application.Orders.DTOs;

namespace Store.Application.Interfaces;

public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task<OrderListResponse> GetAllOrdersAsync(OrderFilterRequest filter, CancellationToken cancellationToken = default);
    Task<OrderResponse> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrderResponse> UpdateOrderAsync(Guid id, UpdateOrderRequest request, CancellationToken cancellationToken = default);
    Task DeleteOrderAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrderResponse> CancelOrderAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrderResponse> ProcessOrderAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrderResponse> ShipOrderAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IProductService
{
    Task<Guid> CreateProductAsync(string name, decimal price, CancellationToken cancellationToken = default);
}
