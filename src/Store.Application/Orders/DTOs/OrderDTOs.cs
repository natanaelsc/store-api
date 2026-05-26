using Store.Domain.Enums;

namespace Store.Application.Orders.DTOs;

public record OrderItemRequest(
    Guid ProductId,
    int Quantity);

public record CreateOrderRequest(
    string BuyerName,
    string BuyerEmail,
    IEnumerable<OrderItemRequest> Items);

public record UpdateOrderRequest(
    IEnumerable<OrderItemRequest> Items);

public record CreateProductRequest(
    string Name,
    decimal Price);

public record OrderItemResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice);

public record BuyerResponse(
    Guid Id,
    string Name,
    string Email);

public record OrderResponse(
    Guid Id,
    BuyerResponse Buyer,
    string Status,
    IEnumerable<OrderItemResponse> Items,
    decimal TotalAmount,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record OrderListResponse(
    IEnumerable<OrderResponse> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

public record OrderFilterRequest(
    OrderStatus? Status = null,
    Guid? BuyerId = null,
    int Page = 1,
    int PageSize = 10);
