using Application.Features.Discount.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class DiscountRepository : DiscountRepositoryContract
{
    private readonly ShopDbContext _shopDbContext;

    public DiscountRepository(ShopDbContext shopDbContext)
    {
        _shopDbContext = shopDbContext;
    }

    public async Task<List<Discount>?> GetAllDiscountAsync()
    {
        return await _shopDbContext
            .Discounts
            .Where(x => !x.IsDeleted)
            .ToListAsync();
    }

    public async Task<List<Discount>?> GetAllActiveDiscountAsync()
    {
        return await _shopDbContext
            .Discounts
            .Where(x => !x.IsDeleted && x.IsActive)
            .ToListAsync();
    }

    public async Task<Discount?> GetActiveDiscountByIdAsync(Guid discountId)
    {
        return await _shopDbContext
            .Discounts
            .Where(d => !d.IsDeleted && d.IsActive)
            .FirstOrDefaultAsync(d => d.Id == discountId);
    }

    public async Task<Discount?> GetDiscountByIdAsync(Guid discountId)
    {
        return await _shopDbContext
            .Discounts
            .Where(d => !d.IsDeleted && d.Id == discountId)
            .FirstOrDefaultAsync();
    }
    public async Task<Discount?> GetDiscountForAdminByIdAsync(Guid discountId)
    {
        return await _shopDbContext
            .Discounts
            .Where(d => d.Id == discountId)
            .FirstOrDefaultAsync();
    }
    public async Task CreateDiscountAsync(Discount discount)
    {
        await _shopDbContext
            .Discounts
            .AddAsync(discount);
    }

}