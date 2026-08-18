namespace Infrastructure.Persistence.Seed.Models;

#region Catalog

public class CategorySeedDto
{
    public string Key { get; set; } = null!;
    public string Title { get; set; } = null!;
}


public class BrandSeedDto
{
    public string Key { get; set; } = null!;
    public string Title { get; set; } = null!;
}


#endregion


#region Product


public class ProductSeedDto
{
    public string Key { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string CategoryKey { get; set; } = null!;
    public string? BrandKey { get; set; }
    public List<ProductImageSeedDto> Images { get; set; }
        = new();
    public List<ProductOptionSeedDto> Options { get; set; }
        = new();
    public List<ProductVariantSeedDto> Variants { get; set; }
        = new();
}



public class ProductOptionSeedDto
{
    public string Name { get; set; } = null!;

    public List<string> Values { get; set; } = new();
}



public class ProductVariantSeedDto
{
    public string Key { get; set; } = null!;


    public string Sku { get; set; } = null!;


    public long Price { get; set; }


    public int Stock { get; set; }


    // Color = Black
    // Storage = 256GB
    public Dictionary<string, string> OptionSelections { get; set; }
        = new();


    public List<ProductImageSeedDto> Images { get; set; }
        = new();
}



public class ProductImageSeedDto
{
    public string Url { get; set; } = null!;

    public bool IsPrimary { get; set; }

    public int SortOrder { get; set; }
}


#endregion


#region Discount


public class DiscountSeedDto
{
    public string Key { get; set; } = null!;

    public string Title { get; set; } = null!;


    public string DiscountType { get; set; } = null!;


    public decimal Value { get; set; }


    public decimal? MaxDiscountAmount { get; set; }


    public DateTime StartsAt { get; set; }

    public DateTime EndsAt { get; set; }


    public List<string> ProductKeys { get; set; }
        = new();


    public List<string> VariantKeys { get; set; }
        = new();
}



public class CouponSeedDto
{
    public string Key { get; set; } = null!;

    public string Code { get; set; } = null!;


    public string DiscountType { get; set; } = null!;


    public decimal Value { get; set; }


    public decimal? MinimumOrderAmount { get; set; }


    public decimal? MaxDiscountAmount { get; set; }


    public int? UsageLimit { get; set; }


    public int? UserUsageLimit { get; set; }


    public DateTime StartsAt { get; set; }


    public DateTime EndAt { get; set; }
}


#endregion


#region User


public class UserSeedDto
{
    public string Key { get; set; } = null!;


    public string FullName { get; set; } = null!;


    public string Email { get; set; } = null!;


    public string PhoneNumber { get; set; } = null!;


    public string Password { get; set; } = null!;


    public string Role { get; set; } = "User";


    public List<AddressSeedDto> Addresses { get; set; }
        = new();
}



public class AddressSeedDto
{
    public string Title { get; set; } = null!;


    public string ReceiverName { get; set; } = null!;


    public string PhoneNumber { get; set; } = null!;


    public string Province { get; set; } = null!;


    public string City { get; set; } = null!;


    public string AddressLine { get; set; } = null!;


    public string PostalCode { get; set; } = null!;


    public bool IsDefault { get; set; }
}



#endregion


#region Review


public class ReviewSeedDto
{
    public string UserKey { get; set; } = null!;


    public string ProductKey { get; set; } = null!;


    public int StarsCount { get; set; }


    public string Comment { get; set; } = null!;


    public string Status { get; set; } = "Approved";
}


#endregion


#region Cart


public class CartSeedDto
{
    public string UserKey { get; set; } = null!;


    public List<CartItemSeedDto> Items { get; set; }
        = new();
}



public class CartItemSeedDto
{
    public string VariantKey { get; set; } = null!;


    public int Quantity { get; set; }
}


#endregion


#region Wishlist


public class WishlistSeedDto
{
    public string UserKey { get; set; } = null!;


    public List<string> ProductKeys { get; set; }
        = new();
}


#endregion


#region Order


public class OrderSeedDto
{
    public string Key { get; set; } = null!;


    public string UserKey { get; set; } = null!;


    public string Status { get; set; } = "Pending";


    public string ReceiverName { get; set; } = null!;


    public string PhoneNumber { get; set; } = null!;


    public string Province { get; set; } = null!;


    public string City { get; set; } = null!;


    public string AddressLine { get; set; } = null!;


    public string PostalCode { get; set; } = null!;


    public string? CouponKey { get; set; }


    public long? CouponDiscountAmount { get; set; }


    public List<OrderItemSeedDto> Items { get; set; }
        = new();


    public PaymentSeedDto? Payment { get; set; }
}



public class OrderItemSeedDto
{
    public string VariantKey { get; set; } = null!;


    public int Quantity { get; set; }


    public long UnitPrice { get; set; }


    public long DiscountAmount { get; set; }
}



public class PaymentSeedDto
{
    public string Gateway { get; set; } = "ZarinPal";


    public string Status { get; set; } = "Pending";


    public string Description { get; set; } = null!;
}


#endregion