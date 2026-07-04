using System.Security.Claims;
using Application.Common.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Persistence.Contexts;

public class UserContext : IUSerContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?
                .User?
                .FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out var id)
                ? id
                : Guid.Empty;
        }
    }

    public bool IsAuthenticated => _httpContextAccessor
        .HttpContext?
        .User?
        .Identity?
        .IsAuthenticated ?? false;

    public string? Email => _httpContextAccessor.HttpContext
        .User
        .FindFirstValue(ClaimTypes.Email);

    public UserRole Role
    {
        get
        {
            var value = _httpContextAccessor.HttpContext
                .User
                .FindFirstValue(ClaimTypes.Role);
            return Enum.TryParse<UserRole>(value, out var role)
                ? role
                : UserRole.User;
        }
    }
}