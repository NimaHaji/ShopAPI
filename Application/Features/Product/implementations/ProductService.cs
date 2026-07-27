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
    private readonly InventoryRepositoryContract _inventoryRepositoryContract;
    private readonly UnitOfWorkContract _unitOfWorkContract;

    public ProductService(ProductRepositoryContract productRepositoryContract,
        InventoryServiceContract inventoryServiceContract, UnitOfWorkContract unitOfWorkContract, InventoryRepositoryContract inventoryRepositoryContract)
    {
        _productRepositoryContract = productRepositoryContract;
        _inventoryServiceContract = inventoryServiceContract;
        _unitOfWorkContract = unitOfWorkContract;
        _inventoryRepositoryContract = inventoryRepositoryContract;
    }

    public async Task<ViewProductDto> GetAllProducts(ProductQueryDto query)
    {
        var products= await _productRepositoryContract.GetProductList(query);
        return new ViewProductDto
        {
            Items = products ?? []
        };
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
            categoryId: dto.CategoryId,
            brandId: dto.BrandId ?? null
        );

        await _inventoryServiceContract.AddStockAsync(product.Id, dto.Quantity, product.Description);

        await _productRepositoryContract.CreateProductAsync(product);
        await _unitOfWorkContract.SaveAsync();

        return $"محصول {product.Title} ساخته شد";
    }

    public async Task<ViewProductCategoryDto> GetAllCategories()
    {
        var categories= await _productRepositoryContract.GetAllProductCategories();
        
        return new ViewProductCategoryDto
        {
            Items = categories
        };
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

    public async Task<SearchProductResultDto> SearchProductByTitle(string query)
    {
        if (string.IsNullOrEmpty(query))
            return new SearchProductResultDto
            {
                Items = []
            };

        var productList = await _productRepositoryContract.SearchProductWithTitle(query);
        if (productList is null)
            return new SearchProductResultDto
            {
                Items = []
            };

        return new SearchProductResultDto
        {
            Items = productList,
        };
    }
}