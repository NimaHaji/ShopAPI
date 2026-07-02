using Application.Features.Order.Interfaces;
using Domain.Entites;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class OrderRepository:OrderRepositoryContract
{
    private readonly ShopDbContext context;

    public OrderRepository(ShopDbContext context)
    {
        this.context = context;
    }

    public async Task<Order?> GetOrderByIdAsync(Guid orderId)
    {
        return await context
            .Orders
            .Where(o => o.Id == orderId)
            .FirstOrDefaultAsync();
    }

    public async Task CreateOrderAsync(Order order)
    {
        await context.AddAsync(order);
    }

    public async Task SaveAsync()
    {
        await context.SaveChangesAsync();
    }
}