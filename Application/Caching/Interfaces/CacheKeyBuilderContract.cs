using Application.Features.Product.DTOs;

namespace Application.Caching.Interfaces;

public interface CacheKeyBuilderContract
{
    #region Product

    string Product(Guid id);

    string ProductList(ProductQueryDto query);

    string ProductListPattern();

    string ProductSearch(string search);

    string ProductSearchPattern();

    #endregion

    #region Category

    string Category(Guid id);

    string CategoryList();

    string CategorySearch(string search);

    string CategorySearchPattern();

    #endregion
    
    #region Brand
    
    string Brand(Guid id);
    
    string BrandList();
    
    string BrandSearch(string search);
    
    string BrandSearchPattern();
    
    #endregion

    #region Review

    string ReviewList(Guid ProductId);

    #endregion
}