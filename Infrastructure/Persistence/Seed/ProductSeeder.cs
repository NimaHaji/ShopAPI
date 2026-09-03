using Domain.Entities;
using Domain.Services;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Persistence.Seed.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Seed;

public class ProductSeeder
{
    private readonly ShopDbContext _context;
    private readonly JsonSeedReader _reader;
    private readonly SkuGeneratorContract _skuGenerator;

    public ProductSeeder(
        ShopDbContext context,
        JsonSeedReader reader,
        SkuGeneratorContract skuGenerator)
    {
        _context = context;
        _reader = reader;
        _skuGenerator = skuGenerator;
    }

    public async Task SeedAsync(SeedContext seedContext)
    {
        var items = await _reader.ReadListAsync<ProductSeedDto>(
            "products.json",
            required: true);


        foreach (var item in items)
        {
            // Idempotency: skip products that already exist (matched by Title).
            var existingProduct = await _context.Products
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Title == item.Title);

            if (existingProduct is not null)
            {
                seedContext.Products[item.Key] = existingProduct.Id;
                MapExistingVariants(seedContext, item, existingProduct);
                continue;
            }

            if (!seedContext.Categories.TryGetValue(
                    item.CategoryKey,
                    out var categoryId))
            {
                throw new InvalidOperationException(
                    $"Category key not found: {item.CategoryKey}");
            }
            
            Guid? brandId = null;

            if (!string.IsNullOrWhiteSpace(item.BrandKey))
            {
                if (!seedContext.Brands.TryGetValue(
                        item.BrandKey,
                        out var bid))
                {
                    throw new InvalidOperationException(
                        $"Brand key not found: {item.BrandKey}");
                }

                brandId = bid;
            }

            var product = Product.Create(
                item.Title,
                item.Description,
                categoryId,
                brandId);
            
            seedContext.Products[item.Key] = product.Id;
            
            foreach (var imageDto in item.Images)
            {
                var image = new ProductImage(
                    product.Id,
                    imageDto.Url,
                    imageDto.IsPrimary,
                    imageDto.SortOrder);


                product.Images.Add(image);
            }



            var optionMap =
                new Dictionary<string,
                    (Guid OptionId, Dictionary<string, Guid> Values)>(
                    StringComparer.OrdinalIgnoreCase);



            /*
             * Product Options
             */
            foreach (var optionDto in item.Options)
            {
                var option = ProductOption.Create(
                    product.Id,
                    optionDto.Name);


                product.Options.Add(option);



                var valueMap =
                    new Dictionary<string, Guid>(
                        StringComparer.OrdinalIgnoreCase);



                foreach (var value in optionDto.Values)
                {
                    var optionValue =
                        ProductOptionValue.Create(
                            option.Id,
                            value);


                    option.Values.Add(optionValue);


                    valueMap[value] = optionValue.Id;
                }



                optionMap[optionDto.Name] =
                (
                    option.Id,
                    valueMap
                );
            }



            if (item.Variants.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Product '{item.Key}' has no variants.");
            }




            /*
             * Variants
             */
            foreach (var variantDto in item.Variants)
            {
                var sku = ResolveSku(variantDto);

                // Idempotency: SKU is unique. Reuse existing variant instead of failing.
                var existingVariant = await _context.Variants
                    .FirstOrDefaultAsync(v => v.Sku == sku);

                if (existingVariant is not null)
                {
                    seedContext.Variants[variantDto.Key] =
                        existingVariant.Id;
                    continue;
                }

                var variant =
                    ProductVariant.Create(
                        product.Id,
                        sku,
                        variantDto.Price);



                product.Variants.Add(variant);



                seedContext.Variants[variantDto.Key] =
                    variant.Id;



                /*
                 * Variant Options
                 */
                foreach (var selection in variantDto.OptionSelections)
                {
                    if (!optionMap.TryGetValue(
                            selection.Key,
                            out var optionEntry))
                    {
                        throw new InvalidOperationException(
                            $"Option '{selection.Key}' not found.");
                    }



                    if (!optionEntry.Values.TryGetValue(
                            selection.Value,
                            out var valueId))
                    {
                        throw new InvalidOperationException(
                            $"Value '{selection.Value}' not found.");
                    }



                    var variantOption =
                        ProductVariantOption.Create(
                            variant.Id,
                            optionEntry.OptionId,
                            valueId);



                    variant.Options.Add(variantOption);
                }




                /*
                 * Variant Images
                 */
                foreach (var imageDto in variantDto.Images)
                {
                    var variantImage =
                        ProductVariantImage.Create(
                            variant.Id,
                            imageDto.Url,
                            imageDto.IsPrimary,
                            imageDto.SortOrder);



                    variant.Images.Add(variantImage);
                }




                /*
                 * Inventory
                 */
                var inventory =
                    new InventoryItem(
                        variant.Id,
                        variantDto.Stock,
                        0);



                variant.SetInventoryItem(inventory);
                
                if (variantDto.Stock > 0)
                {
                    var transaction =
                        new InventoryTransaction(
                            inventory.InventoryId,
                            TransactionType.StockIn,
                            variantDto.Stock,
                            "INITIAL_SEED",
                            $"Initial stock for {variantDto.Key}");

                    inventory.Transactions.Add(transaction);
                }
            }

            await _context.Products.AddAsync(product);
        }
        
        await _context.SaveChangesAsync();
    }

    private string ResolveSku(ProductVariantSeedDto variantDto)
    {
        if (!string.IsNullOrWhiteSpace(variantDto.Sku))
            return variantDto.Sku.Trim();

        // Deterministic fallback so re-running the seeder produces the same SKU
        // instead of a new random one (which would break idempotency).
        // Variant keys in seed JSON are unique, so they are safe to derive SKUs from.
        return $"SEED-{variantDto.Key.Trim().ToUpperInvariant()}";
    }

    private static void MapExistingVariants(
        SeedContext seedContext,
        ProductSeedDto item,
        Product existingProduct)
    {
        var existingVariants = existingProduct.Variants
            .OrderBy(v => v.AddedAt)
            .ThenBy(v => v.Sku, StringComparer.OrdinalIgnoreCase)
            .ToList();

        for (var i = 0; i < item.Variants.Count; i++)
        {
            var variantDto = item.Variants[i];

            // Prefer matching by explicit SKU when available.
            if (!string.IsNullOrWhiteSpace(variantDto.Sku))
            {
                var sku = variantDto.Sku.Trim();
                var bySku = existingVariants.FirstOrDefault(v =>
                    v.Sku.Equals(sku, StringComparison.OrdinalIgnoreCase));

                if (bySku is not null)
                {
                    seedContext.Variants[variantDto.Key] = bySku.Id;
                    continue;
                }
            }

            // Fallback for variants with generated SKUs (old seed runs used random
            // SKUs, so we cannot match by SKU): map by position when counts align.
            if (existingVariants.Count == item.Variants.Count)
            {
                seedContext.Variants[variantDto.Key] = existingVariants[i].Id;
                continue;
            }

            // Last resort: deterministic SKU lookup (for runs seeded with new logic).
            var deterministicSku = $"SEED-{variantDto.Key.Trim().ToUpperInvariant()}";
            var byDeterministic = existingVariants.FirstOrDefault(v =>
                v.Sku.Equals(deterministicSku, StringComparison.OrdinalIgnoreCase));

            if (byDeterministic is not null)
            {
                seedContext.Variants[variantDto.Key] = byDeterministic.Id;
            }
        }
    }
}