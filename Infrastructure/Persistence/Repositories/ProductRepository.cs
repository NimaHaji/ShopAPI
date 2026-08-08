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

    #region product

    public async Task<List<Product>?> GetProductList(ProductQueryDto query)
    {
        var products = _context
            .Products
            .Include(x => x.Category)
            .Include(x => x.Brand)
            .Include(x => x.InventoryItem)
            .Include(x=>x.Images)
            .Include(x=>x.Reviews)
            .Include(x=>x.DiscountProducts)
            .ThenInclude(x=>x.Discount)
            .Where(p => !p.IsDeleted)
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
        return await _context
            .Products
            .Where(p => !p.IsDeleted && p.Title == productName)
            .AnyAsync();
    }

    public async Task<Product?> GetProductByIdAsync(Guid productId)
    {
        return await _context
            .Products
            .Include(x => x.Category)
            .Include(x => x.Brand)
            .Include(x => x.Images)
            .Include(x => x.InventoryItem)
            .Where(p => !p.IsDeleted && p.Id == productId)
            .FirstOrDefaultAsync();
    }

    public async Task CreateProductAsync(Product product)
    {
        await _context
            .Products
            .AddAsync(product);
    }

    public async Task<List<Product>> GetProductsByIdsAsync(List<Guid> productIds)
    {
        return await _context
            .Products
            .Where(p => !p.IsDeleted && productIds.Contains(p.Id))
            .ToListAsync();
    }

    public async Task<List<Product>?> SearchProductWithTitle(string query)
    {
        return await _context
            .Products
            .Include(x => x.Category)
            .Where(p => !p.IsDeleted && p.Title.Contains(query)).ToListAsync();
    }

    #endregion

    #region Category

    public async Task<List<ProductCategory>> GetAllProductCategories()
    {
        return await _context
            .ProductCategories
            .Where(c => !c.IsDeleted)
            .OrderByDescending(x => x.Title)
            .ToListAsync();
    }

    public async Task<bool> IsExistingProductCategory(string dtoTitle)
    {
        return await _context
            .ProductCategories
            .Where(c => !c.IsDeleted && c.Title == dtoTitle)
            .AnyAsync();
    }

    public async Task AddProductCategory(ProductCategory category)
    {
        await _context
            .ProductCategories
            .AddAsync(category);
    }

    public async Task<ProductCategory?> GetProductCategoryById(Guid productCategoryId)
    {
        return await _context
            .ProductCategories
            .Where(c => !c.IsDeleted && c.Id == productCategoryId)
            .FirstOrDefaultAsync();
    }

    public async Task<List<ProductCategory>?> SearchProductCategoriesWithTitle(string dtoTitle)
    {
        return await _context
            .ProductCategories
            .Where(c => !c.IsDeleted && c.Title.Contains(dtoTitle))
            .ToListAsync();
    }

    public async Task<List<ProductBrand>> GetAllBrandAsync()
    {
        return await _context
            .ProductBrands
            .Where(b=>!b.IsDeleted)
            .ToListAsync();
    }

    public async Task<bool> IsExistingBrand(string dtoTitle)
    {
        return await _context
            .ProductBrands
            .Where(b => !b.IsDeleted && b.Title == dtoTitle)
            .AnyAsync();
    }

    public async Task AddBrandAsync(ProductBrand brand)
    {
        await _context
            .ProductBrands
            .AddAsync(brand);
    }

    public async Task<ProductBrand?> GetProductBrandById(Guid productBrandId)
    {
        return await _context
            .ProductBrands
            .Where(b => !b.IsDeleted && b.Id == productBrandId)
            .FirstOrDefaultAsync();
    }

    public async Task<List<ProductBrand>?> SearchProductBrandsWithTitle(string dtoTitle)
    {
        return await _context
            .ProductBrands
            .Where(b => !b.IsDeleted && b.Title.Contains(dtoTitle))
            .ToListAsync();
    }

    public async Task<List<Product>> GetProductsWithDiscountByIdsAsync(List<Guid> productIds)
    {
        return await _context
            .Products
            .Include(p=>p.DiscountProducts)
            .ThenInclude(d=>d.Discount)
            .Where(p => !p.IsDeleted && productIds.Contains(p.Id))
            .ToListAsync();
    }

    #endregion
}