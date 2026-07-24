namespace Application.Features.Product.Interfaces;

public interface ProductRepositoryContract
{
    Task<Domain.Entities.Product?> GetProductByIdAsync(Guid productId);
    Task CreateProductAsync(Domain.Entities.Product product);
    Task<List<Domain.Entities.Product>> GetProductsByIdsAsync(List<Guid> productIds);
}