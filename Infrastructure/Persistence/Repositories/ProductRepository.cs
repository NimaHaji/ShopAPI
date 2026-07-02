using Application.Features.Order.Interfaces;
using Application.Features.Product.Interfaces;
using Domain.Entites;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ProductRepository:ProductRepositoryContract
{
    private readonly ShopDbContext _context;

    public ProductRepository(ShopDbContext context)
    {
        this._context = context;
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

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}