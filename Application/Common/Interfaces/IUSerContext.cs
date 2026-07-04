using Domain.Enums;

namespace Application.Common.Interfaces;

public interface IUSerContext
{
    Guid? UserId { get;}
    string? Email { get;}
    UserRole Role { get;}
    bool IsAuthenticated { get;}
}