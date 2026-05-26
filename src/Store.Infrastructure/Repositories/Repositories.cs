using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Store.Domain.Entities;
using Store.Domain.Enums;
using Store.Domain.Interfaces;
using Store.Infrastructure.Persistence;

namespace Store.Infrastructure.Repositories;

public class OrderRepository(AppDbContext context) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Orders
            .Include(o => o.Buyer)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<(IEnumerable<Order> Items, int TotalCount)> GetAllAsync(
        OrderStatus? statusFilter,
        Guid? buyerIdFilter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Order> query = context.Orders
            .Include(o => o.Buyer)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .AsQueryable();

        if (statusFilter.HasValue)
            query = query.Where(o => o.Status == statusFilter.Value);

        if (buyerIdFilter.HasValue)
            query = query.Where(o => o.BuyerId == buyerIdFilter.Value);

        int total = await query.CountAsync(cancellationToken);

        List<Order> items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
        => await context.Orders.AddAsync(order, cancellationToken);

    public void Update(Order order)
        => context.Orders.Update(order);

    public void UpdateOrderOnly(Order order)
        => context.Entry(order).State = EntityState.Modified;

    public void Delete(Order order)
        => context.Orders.Remove(order);

    public async Task DeleteItemsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        await context.OrderItems
            .Where(i => i.OrderId == orderId)
            .ExecuteDeleteAsync(cancellationToken);

        List<EntityEntry<OrderItem>> trackedItems = context.ChangeTracker
            .Entries<OrderItem>()
            .Where(e => e.Entity.OrderId == orderId)
            .ToList();

        foreach (var entry in trackedItems)
            entry.State = EntityState.Detached;
    }

    public void RemoveItems(IEnumerable<OrderItem> items)
        => context.OrderItems.RemoveRange(items);

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Orders.AnyAsync(o => o.Id == id, cancellationToken);
}

public class BuyerRepository(AppDbContext context) : IBuyerRepository
{
    public async Task<Buyer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Buyers.FindAsync([id], cancellationToken);

    public async Task<Buyer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await context.Buyers.FirstOrDefaultAsync(b => b.Email == email, cancellationToken);

    public async Task AddAsync(Buyer buyer, CancellationToken cancellationToken = default)
        => await context.Buyers.AddAsync(buyer, cancellationToken);
}

public class ProductRepository(AppDbContext context) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Products.FindAsync([id], cancellationToken);

    public async Task<IEnumerable<Product>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        => await context.Products.Where(p => ids.Contains(p.Id)).ToListAsync(cancellationToken);

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
        => await context.Products.AddAsync(product, cancellationToken);
}