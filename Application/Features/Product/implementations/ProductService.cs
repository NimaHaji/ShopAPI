using System.ComponentModel;
using System.Data;
using Application.Common.Interfaces;
using Application.Features.Inventory.Interfaces;
using Application.Features.Order.Interfaces;
using Application.Features.Product.DTOs;
using Application.Features.Product.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Exceptions;

namespace Application.Features.Product.implementations;

public class ProductService : ProductServicesContract
{
    private readonly ProductRepositoryContract _productRepositoryContract;
    private readonly InventoryServiceContract _inventoryServiceContract;
    private readonly InventoryRepositoryContract _inventoryRepositoryContract;
    private readonly UnitOfWorkContract _unitOfWorkContract;

    public ProductService(ProductRepositoryContract productRepositoryContract,
        InventoryServiceContract inventoryServiceContract, UnitOfWorkContract unitOfWorkContract,
        InventoryRepositoryContract inventoryRepositoryContract)
    {
        _productRepositoryContract = productRepositoryContract;
        _inventoryServiceContract = inventoryServiceContract;
        _unitOfWorkContract = unitOfWorkContract;
        _inventoryRepositoryContract = inventoryRepositoryContract;
    }

    public async Task<ViewProductDto> GetAllProducts(ProductQueryDto query)
    {
        var products = await _productRepositoryContract.GetProductList(query);
        foreach (var p in products)
        {
            Console.WriteLine($"""
                               Product: {p.Title}
                               Category: {p.Category != null}
                               Brand: {p.Brand != null}
                               Inventory: {p.InventoryItem != null}
                               """);
        }

        var dto = products.Select(p => new ViewProductItemDto
        {
            Title = p.Title,
            Description = p.Description,
            Brand = p.Brand?.Title ?? "بدون برند",
            Price = p.Price,
            Category = p.Category.Title,
            DiscountPercentage = p.DiscountPercentage,
            Stock = p.InventoryItem.AvailableQuantity
        }).ToList();

        return new ViewProductDto
        {
            Items = dto
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
        var categories = await _productRepositoryContract.GetAllProductCategories();

        var dto = categories.Select(x => new ViewProductCategoryItemDto
        {
            Title = x.Title
        }).ToList();

        return new ViewProductCategoryDto
        {
            Items = dto
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

        var dto = productList.Select(x => new SearchProductItemsResultDto
        {
            Title = x.Title,
            Category = x.Category.Title,
        }).ToList();


        return new SearchProductResultDto
        {
            Items = dto
        };
    }

    public async Task<ViewProductItemDto> GetProductById(Guid productId)
    {
        if (productId.Equals(Guid.Empty))
            throw new BusinessException("شناسه محصول خالی است");

        var product = await _productRepositoryContract.GetProductByIdAsync(productId);

        if (product is null)
            throw new NotFoundException("محصول یافت نشد");

        var dto = new ViewProductItemDto
        {
            Title = product.Title,
            Description = product.Description,
            Price = product.Price,
            Category = product.Category.Title,
            DiscountPercentage = product.DiscountPercentage,
            Brand = product.Brand?.Title ?? "بدون برند",
            Stock = product.InventoryItem.AvailableQuantity
        };

        return dto;
    }

    public async Task<string> EditProductAsync(EditProductDto dto)
    {
        if (dto.Title is null &&
            dto.Description is null &&
            dto.DiscountPercentage is null &&
            dto.DiscountPercentage is null)
            throw new BusinessException("برای ویرایش محصول، حداقل یک فیلد را تغییر دهید .");
        
        int attempt = 0;
        const int maxAttempts = 5;

        while (maxAttempts >= attempt)
        {
            var product = await _productRepositoryContract.GetProductByIdAsync(dto.Id);

            if (product is null)
                throw new NotFoundException("محصولی یافت نشد !");
            
            try
            {
                await _unitOfWorkContract.BeginTransactionAsync();

                product.Edit(dto.Title, dto.Description, dto.Price, dto.DiscountPercentage);

                await _unitOfWorkContract.SaveAsync();
                await _unitOfWorkContract.CommitTransactionAsync();
                return "محصول با موفقیت تغییر کرد";
            }
            catch (DbUpdateConcurrencyException)
            {
                attempt++;

                await _unitOfWorkContract.RollbackTransactionAsync();
                _unitOfWorkContract.ClearChangeTracker();

                if (attempt == maxAttempts)
                    throw new ConflictException("محصول در حال تغییر است . لطفا دوباره تلاش کنید");
            }
            catch
            {
                await _unitOfWorkContract.RollbackTransactionAsync();
                throw;
            }
        }

        throw new InvalidOperationException("خطای ناشناخته");
    }
}