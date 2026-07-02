namespace Application.Features.Product.Interfaces;

public interface ProductRepositoryContract
{
    Task<Domain.Entites.Product?> GetProductByIdAsync(Guid productId);
    Task CreateProductAsync(Domain.Entites.Product product);
    Task SaveAsync();
}