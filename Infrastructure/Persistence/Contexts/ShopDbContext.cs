using System.Reflection;
using Domain.Entites;
using Infrastructure.Persistence.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Contexts; 

public class ShopDbContext:DbContext
{
    public ShopDbContext(DbContextOptions options) : base(options)
    {}
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<ProductBrand> ProductBrands { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var assembly=Assembly.GetAssembly(typeof(ProductMapping));
        modelBuilder.ApplyConfigurationsFromAssembly(assembly);
        base.OnModelCreating(modelBuilder);
    }
}