namespace Application.Features.Auth.Interfaces;

public interface IPasswordRecoveryService
{
    Task<string> ForgetPasswordAsync(string email);
    
    Task<string> ResetPasswordAsync(string email, string code, string newPassword);
}