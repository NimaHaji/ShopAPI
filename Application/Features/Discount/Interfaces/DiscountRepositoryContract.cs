using Application.Features.Discount.DTOs;
using Domain.Entities;

namespace Application.Features.Discount.Interfaces;

public interface DiscountRepositoryContract
{
    Task<List<Domain.Entities.Discount>?> GetAllDiscountAsync();
    Task<List<Domain.Entities.Discount>?> GetAllActiveDiscountAsync();
    Task<Domain.Entities.Discount?> GetActiveDiscountByIdAsync(Guid discountId);
    Task<Domain.Entities.Discount?> GetDiscountByIdAsync(Guid discountId);
    Task<Domain.Entities.Discount?> GetDiscountForAdminByIdAsync(Guid discountId);
    Task CreateDiscountAsync(Domain.Entities.Discount discount);

}