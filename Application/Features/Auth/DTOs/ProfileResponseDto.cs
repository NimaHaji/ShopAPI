namespace Application.Features.Auth.DTOs;

public class ProfileResponseDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Role { get; set; } = null!;
}