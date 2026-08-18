using Domain.Entities;
using Domain.Enums;
using Domain.Services;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Persistence.Seed.Models;

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
                var sku =
                    string.IsNullOrWhiteSpace(variantDto.Sku)
                    ? _skuGenerator.GenerateSku()
                    : variantDto.Sku.Trim();



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
}