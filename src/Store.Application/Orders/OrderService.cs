using Store.Application.Common;
using Store.Application.Interfaces;
using Store.Application.Orders.DTOs;
using Store.Domain.Entities;
using Store.Domain.Exceptions;
using Store.Domain.Interfaces;

namespace Store.Application.Orders;

public class OrderService(
    IOrderRepository orderRepository,
    IBuyerRepository buyerRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork) : IOrderService
{

    public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        Buyer? buyer = await buyerRepository.GetByEmailAsync(request.BuyerEmail, cancellationToken);

        if (buyer is null)
        {
            buyer = new Buyer(request.BuyerName, request.BuyerEmail);
            await buyerRepository.AddAsync(buyer, cancellationToken);
        }

        Dictionary<Guid, Product> products = await ResolveProductsAsync(request.Items.Select(i => i.ProductId), cancellationToken);

        List<OrderItem> items = request.Items
            .Select(i => new OrderItem(i.ProductId, products[i.ProductId].Price, i.Quantity))
            .ToList();

        Order order = new(buyer.Id, items);

        await orderRepository.AddAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        Order? created = await orderRepository.GetByIdAsync(order.Id, cancellationToken);

        return OrderMapper.ToResponse(created!);
    }

    public async Task<OrderListResponse> GetAllOrdersAsync(OrderFilterRequest filter, CancellationToken cancellationToken = default)
    {
        int page = Math.Max(1, filter.Page);
        int pageSize = Math.Clamp(filter.PageSize, 1, 100);

        (IEnumerable<Order> Items, int TotalCount) = await orderRepository.GetAllAsync(
            filter.Status, filter.BuyerId, page, pageSize, cancellationToken);

        int totalPages = (int)Math.Ceiling(TotalCount / (double)pageSize);

        return new OrderListResponse(
            Items.Select(OrderMapper.ToResponse),
            TotalCount,
            page,
            pageSize,
            totalPages);
    }

    public async Task<OrderResponse> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Order order = await GetOrderOrThrowAsync(id, cancellationToken);
        return OrderMapper.ToResponse(order);
    }

    public async Task<OrderResponse> UpdateOrderAsync(Guid id, UpdateOrderRequest request, CancellationToken cancellationToken = default)
    {
        Order order = await GetOrderOrThrowAsync(id, cancellationToken);

        Dictionary<Guid, Product> products = await ResolveProductsAsync(request.Items.Select(i => i.ProductId), cancellationToken);

        List<OrderItem> newItems = request.Items
            .Select(i => new OrderItem(i.ProductId, products[i.ProductId].Price, i.Quantity))
            .ToList();

        order.UpdateItems(newItems);

        await orderRepository.DeleteItemsByOrderIdAsync(id, cancellationToken);

        orderRepository.UpdateOrderOnly(order);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        Order? updated = await orderRepository.GetByIdAsync(id, cancellationToken);

        return OrderMapper.ToResponse(updated!);
    }

    public async Task DeleteOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Order? order = await GetOrderOrThrowAsync(id, cancellationToken);

        orderRepository.Delete(order);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<OrderResponse> CancelOrderAsync(Guid id, CancellationToken cancellationToken = default)
        => await TransitionOrderAsync(id, o => o.Cancel(), cancellationToken);

    public async Task<OrderResponse> ProcessOrderAsync(Guid id, CancellationToken cancellationToken = default)
        => await TransitionOrderAsync(id, o => o.Process(), cancellationToken);

    public async Task<OrderResponse> ShipOrderAsync(Guid id, CancellationToken cancellationToken = default)
        => await TransitionOrderAsync(id, o => o.Ship(), cancellationToken);

    private async Task<OrderResponse> TransitionOrderAsync(Guid id, Action<Order> transition, CancellationToken cancellationToken)
    {
        Order? order = await GetOrderOrThrowAsync(id, cancellationToken) 
            ?? throw new OrderNotFoundException(id);

        transition(order);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        Order? updated = await orderRepository.GetByIdAsync(id, cancellationToken);

        return OrderMapper.ToResponse(updated!);
    }

    private async Task<Order> GetOrderOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        Order? order = await orderRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new OrderNotFoundException(id);

        return order;
    }

    private async Task<Dictionary<Guid, Product>> ResolveProductsAsync(
        IEnumerable<Guid> productIds,
        CancellationToken cancellationToken)
    {
        List<Guid> ids = productIds
            .Distinct()
            .ToList();

        Dictionary<Guid, Product> products = (await productRepository.GetByIdsAsync(ids, cancellationToken))
            .ToDictionary(p => p.Id);

        Guid missing = ids
            .FirstOrDefault(id => !products.ContainsKey(id));

        if (missing != Guid.Empty && !products.ContainsKey(missing))
            throw new KeyNotFoundException($"Product '{missing}' not found.");

        return products;
    }
}