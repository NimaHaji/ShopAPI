namespace Application.Features.DiscountVariant.Interfaces;

public interface DiscountVariantRepositoryContract
{
    Task<Domain.Entities.DiscountVariant?> GetDiscountProductAsync(Guid discountId,Guid productVariantId);
    Task RemoveAsync(Domain.Entities.DiscountVariant discountVariant);
    Task<Domain.Entities.Discount?> GetDiscountByProductVariantIdAsync(Guid productVariantId);
    Task<List<Guid>> GetExistingDiscountVariantsAsync(Guid discountId, List<Guid> dtoProductVariantIds);
    Task AddProductToDiscountAsync(Domain.Entities.DiscountVariant discountVariant);
}