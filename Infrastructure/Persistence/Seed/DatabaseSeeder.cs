using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Seed;

public class DatabaseSeeder
{
    private readonly ShopDbContext _context;
    private readonly CategorySeeder _categorySeeder;
    private readonly BrandSeeder _brandSeeder;
    private readonly ProductSeeder _productSeeder;
    private readonly DiscountSeeder _discountSeeder;
    private readonly CouponSeeder _couponSeeder;
    private readonly UserSeeder _userSeeder;
    private readonly ReviewSeeder _reviewSeeder;
    private readonly CartSeeder _cartSeeder;
    private readonly WishlistSeeder _wishlistSeeder;
    private readonly OrderSeeder _orderSeeder;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseSeeder> _logger;
    private readonly IHostEnvironment _environment;


    public DatabaseSeeder(
        ShopDbContext context,
        CategorySeeder categorySeeder,
        BrandSeeder brandSeeder,
        ProductSeeder productSeeder,
        DiscountSeeder discountSeeder,
        CouponSeeder couponSeeder,
        UserSeeder userSeeder,
        ReviewSeeder reviewSeeder,
        CartSeeder cartSeeder,
        WishlistSeeder wishlistSeeder,
        OrderSeeder orderSeeder,
        IConfiguration configuration,
        ILogger<DatabaseSeeder> logger,
        IHostEnvironment environment)
    {
        _context = context;

        _categorySeeder = categorySeeder;
        _brandSeeder = brandSeeder;
        _productSeeder = productSeeder;
        _discountSeeder = discountSeeder;
        _couponSeeder = couponSeeder;
        _userSeeder = userSeeder;
        _reviewSeeder = reviewSeeder;
        _cartSeeder = cartSeeder;
        _wishlistSeeder = wishlistSeeder;
        _orderSeeder = orderSeeder;
        _configuration = configuration;
        _logger = logger;
        _environment = environment;
    }


    public async Task SeedAsync()
    {
        var force = _configuration.GetValue<bool>("Seed:Force");

        var hasData = await _context.Products.AnyAsync();
        
        if (hasData && !force)
        {
            var count = await _context.Products.CountAsync();

            _logger.LogInformation(
                "Seeder skipped. Existing products: {Count}",
                count);

            return;
        }
        
        if (force && hasData)
        {
            if (!_environment.IsDevelopment())
            {
                throw new InvalidOperationException(
                    "Force seed is only allowed in Development environment.");
            }
            
            _logger.LogWarning(
                "Force seed enabled. Recreating database...");
            
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.MigrateAsync();
        }
        
        await using var transaction =
            await _context.Database.BeginTransactionAsync();
        
        try
        {
            _logger.LogInformation(
                "Database seeding started.");
            
            var seedContext = new SeedContext();
            await _categorySeeder.SeedAsync(seedContext);
            await _brandSeeder.SeedAsync(seedContext);
            await _productSeeder.SeedAsync(seedContext);
            await _discountSeeder.SeedAsync(seedContext);
            await _couponSeeder.SeedAsync(seedContext);
            await _userSeeder.SeedAsync(seedContext);
            await _reviewSeeder.SeedAsync(seedContext);
            await _cartSeeder.SeedAsync(seedContext);
            await _wishlistSeeder.SeedAsync(seedContext);
            await _orderSeeder.SeedAsync(seedContext);
            
            await _context.SaveChangesAsync();
            
            await transaction.CommitAsync();
            
            var productCount =
                await _context.Products.CountAsync();
            
            _logger.LogInformation(
                "Database seeding completed successfully. Products: {Count}",
                productCount);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();


            _logger.LogError(
                ex,
                "Database seeding failed.");


            throw;
        }
    }
}