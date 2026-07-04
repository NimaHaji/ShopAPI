namespace Application.Features.Auth.DTOs;

public class ForgetPasswordRequestDto
{
 public string Email { get; set; }
}

public class ResetPasswordRequestDto
{
 public string Email { get; set; }
 public string Code { get; set; }
 public string NewPassword { get; set; }
}