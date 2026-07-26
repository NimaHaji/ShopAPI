using Application.Features.Order.Interfaces;
using Application.Features.Product.Interfaces;
using Domain.Entities;
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

    public async Task<List<Product>?> GetAllProducts()
    {
        return await _context.Products.ToListAsync();
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
}