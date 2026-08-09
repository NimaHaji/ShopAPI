using Application.Features.Discount.DTOs;

namespace Application.Features.Discount.Interfaces;

public interface DiscountServiceContract
{
    Task<ViewDiscountDto> GetAllDiscountsAsync();
    Task<ViewDiscountItemsDto> GetDiscountByIdAsync(Guid discountId);
    Task<string> ActivateDiscountAsync(Guid discountId);
    Task<string> DeActivateDiscountAsync(Guid discountId);
    Task<string> CreateDiscountAsync(CreateDiscountDto dto);
    Task<string> EditDiscountByIdAsync(Guid discountId, EditDiscountDto dto);
    Task<string> DeleteDiscountByIdAsync(Guid discountId);
    Task<string> RestoreDiscountByIdAsync(Guid discountId);

    #region Product

    Task<string> SetDiscountForProductAsync(Guid discountId,AddProductToDiscountDto dto);
    Task<string> DeleteDiscountFoProduct(Guid discountId, Guid productId);
    Task<ViewDiscountItemsDto> GetDiscountByProductId(Guid productId);
    
    #endregion

}