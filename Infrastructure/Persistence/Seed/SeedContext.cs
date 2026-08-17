namespace Infrastructure.Persistence.Seed;

public class SeedContext
{
    public Dictionary<string, Guid> Categories { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, Guid> Brands { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, Guid> Products { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, Guid> ProductOptions { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, Guid> ProductOptionValues { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, Guid> Variants { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, Guid> InventoryItems { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, Guid> Users { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, Guid> Carts { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, Guid> Wishlists { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, Guid> Discounts { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, Guid> Coupons { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, Guid> Orders { get; } = new(StringComparer.OrdinalIgnoreCase);
}