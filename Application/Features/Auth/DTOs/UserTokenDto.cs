namespace Application.Features.Auth.DTOs;

public class UserTokenDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
}