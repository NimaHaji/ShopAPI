using System.Data;
using Application.Common.Interfaces;
using Application.Features.Inventory.Interfaces;
using Application.Features.Order.Interfaces;
using Application.Features.Product.DTOs;
using Application.Features.Product.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Product.implementations;

public class ProductService : ProductServicesContract
{
    private readonly ProductRepositoryContract _productRepositoryContract;
    private readonly InventoryServiceContract _inventoryServiceContract;
    private readonly UnitOfWorkContract _unitOfWorkContract;

    public ProductService(ProductRepositoryContract productRepositoryContract,
        InventoryServiceContract inventoryServiceContract, UnitOfWorkContract unitOfWorkContract)
    {
        _productRepositoryContract = productRepositoryContract;
        _inventoryServiceContract = inventoryServiceContract;
        _unitOfWorkContract = unitOfWorkContract;
    }

    public async Task<List<Domain.Entities.Product>?> GetAllProducts()
    {
        return await _productRepositoryContract.GetAllProducts();
    }

    public async Task<string> AddProductAsync(CreateProductDto dto)
    {
        var isExist = await _productRepositoryContract.IsExistingProduct(dto.Title);
        if (isExist)
            throw new DuplicateNameException("این محصول از قبل وجود دارد");

        var product = Domain.Entities.Product.Create(
            title: dto.Title,
            description: dto.Description,
            price: dto.Price,
            discountPercentage: null,
            stock: dto.Stock,
            categoryId: dto.CategoryId,
            brandId: dto.BrandId ?? null
        );

        await _inventoryServiceContract.AddStockAsync(product.Id, product.Stock, product.Description);

        await _productRepositoryContract.CreateProductAsync(product);
        await _unitOfWorkContract.SaveAsync();

        return $"محصول {product.Title} ساخته شد";
    }

    public async Task<List<ProductCategory>> GetAllCategories()
    {
        return await _productRepositoryContract.GetAllProductCategories();
    }

    public async Task<string> CreateProductCategory(CreateProductCategoryDto dto)
    {
        var isExist = await _productRepositoryContract.IsExistingProductCategory(dto.Title);
        if (isExist)
            throw new DuplicateNameException("دسته بندی محصول وجود دارد");

        var category = ProductCategory.Create(dto.Title);

        await _productRepositoryContract.AddProductCategory(category);
        await _unitOfWorkContract.SaveAsync();
        
        return $"دسته بندی {category.Title} ساخته شد .";
    }
}