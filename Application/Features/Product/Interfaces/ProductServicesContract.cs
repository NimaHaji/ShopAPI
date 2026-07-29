using Application.Features.Order.DTOs;
using Application.Features.Product.DTOs;
using Domain.Entities;

namespace Application.Features.Product.Interfaces;

public interface ProductServicesContract
{
    Task<ViewProductDto> GetAllProducts(ProductQueryDto query);
    Task<string> AddProductAsync(CreateProductDto dto);
    Task<ViewProductCategoryDto> GetAllCategories();
    Task<string> CreateProductCategory(CreateProductCategoryDto dto);
    Task<SearchProductResultDto> SearchProductByTitle(string query);
    Task<ViewProductItemDto> GetProductById(Guid productId);
    Task<string> EditProductAsync(EditProductDto dto);
}