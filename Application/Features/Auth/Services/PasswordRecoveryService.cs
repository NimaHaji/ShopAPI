using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Auth.Interfaces;
using Domain.Entities;
using FluentValidation.Validators;
using Shared.Exceptions;

namespace Application.Features.Auth.Services;

public class PasswordRecoveryService : IPasswordRecoveryService
{
    private readonly IHasher _hasher;
    private readonly IUserRepository _userRepository;
    private readonly IEmailSender _emailSenderContract;
    private readonly IVerificationCodeGenerator _codeGenerator;
    private readonly IPasswordHasher _passwordHasher;
    public PasswordRecoveryService(IHasher hasher, IUserRepository userRepository, IEmailSender emailSenderContract, IVerificationCodeGenerator codeGenerator, IPasswordHasher passwordHasher)
    {
        _hasher = hasher;
        _userRepository = userRepository;
        _emailSenderContract = emailSenderContract;
        _codeGenerator = codeGenerator;
        _passwordHasher = passwordHasher;
    }

    public async Task<string> ResetPasswordAsync(string email,string code,string newPassword)
    {
        var user = await _userRepository.GetUserByEmailAsync(email);

        if (user is null)
            throw new NotFoundException("کاربر یافت نشد.");

        if (user.PasswordResetCodeHash is null ||
            user.PasswordResetCodeExpireAt is null)
        {
            throw new InvalidOperationException(
                "کد بازیابی معتبر نیست.");
        }

        if (user.HasExceededPasswordResetAttempts())
        {
            throw new InvalidOperationException(
                "تعداد تلاش‌های مجاز به پایان رسیده است.");
        }

        if (DateTime.UtcNow > user.PasswordResetCodeExpireAt)
        {
            throw new InvalidOperationException(
                "کد بازیابی منقضی شده است.");
        }

        var isValidCode = _hasher.Verify(
            code,
            user.PasswordResetCodeHash);

        if (!isValidCode)
        {
            user.IncreasePasswordResetAttemptCount();

            await _userRepository.SaveChangesAsync();

            throw new InvalidOperationException(
                "کد بازیابی اشتباه است.");
        }

        var hashedPassword = _passwordHasher.Hash(newPassword);

        user.ChangePassword(hashedPassword);
        user.ClearPasswordResetCode();

        await _userRepository.SaveChangesAsync();

        return "رمز عبور با موفقیت تغییر کرد.";
    }

    public async Task<string> ForgetPasswordAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new BusinessException("ایمیل نمی تواند خالی باشد");
        
        var claimingUser = await _userRepository.GetUserByEmailAsync(email);
        
        if (claimingUser is null)
            return "اگر حسابی با این ایمیل وجود داشته باشد، کد بازیابی ارسال خواهد شد.";
        
        var code = _codeGenerator.Generate6DigitCode();
        var codeHash = _hasher.Hash(code);
        
        claimingUser.SetPasswordResetCode(
            codeHash,
            DateTime.UtcNow.AddMinutes(5)
        );
        
        await _userRepository.SaveChangesAsync();
        
        var htmlBody = await _emailSenderContract.RenderPasswordResetAsync(code);
        
        await _emailSenderContract.SendEmailAsync(email,"Password Recovery",htmlBody);
        
        return "اگر حسابی با این ایمیل وجود داشته باشد، کد بازیابی ارسال خواهد شد.";
    }
}