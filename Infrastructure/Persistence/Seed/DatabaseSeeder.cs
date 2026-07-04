using System.Net.Http.Json;
using System.Text.Json;
using Application.Features.DummyData.DTOs;
using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Seed;

public class DatabaseSeeder
{
    private readonly ShopDbContext _context;
    private readonly HttpClient _httpClient;

    public DatabaseSeeder(ShopDbContext context, HttpClient httpClient)
    {
        _context = context;
        _httpClient = httpClient;
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

            // Categories
            var categoryTitles = products
                .Select(x => (x.Category ?? "").Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            Console.WriteLine($"Seeder: categories distinct = {categoryTitles.Count}");

            var categoryEntities = categoryTitles.Select(ProductCategory.Create).ToList();
            await _context.ProductCategories.AddRangeAsync(categoryEntities);

            // Brands
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

            // Lookup (safe against duplicates)
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
            var entities = products.Select(p =>
            {
                var categoryTitle = (p.Category ?? "").Trim();
                if (!categoryMap.TryGetValue(categoryTitle, out var categoryId))
                    throw new Exception($"Seeder: category not found in map: '{p.Category}'");

                Guid? brandId = null;
                var brandTitle = (p.Brand ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(brandTitle) && brandMap.TryGetValue(brandTitle, out var bid))
                    brandId = bid;
                
                var tomanPrice = (long)(Math.Round(p.Price * DollarToToman / 1000m, MidpointRounding.AwayFromZero) * 1000m);
                return Product.Create(
                    p.Title,
                    p.Description,
                    tomanPrice,
                    p.DiscountPercentage,
                    p.Stock,
                    categoryId,
                    brandId,
                    p.Rating
                );
            }).ToList();

            Console.WriteLine($"Seeder: inserting products={entities.Count}...");
            await _context.Products.AddRangeAsync(entities);
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