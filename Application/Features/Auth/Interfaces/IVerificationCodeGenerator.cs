namespace Application.Features.Auth.Interfaces;

public interface IVerificationCodeGenerator
{
    string Generate6DigitCode();
}