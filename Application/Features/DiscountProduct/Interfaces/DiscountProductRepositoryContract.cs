namespace Application.Features.DiscountProduct.Interfaces;

public interface DiscountProductRepositoryContract
{
    Task AddProductToDiscountAsync(Domain.Entities.DiscountProduct discountProduct);
    Task<List<Guid>> GetExistingDiscountProductsAsync(Guid discountId, List<Guid> productIds);
    Task<Domain.Entities.DiscountProduct?> GetDiscountProductAsync(Guid discountId, Guid productId);
    Task RemoveAsync(Domain.Entities.DiscountProduct discountProduct);
    Task<Domain.Entities.Discount?> GetDiscountByProductIdAsync(Guid productId);
}