using Application.Features.Address.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class AddressRepository:AddressRepositoryContract
{
    private readonly ShopDbContext _shopDbContext;

    public AddressRepository(ShopDbContext shopDbContext)
    {
        _shopDbContext = shopDbContext;
    }

    public async Task<List<Address>> GetAllAddressesAsync()
    {
        return await _shopDbContext
            .Addresses
            .OrderByDescending(a=>a.CreatedAt)
            .ToListAsync();
    }

    public async Task<Address?> GetAddressByIdAndUserIdAsync(Guid userId, Guid dtoAddressId)
    {
        return await _shopDbContext
            .Addresses
            .Where(a => a.UserId == userId && a.Id == dtoAddressId)
            .FirstOrDefaultAsync();
    }

    public async Task<Address?> GetAddressByIdAsync(Guid addressId)
    {
        return await _shopDbContext
            .Addresses
            .Where(a => a.Id == addressId)
            .FirstOrDefaultAsync();
    }

    public async Task CreateAddressAsync(Address address)
    {
        await _shopDbContext
            .Addresses
            .AddAsync(address);
    }

    public async Task DeleteAddressAsync(Address address)
    {
         _shopDbContext
            .Addresses
            .Remove(address);
    }

    public async Task<List<Address>> GetAllAddressesByUserIdAsync(Guid userId)
    {
        return await _shopDbContext
            .Addresses
            .Where(a => a.UserId == userId)
            .ToListAsync();
    }
}