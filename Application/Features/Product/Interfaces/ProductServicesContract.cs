using Application.Features.Order.DTOs;
using Application.Features.Product.DTOs;
using Domain.Entities;

namespace Application.Features.Product.Interfaces;

public interface ProductServicesContract
{
    Task<List<Domain.Entities.Product>?> GetAllProducts();
    Task<string> AddProductAsync(CreateProductDto dto);
    Task<List<ProductCategory>> GetAllCategories();
    Task<string> CreateProductCategory(CreateProductCategoryDto dto);
}