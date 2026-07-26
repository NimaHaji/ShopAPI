using Domain.Entities;

namespace Application.Features.Product.Interfaces;

public interface ProductRepositoryContract
{
    Task<List<Domain.Entities.Product>?> GetAllProducts();
    Task<bool> IsExistingProduct(string productName);
    Task<Domain.Entities.Product?> GetProductByIdAsync(Guid productId);
    Task CreateProductAsync(Domain.Entities.Product product);
    Task<List<Domain.Entities.Product>> GetProductsByIdsAsync(List<Guid> productIds);
    Task<List<ProductCategory>> GetAllProductCategories();
    Task<bool> IsExistingProductCategory(string dtoTitle);
    Task AddProductCategory(ProductCategory category);
}