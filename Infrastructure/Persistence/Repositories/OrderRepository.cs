using Application.Features.Order.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class OrderRepository : OrderRepositoryContract
{
    private readonly ShopDbContext _context;

    public OrderRepository(ShopDbContext context)
    {
        this._context = context;
    }

    public async Task<Order?> GetOrderByIdAsync(Guid orderId, Guid userId)
    {
        return await _context
            .Orders
            .Include(o => o.OrderItems)
            .Where(o => o.Id == orderId && o.UserId == userId)
            .FirstOrDefaultAsync();
    }

    public async Task CreateOrderAsync(Order order)
    {
        await _context.AddAsync(order);
    }

    public void UpdateOrder(Order order)
    {
        _context.Update(order);
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<List<Order>?> GetAllOrders()
    {
        return await _context
            .Orders
            .Include(o => o.OrderItems)
            .ToListAsync();
    }

    public async Task<List<Order>?> GetOrderByUserIdAsync(Guid userId)
    {
        return await _context
            .Orders
            .Include(o => o.OrderItems)
            .Where(o => o.UserId == userId)
            .ToListAsync();
    }

    public async Task<Order?> GetOrderByIdAsync(Guid paymentOrderId)
    {
        return await _context
            .Orders
            .Include(o => o.OrderItems)
            .Where(o => o.Id == paymentOrderId)
            .FirstOrDefaultAsync();
    }
}