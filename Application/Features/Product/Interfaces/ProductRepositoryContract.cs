using Application.Features.Product.DTOs;
using Domain.Entities;

namespace Application.Features.Product.Interfaces;

public interface ProductRepositoryContract
{
    Task<List<ViewProductItemDto>> GetProductList(ProductQueryDto query);
    Task<bool> IsExistingProduct(string productName);
    Task<Domain.Entities.Product?> GetProductByIdAsync(Guid productId);
    Task CreateProductAsync(Domain.Entities.Product product);
    Task<List<Domain.Entities.Product>> GetProductsByIdsAsync(List<Guid> productIds);
    Task<List<ViewProductCategoryItemDto>> GetAllProductCategories();
    Task<bool> IsExistingProductCategory(string dtoTitle);
    Task AddProductCategory(ProductCategory category);
    Task<List<SearchProductItemsResultDto>?> SearchProductWithTitle(string query);
}