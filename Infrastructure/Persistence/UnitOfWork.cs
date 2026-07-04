using Application.Common.Interfaces;
using Application.Features.Cart.Interfaces;
using Infrastructure.Persistence.Contexts;

namespace Infrastructure.Persistence;

public class UnitOfWork:UnitOfWorkContract
{
    private readonly ShopDbContext _shopDbContext;

    public UnitOfWork(ShopDbContext shopDbContext)
    {
        _shopDbContext = shopDbContext;
    }

    public async Task SaveAsync()
    {
        await _shopDbContext.SaveChangesAsync();
    }
}