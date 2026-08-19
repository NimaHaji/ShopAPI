using Application.Features.DiscountVariant.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class DiscountVariantRepository:DiscountVariantRepositoryContract
{
    private readonly ShopDbContext _shopDbContext;

    public DiscountVariantRepository(ShopDbContext shopDbContext)
    {
        _shopDbContext = shopDbContext;
    }

    public async Task<DiscountVariant?> GetDiscountProductAsync(Guid discountId, Guid productVariantId)
    {
        return await _shopDbContext
            .DiscountVariants
            .Where(d => d.ProductVariantId == productVariantId && d.DiscountId == discountId)
            .FirstOrDefaultAsync();
    }

    public async Task RemoveAsync(DiscountVariant discountVariant)
    {
        _shopDbContext.DiscountVariants.Remove(discountVariant);
    }

    public async Task<Discount?> GetDiscountByProductVariantIdAsync(Guid productVariantId)
    {
        return await _shopDbContext
            .DiscountVariants
            .Where(d => d.ProductVariantId == productVariantId)
            .Select(dv=>dv.Discount)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Guid>> GetExistingDiscountVariantsAsync(Guid discountId, List<Guid> productVariantIds)
    {
        return await _shopDbContext
            .DiscountVariants
            .Where(x =>
                x.DiscountId == discountId &&
                productVariantIds.Contains(x.ProductVariantId))
            .Select(x => x.ProductVariantId)
            .ToListAsync();
    }

    public async Task AddProductToDiscountAsync(DiscountVariant discountVariant)
    {
        await _shopDbContext
            .DiscountVariants
            .AddAsync(discountVariant);
    }
}