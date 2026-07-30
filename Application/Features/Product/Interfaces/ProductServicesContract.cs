using Application.Features.Order.DTOs;
using Application.Features.Product.DTOs;
using Domain.Entities;

namespace Application.Features.Product.Interfaces;

public interface ProductServicesContract
{
    #region Product
    
    Task<ViewProductDto> GetAllProducts(ProductQueryDto query);
    Task<string> AddProductAsync(CreateProductDto dto);
    Task<SearchProductResultDto> SearchProductByTitle(string query);
    Task<ViewProductItemDto> GetProductById(Guid productId);
    Task<string> EditProductAsync(EditProductDto dto);
    Task<string> DeleteProductAsync(Guid productId);
    
    #endregion
    
    #region Category
    
    Task<ViewProductCategoryDto> GetAllCategories();
    Task<string> CreateProductCategoryAsync(CreateProductCategoryDto dto);
    Task<string> DeleteProductCategoryAsync(Guid productCategoryId);
    Task<string> EditProductCategoryAsync(EditProductCategoryDto dto);
    Task<ViewProductCategoryDto> SearchProductCategoryByTitle(SearchProductCategoryDto dto);
    Task<ViewProductCategoryItemDto> GetProductCategoryById(Guid productCategoryId);
    
    #endregion
    
    #region Brand
    
    Task<ViewProductBrandDto> GetAllProductBrands();
    Task<string> CreateProductBrandAsync(CreateProductBrandDto dto);
    Task<string> DeleteProductBrandAsync(Guid productBrandId);
    Task<string> EditProductBrandAsync(EditProductBrandDto dto);
    Task<ViewProductBrandDto> SearchProductBrandByTitle(SearchProductBrandDto dto);
    Task<ViewProductBrandItemDto> GetProductBrandById(Guid productBrandId);
    #endregion

}