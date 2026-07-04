using Application.Common.Interfaces;
using Application.Features.Auth.Interfaces;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Features.Auth.Services;

public class PasswordRecoveryService:IPasswordRecoveryService
{
    private readonly IHasher _hasher;
    private readonly IUserRepository _userRepository;
    public PasswordRecoveryService(IHasher hasher, IUserRepository userRepository)
    {
        _hasher = hasher;
        _userRepository = userRepository;
    }
    
    // public async Task ResetPasswordAsync(string email,string code,string newPassword)
    // {
    //     var user=await _userRepository.GetUserByEmailAsync(email);
    //     if (user == null)
    //         throw new InvalidOperationException("Invalid request");
    //     
    //     var codeHash=_hasher.Hash(code);
    //
    //     if (!user.CanUseResetPassword(codeHash, DateTime.UtcNow))
    //     {
    //         user.IncreasePasswordResetAttemptCount();
    //         await _userRepository.SaveChangesAsync();
    //         throw new InvalidOperationException("Incorrect or expired code");
    //     }
    //     
    //     var hashedPassword = _hasher.Hash(newPassword);
    //     user.ResetPassword(hashedPassword, DateTime.UtcNow);
    //     user.ClearPasswordResetCode();
    //     
    //     await _userRepository.SaveChangesAsync();
    // }
    
    // public async Task ForgetPasswordAsync(string email)
    // {
    //     var user = await _userRepository.GetUserByEmailAsync(email);
    //     if (user == null)
    //         throw new NotFoundException($"user with email : {email} not found"); 
    //
    //     var code = _codeGenerator.Generate6DigitCode();
    //     var codeHash = _hasher.Hash(code);
    //
    //     user.ResetPassword(
    //         codeHash,
    //         DateTime.UtcNow.AddMinutes(5)
    //     );
    //
    //     await _userRepository.SaveChangesAsync();
    //     
    //     //Implement Email Service
    //     
    //     // await _emailService.SendAsync(
    //     //     email,
    //     //     "Password Reset Code",
    //     //     $"Your verification code is: {code}"
    //     // );
    // }
}