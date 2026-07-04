namespace Application.Features.Auth.DTOs;

public class RegisterUserRequestDto
{
    public string FullName { get; set; }
    public string Email  { get; set; }
    public string PhoneNumber { get; set; }
    public string Password { get; set; }
}