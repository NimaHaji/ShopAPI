using System.Net.Http.Json;
using System.Security.AccessControl;
using System.Text.Json;
using Application.Features.DummyData.DTOs;
using Domain.Entities;
using Domain.Services;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Seed;

public class DatabaseSeeder
{
    private readonly ShopDbContext _context;
    private readonly SkuGeneratorContract  _skuGeneratorContract;
    private readonly HttpClient _httpClient;

    public DatabaseSeeder(ShopDbContext context, HttpClient httpClient, SkuGeneratorContract skuGeneratorContract)
    {
        _context = context;
        _httpClient = httpClient;
        _skuGeneratorContract = skuGeneratorContract;
    }

    public async Task SeedAsync()
    {
        const long DollarToToman = 172000;
        try
        {
            Console.WriteLine("Seeder: started");

            var any = await _context.Products.AsNoTracking().AnyAsync();
            Console.WriteLine($"Seeder: products any? {any}");
            if (any) return;

            Console.WriteLine("Seeder: fetching DummyJSON...");
            var response = await _httpClient.GetFromJsonAsync<DummyJsonProductResponse>(
                "https://dummyjson.com/products?limit=100",
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (response == null)
                throw new Exception("Seeder: response is NULL (deserialize failed?)");

            if (response.Products == null)
                throw new Exception("Seeder: response.Products is NULL (DTO mismatch?)");

            Console.WriteLine($"Seeder: received products = {response.Products.Count}");
            if (response.Products.Count == 0)
                throw new Exception("Seeder: received 0 products");

            var products = response.Products;

            #region Category

            var categoryTitles = products
                .Select(x => (x.Category ?? "").Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            Console.WriteLine($"Seeder: categories distinct = {categoryTitles.Count}");

            var categoryEntities = categoryTitles.Select(ProductCategory.Create).ToList();
            await _context.ProductCategories.AddRangeAsync(categoryEntities);

            #endregion

            #region Brands

            var brandTitles = products
                .Select(x => (x.Brand ?? "").Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            Console.WriteLine($"Seeder: brands distinct = {brandTitles.Count}");

            var brandEntities = brandTitles.Select(ProductBrand.Create).ToList();
            await _context.ProductBrands.AddRangeAsync(brandEntities);

            Console.WriteLine("Seeder: saving categories/brands...");
            await _context.SaveChangesAsync();

            #endregion

            var categoryMap = (await _context.ProductCategories
                    .AsNoTracking()
                    .ToListAsync())
                .GroupBy(x => x.Title.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.First().Id,
                    StringComparer.OrdinalIgnoreCase);

            var brandMap = (await _context.ProductBrands
                    .AsNoTracking()
                    .ToListAsync())
                .GroupBy(x => x.Title.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.First().Id,
                    StringComparer.OrdinalIgnoreCase);

            Console.WriteLine("Seeder: creating product entities...");

            #region InventoryItem & Product

            var productEntities = new List<Product>();
            var inventoryItems = new List<InventoryItem>();
            var inventoryTransactions = new List<InventoryTransaction>();

            foreach (var p in products)
            {
                var categoryTitle = (p.Category ?? "").Trim();
                if (!categoryMap.TryGetValue(categoryTitle, out var categoryId))
                    throw new Exception($"Seeder: category not found in map: '{p.Category}'");

                Guid? brandId = null;
                var brandTitle = (p.Brand ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(brandTitle) && brandMap.TryGetValue(brandTitle, out var bid))
                    brandId = bid;

                var tomanPrice = (long)(Math.Round(p.Price * DollarToToman / 1000m, MidpointRounding.AwayFromZero) * 1000m);
                
                var sku=_skuGeneratorContract.GenerateSku();
                
                var product = Product.Create(
                    p.Title,
                    p.Description,
                    tomanPrice,
                    p.DiscountPercentage,
                    categoryId,
                    brandId,
                    sku
                );
                
                for (var i = 0; i < p.Images.Count; i++)
                {
                    product.Images.Add(
                        new ProductImage(
                            product.Id,
                            p.Images[i],
                            i == 0,
                            i
                        )
                    );
                }

                productEntities.Add(product);
                var inventoryItem = new InventoryItem(
                    productId: product.Id,
                    stockQuantity: p.Stock,
                    reservedQuantity: 0
                );
                inventoryItems.Add(inventoryItem);
                var transaction = new InventoryTransaction
                (
                    inventoryItemId: inventoryItem.InventoryId,
                    transactionType: TransactionType.StockIn,
                    quantity: p.Stock,
                    reference: "INITIAL_SEED",
                    description: $"Initial stock from DummyJSON: {p.Stock} units"
                );

                inventoryTransactions.Add(transaction);
            }

            Console.WriteLine($"Seeder: inserting products={productEntities.Count}...");
            await _context.Products.AddRangeAsync(productEntities);

            Console.WriteLine($"Seeder: inserting inventory items={inventoryItems.Count}...");
            await _context.InventoryItems.AddRangeAsync(inventoryItems);

            Console.WriteLine($"Seeder: inserting inventory transactions={inventoryTransactions.Count}...");
            await _context.InventoryTransactions.AddRangeAsync(inventoryTransactions);

            #endregion

            await _context.SaveChangesAsync();

            Console.WriteLine("Seeder: done");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Seeder: FAILED ");
            Console.WriteLine(ex.ToString());
            throw;
        }
    }
}