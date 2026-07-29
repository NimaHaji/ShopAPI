using Application.Features.Order.Interfaces;
using Application.Features.Product.DTOs;
using Application.Features.Product.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ProductRepository : ProductRepositoryContract
{
    private readonly ShopDbContext _context;

    public ProductRepository(ShopDbContext context)
    {
        this._context = context;
    }

    public async Task<List<Product>> GetProductList(ProductQueryDto query)
    {
        var products = _context
            .Products
            .Include(x=>x.Category)
            .Include(x=>x.Brand)
            .Include(x=>x.InventoryItem)
            .AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            products = products.Where(c => c.Title.Contains(query.Q));
        }

        if (query.CategoryId.HasValue)
        {
            products = products.Where(p => p.CategoryId == query.CategoryId);
        }

        if (query.BrandId.HasValue)
        {
            products = products.Where(p => p.BrandId == query.BrandId);
        }

        if (query.MinPrice.HasValue)
        {
            products = products.Where(p => p.Price >= query.MinPrice);
        }

        if (query.MaxPrice.HasValue)
        {
            products = products.Where(p => p.Price <= query.MaxPrice);
        }

        if (query.SortBy.HasValue)
        {
            switch (query.SortBy)
            {
                case SortByType.PriceHigh:
                    products = products.OrderByDescending(p => p.Price);
                    break;

                case SortByType.PriceLow:
                    products = products.OrderBy(p => p.Price);
                    break;

                case SortByType.NewestArrived:
                    products = products.OrderByDescending(p => p.AddedAt);
                    break;

                // case SortByType.BestSellers
            }
        }

        query.Page = Math.Max(query.Page, 1);
        query.PageSize = Math.Clamp(query.PageSize, 1, 100);

        return await products
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();
    }

    public async Task<bool> IsExistingProduct(string productName)
    {
        return await _context.Products.Where(p => p.Title == productName).AnyAsync();
    }

    public async Task<Product?> GetProductByIdAsync(Guid productId)
    {
        return await _context
            .Products
            .Where(p => p.Id == productId)
            .FirstOrDefaultAsync();
    }

    public async Task CreateProductAsync(Product product)
    {
        await _context.Products.AddAsync(product);
    }

    public async Task<List<Product>> GetProductsByIdsAsync(List<Guid> productIds)
    {
        return await _context
            .Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();
    }

    public async Task<List<ProductCategory>> GetAllProductCategories()
    {
        return await _context
            .ProductCategories
            .OrderByDescending(x => x.Title)
            .ToListAsync();
    }

    public async Task<bool> IsExistingProductCategory(string dtoTitle)
    {
        return await _context.ProductCategories.Where(c => c.Title == dtoTitle).AnyAsync();
    }

    public async Task AddProductCategory(ProductCategory category)
    {
        await _context.ProductCategories.AddAsync(category);
    }

    public async Task<List<Product>?> SearchProductWithTitle(string query)
    {
        return await _context
            .Products
            .Include(x => x.Category)
            .Where(p => p.Title.Contains(query)).ToListAsync();
    }
}