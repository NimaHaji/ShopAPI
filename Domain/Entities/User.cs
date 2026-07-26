using Domain.Enums;

namespace Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string FullName { get; private set; }
    public UserRole Role { get; private set; }
    public string Email { get; private set; }
    public string PhoneNumber { get; private set; }
    public string Password { get; private set; }
    public string? PasswordResetCodeHash { get; private set; }
    public DateTime? PasswordResetCodeExpireAt { get; private set; }
    public int PasswordResetAttemptsCount { get; private set; }

    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

    public User()
    {
    }

    public User(string fullName, string email, string phoneNumber, string password)
    {
        Id = Guid.NewGuid();
        FullName = fullName;
        Role = UserRole.User;
        Email = email;
        PhoneNumber = phoneNumber;
        Password = password;
    }

    public User(string fullName, string email, string phoneNumber, UserRole role, string password)
    {
        Id = Guid.NewGuid();
        FullName = fullName;
        Role = role;
        Email = email;
        PhoneNumber = phoneNumber;
        Password = password;
    }

    public void ChangeRoleTo(UserRole role)
    {
        Role = role;
    }

    public void UpdateProfile(string fullName, string phoneNumber)
    {
        FullName = fullName;
        PhoneNumber = phoneNumber;
    }

    public void ResetPassword(string codeHash, DateTime expiresAt)
    {
        PasswordResetCodeHash = codeHash;
        PasswordResetCodeExpireAt = expiresAt;
        PasswordResetAttemptsCount = 0;
    }

    public bool CanUseResetPassword(string codeHash, DateTime now)
    {
        if (PasswordResetCodeExpireAt == null || now > PasswordResetCodeExpireAt)
            return false;

        return PasswordResetCodeHash == codeHash;
    }

    public void IncreasePasswordResetAttemptCount() => PasswordResetAttemptsCount++;

    public void ClearPasswordResetCode()
    {
        PasswordResetCodeHash = null;
        PasswordResetCodeExpireAt = null;
        PasswordResetAttemptsCount = 0;
    }
}