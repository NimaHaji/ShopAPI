using Application.Features.Product.DTOs;
using Domain.Entities;

namespace Application.Features.Product.Interfaces;

public interface ProductRepositoryContract
{
    #region Product

    Task<List<Domain.Entities.Product>?> GetProductList(ProductQueryDto query);
    Task<bool> IsExistingProduct(string productName);
    Task<Domain.Entities.Product?> GetProductByIdAsync(Guid productId);
    Task CreateProductAsync(Domain.Entities.Product product);
    Task<List<Domain.Entities.Product>> GetProductsByIdsAsync(List<Guid> productIds);
    Task<List<Domain.Entities.Product>?> SearchProductWithTitle(string query);

    #endregion

    #region Category

    Task<List<ProductCategory>> GetAllProductCategories();
    Task<bool> IsExistingProductCategory(string dtoTitle);
    Task AddProductCategory(ProductCategory category);
    Task<ProductCategory?> GetProductCategoryById(Guid productCategoryId);
    Task<List<ProductCategory>?> SearchProductCategoriesWithTitle(string dtoTitle);

    #endregion
    
    #region Brand
    
    Task<List<ProductBrand>> GetAllBrandAsync();
    Task<bool> IsExistingBrand(string dtoTitle);
    Task AddBrandAsync(ProductBrand brand);
    Task<ProductBrand?> GetProductBrandById(Guid productBrandId);
    Task<List<ProductBrand>?> SearchProductBrandsWithTitle(string dtoTitle);
    #endregion

    Task<List<Domain.Entities.Product>> GetProductsWithDiscountByIdsAsync(List<Guid> productIds);
}