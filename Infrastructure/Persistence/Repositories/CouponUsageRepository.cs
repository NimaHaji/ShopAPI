using Application.Features.CouponUsage.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class CouponUsageRepository : CouponUsageRepositoryContract
{
    private readonly ShopDbContext _shopDbContext;

    public CouponUsageRepository(ShopDbContext shopDbContext)
    {
        _shopDbContext = shopDbContext;
    }

    public async Task CreateCouponUsage(CouponUsage couponUsage)
    {
        await _shopDbContext
            .CouponUsages
            .AddAsync(couponUsage);
    }

    public async Task<bool> IsExistCouponUsageByOrderId(Guid orderId)
    {
        return await _shopDbContext
            .CouponUsages
            .AnyAsync(cp => cp.OrderId == orderId);
    }
}