namespace Application.Features.Auth.DTOs;

public class UpdateProfileRequestDto
{
    public string FullName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
}