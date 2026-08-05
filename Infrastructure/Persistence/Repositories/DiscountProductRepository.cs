using Application.Features.Discount.DTOs;
using Application.Features.Discount.Interfaces;
using Application.Features.DiscountProduct.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Shared.Exceptions;

namespace Infrastructure.Persistence.Repositories;

public class DiscountProductRepository : DiscountProductRepositoryContract
{
    private readonly ShopDbContext _shopDbContext;

    public DiscountProductRepository(ShopDbContext shopDbContext)
    {
        _shopDbContext = shopDbContext;
    }

    public async Task AddProductToDiscountAsync(DiscountProduct discountProduct)
    {
        await _shopDbContext
            .DiscountProducts
            .AddAsync(discountProduct);
    }

    public async Task<List<Guid>> GetExistingDiscountProductsAsync(Guid discountId, List<Guid> productIds)
    {
        return await _shopDbContext
            .DiscountProducts
            .Where(x =>
                x.DiscountId == discountId &&
                productIds.Contains(x.ProductId))
            .Select(x => x.ProductId)
            .ToListAsync();
    }

    public async Task<DiscountProduct?> GetDiscountProductAsync(Guid discountId, Guid productId)
    {
        return await _shopDbContext.DiscountProducts
            .FirstOrDefaultAsync(x =>
                x.DiscountId == discountId &&
                x.ProductId == productId);
    }

    public Task RemoveAsync(DiscountProduct discountProduct)
    {
        _shopDbContext.DiscountProducts.Remove(discountProduct);

        return Task.CompletedTask;
    }

    public async Task<Discount?> GetDiscountByProductIdAsync(Guid productId)
    {
        return await _shopDbContext
            .DiscountProducts
            .Where(x => x.ProductId == productId)
            .Select(x=>x.Discount)
            .FirstOrDefaultAsync();
    }
}