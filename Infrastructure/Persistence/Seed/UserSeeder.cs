using Application.Common;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Persistence.Seed.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Seed;

public class UserSeeder
{
    private readonly ShopDbContext _context;
    private readonly JsonSeedReader _reader;
    private readonly IPasswordHasher _passwordHasher;

    public UserSeeder(ShopDbContext context, JsonSeedReader reader, IPasswordHasher passwordHasher)
    {
        _context = context;
        _reader = reader;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync(SeedContext seedContext)
    {
        var items = await _reader.ReadListAsync<UserSeedDto>("users.json");

        foreach (var item in items)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == item.Email);

            if (existingUser is not null)
            {
                seedContext.Users[item.Key] = existingUser.Id;

                var existingCart = await _context.Carts
                    .FirstOrDefaultAsync(c => c.UserId == existingUser.Id);
                if (existingCart is not null)
                {
                    seedContext.Carts[item.Key] = existingCart.Id;
                }
                else
                {
                    var newCart = new Cart(existingUser.Id);
                    seedContext.Carts[item.Key] = newCart.Id;
                    await _context.Carts.AddAsync(newCart);
                }

                var existingWishlist = await _context.Wishlists
                    .FirstOrDefaultAsync(w => w.UserId == existingUser.Id);
                if (existingWishlist is not null)
                {
                    seedContext.Wishlists[item.Key] = existingWishlist.Id;
                }
                else
                {
                    var newWishlist = new Wishlist(existingUser.Id);
                    seedContext.Wishlists[item.Key] = newWishlist.Id;
                    await _context.Wishlists.AddAsync(newWishlist);
                }

                await EnsureAddressesAsync(item, existingUser.Id);
                continue;
            }

            var role = Enum.Parse<UserRole>(item.Role, ignoreCase: true);
            var hashedPassword = _passwordHasher.Hash(item.Password);
            var user = new User(item.FullName, item.Email, item.PhoneNumber, role, hashedPassword);

            seedContext.Users[item.Key] = user.Id;
            await _context.Users.AddAsync(user);

            var cart = new Cart(user.Id);
            seedContext.Carts[item.Key] = cart.Id;
            await _context.Carts.AddAsync(cart);

            var wishlist = new Wishlist(user.Id);
            seedContext.Wishlists[item.Key] = wishlist.Id;
            await _context.Wishlists.AddAsync(wishlist);

            foreach (var addressDto in item.Addresses)
            {
                var address = new Address(
                    user.Id,
                    addressDto.Title,
                    addressDto.ReceiverName,
                    addressDto.PhoneNumber,
                    addressDto.Province,
                    addressDto.City,
                    addressDto.AddressLine,
                    addressDto.PostalCode);

                if (addressDto.IsDefault)
                    address.SetAsDefault();

                await _context.Addresses.AddAsync(address);
            }
        }
    }

    private async Task EnsureAddressesAsync(UserSeedDto item, Guid userId)
    {
        foreach (var addressDto in item.Addresses)
        {
            var exists = await _context.Addresses.AnyAsync(a =>
                a.UserId == userId &&
                a.PostalCode == addressDto.PostalCode &&
                a.AddressLine == addressDto.AddressLine);

            if (exists)
                continue;

            var address = new Address(
                userId,
                addressDto.Title,
                addressDto.ReceiverName,
                addressDto.PhoneNumber,
                addressDto.Province,
                addressDto.City,
                addressDto.AddressLine,
                addressDto.PostalCode);

            if (addressDto.IsDefault)
                address.SetAsDefault();

            await _context.Addresses.AddAsync(address);
        }
    }
}
