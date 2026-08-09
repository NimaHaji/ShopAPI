using Application.Features.Coupon.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class CouponRepository : CouponRepositoryContract
{
    private readonly ShopDbContext _shopDbContext;

    public CouponRepository(ShopDbContext shopDbContext)
    {
        _shopDbContext = shopDbContext;
    }

    public async Task<List<Coupon>> GetAllDiscountsForAdminAsync()
    {
        return await _shopDbContext
            .Coupons
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Coupon?> GetCouponByIdForAdminAsync(Guid couponId)
    {
        return await _shopDbContext
            .Coupons
            .Where(c => c.Id == couponId)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> IsCouponCodeExistAsync(string dtoCode)
    {
        return await _shopDbContext
            .Coupons
            .AnyAsync(c => c.Code == dtoCode);
    }

    public async Task CreatCouponAsync(Coupon coupon)
    {
        await _shopDbContext
            .Coupons
            .AddAsync(coupon);
    }

    public async Task<Coupon?> GetCouponByCodeAsync(string dtoCode)
    {
        return await _shopDbContext
            .Coupons
            .Include(c=>c.CouponUsages)
            .Where(c => !c.IsDeleted &&
                        c.Code == dtoCode)
            .FirstOrDefaultAsync();
    }
    public async Task<int> GetUserCouponUsageCountAsync(
        Guid couponId,
        Guid userId)
    {
        return await _shopDbContext
            .CouponUsages
            .CountAsync(x =>
                x.CouponId == couponId &&
                x.UserId == userId);
    }

    public async Task<Coupon?> GetCouponByIdAsync(Guid orderCouponId)
    {
        return await _shopDbContext
            .Coupons
            .Where(c => c.Id == orderCouponId)
            .FirstOrDefaultAsync();
    }
}