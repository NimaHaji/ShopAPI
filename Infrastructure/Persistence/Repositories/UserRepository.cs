using Application.Features.Auth.DTOs;
using Application.Features.Auth.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ShopDbContext _context;

    public UserRepository(ShopDbContext context)
    {
        _context = context;
    }

    public async Task RegisterUserAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await SaveChangesAsync();
    }

    public async Task<List<User>?> GetUsersByEmailAsync(string email)
    {
        return await _context
            .Users
            .Where(x => email == x.Email)
            .ToListAsync();
    }
    
    public async Task<bool> IsUserExistsByIdAsync(Guid userId)
    {
        return await _context.Users.AnyAsync(x => x.Id == userId);
    }

    public async Task<bool> IsUserExistsByEmailAsync(string email)
    {
        return await _context
            .Users
            .AnyAsync(x => x.Email == email);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<List<ViewUser>> GetAllUsersAsync()
    {
        return await _context
            .Users
            .Select(x => new ViewUser
            {
                FullName = x.FullName,
                Email = x.Email,
                UserRole = x.Role.ToString(),
                PhoneNumber = x.PhoneNumber,
            }).ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _context
            .Users
            .Where(x => x.Id == userId)
            .FirstOrDefaultAsync();
    }

}