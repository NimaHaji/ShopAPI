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

    public async Task<List<ViewProductItemDto>> GetProductList(ProductQueryDto query)
    {
        var products = _context
            .Products
            .Join(_context.InventoryItems,
                product => product.Id,
                inventoryItem => inventoryItem.ProductId,
                (product, inventory) => new { product, inventory });

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            products = products.Where(c => c.product.Title.Contains(query.Q));
        }

        if (query.CategoryId.HasValue)
        {
            products = products.Where(p => p.product.CategoryId == query.CategoryId);
        }

        if (query.BrandId.HasValue)
        {
            products = products.Where(p => p.product.BrandId == query.BrandId);
        }

        if (query.MinPrice.HasValue)
        {
            products = products.Where(p => p.product.Price >= query.MinPrice);
        }

        if (query.MaxPrice.HasValue)
        {
            products = products.Where(p => p.product.Price <= query.MaxPrice);
        }

        if (query.SortBy.HasValue)
        {
            switch (query.SortBy)
            {
                case SortByType.PriceHigh:
                    products = products.OrderByDescending(p => p.product.Price);
                    break;
                
                case SortByType.PriceLow:
                    products = products.OrderBy(p => p.product.Price);
                    break;
                
                case SortByType.NewestArrived:
                    products = products.OrderByDescending(p => p.product.AddedAt);
                    break;
                
                // case SortByType.BestSellers
            }
        }
        
        query.Page = Math.Max(query.Page, 1);
        query.PageSize = Math.Clamp(query.PageSize, 1, 100);
        
        return await products
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(pi => new ViewProductItemDto
                {
                    Title = pi.product.Title,
                    Category = pi.product.Category.Title,
                    Brand = pi.product.Brand.Title,
                    Description = pi.product.Description,
                    DiscountPercentage = pi.product.DiscountPercentage ?? null,
                    Price = pi.product.Price,
                    Stock = pi.inventory.StockQuantity - pi.inventory.ReservedQuantity,
                }
            )
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

    public async Task<List<ViewProductCategoryItemDto>> GetAllProductCategories()
    {
        return await _context
            .ProductCategories
            .OrderByDescending(x => x.Title)
            .Select(p => new ViewProductCategoryItemDto
            {
                Title = p.Title
            })
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

    public async Task<List<SearchProductItemsResultDto>?> SearchProductWithTitle(string query)
    {
        return await _context
            .Products
            .Where(p => p.Title.Contains(query))
            .Select(p => new SearchProductItemsResultDto
            {
                Title = p.Title,
                Category = p.Category.Title,
            }).ToListAsync();
    }
}