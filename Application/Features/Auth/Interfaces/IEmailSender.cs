namespace Application.Features.Auth.Interfaces;

public interface IEmailSender
{
    Task SendEmailAsync(string to, string subject, string body);
    Task<string> RenderPasswordResetAsync(string code);
}