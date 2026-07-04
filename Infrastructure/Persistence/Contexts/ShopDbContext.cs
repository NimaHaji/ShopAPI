using System.Reflection;
using Domain.Entities;
using Infrastructure.Persistence.Mappings;
using Microsoft.EntityFrameworkCore;
using OrderItem = Domain.Entities.OrderItem;

namespace Infrastructure.Persistence.Contexts; 

public class ShopDbContext:DbContext
{
    public ShopDbContext(DbContextOptions options) : base(options)
    {}
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<ProductBrand> ProductBrands { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var assembly=Assembly.GetAssembly(typeof(ProductMapping));
        modelBuilder.ApplyConfigurationsFromAssembly(assembly);
        base.OnModelCreating(modelBuilder);
    }
}