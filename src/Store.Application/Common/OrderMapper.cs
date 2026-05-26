using Store.Application.Orders.DTOs;
using Store.Domain.Entities;

namespace Store.Application.Common;

public static class OrderMapper
{
    public static OrderResponse ToResponse(Order order)
    {
        return new OrderResponse(
            order.Id,
            new BuyerResponse(order.Buyer.Id, order.Buyer.Name, order.Buyer.Email),
            order.Status.ToString(),
            order.Items.Select(i => new OrderItemResponse(
                i.Id,
                i.ProductId,
                i.Product?.Name ?? string.Empty,
                i.UnitPrice,
                i.Quantity,
                i.TotalPrice)),
            order.TotalAmount,
            order.CreatedAt,
            order.UpdatedAt);
    }
}
