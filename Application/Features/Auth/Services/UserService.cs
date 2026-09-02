using System.Diagnostics;
using Application.Common;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Observability;
using Application.Features.Auth.DTOs;
using Application.Features.Auth.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Exceptions;

namespace Application.Features.Auth.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IHasher _hasher;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUSerContext _userContext;
    private readonly UnitOfWorkContract _unitOfWorkContract;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository repository, IJwtTokenService jwtTokenGenerator,
        IRefreshTokenRepository refreshTokenRepository, IHasher hasher, IPasswordHasher passwordHasher,
        IUSerContext userContext, UnitOfWorkContract unitOfWorkContract, ILogger<UserService> logger)
    {
        _userRepository = repository;
        _jwtTokenService = jwtTokenGenerator;
        _refreshTokenRepository = refreshTokenRepository;
        _hasher = hasher;
        _passwordHasher = passwordHasher;
        _userContext = userContext;
        _unitOfWorkContract = unitOfWorkContract;
        _logger = logger;
    }

    public async Task<string> RegisterUserAsync(
        RegisterUserRequestDto registerUserRequestDto)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(RegisterUserAsync));

        _logger.LogInformation(
            LogEvents.UserRegistration.Started,
            "User registration started.");

        if (await _userRepository
                .IsUserExistsByEmailAsync(registerUserRequestDto.Email))
        {
            _logger.LogWarning(
                LogEvents.UserRegistration.DuplicateEmail,
                "User registration failed because email already exists.");

            throw new DuplicateUserException(
                "این ایمیل از قبل وجود دارد .");
        }

        var password =
            _passwordHasher.Hash(registerUserRequestDto.Password);

        var user = new User(
            registerUserRequestDto.FullName,
            registerUserRequestDto.Email,
            registerUserRequestDto.PhoneNumber,
            password);

        await _userRepository.RegisterUserAsync(user);
        await _unitOfWorkContract.SaveAsync();

        activity?.SetTag(
            "user.id",
            user.Id);

        activity?.SetStatus(
            ActivityStatusCode.Ok);

        _logger.LogInformation(
            LogEvents.UserRegistration.Succeeded,
            "User registration completed successfully. UserId: {UserId}",
            user.Id);

        return $"{user.FullName} با موفقیت ثبت نام شد ";
    }

    public async Task<LoginUserResponseDto> LoginUserAsync(
        LoginUserRequestDto loginUserRequestDto)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(LoginUserAsync));

        _logger.LogInformation(
            LogEvents.UserLogin.Started,
            "User login started.");

        var user =
            await _userRepository
                .GetUserByEmailAsync(loginUserRequestDto.Email);

        if (user == null)
        {
            _logger.LogWarning(
                LogEvents.UserLogin.InvalidCredentials,
                "User login failed due to invalid credentials.");

            throw new UnauthorizedAccessException(
                "ایمیل یا رمز عبور نامعتبر است .");
        }

        var isUserValid =
            _passwordHasher.Verify(
                user.Password,
                loginUserRequestDto.Password);

        if (!isUserValid)
        {
            _logger.LogWarning(
                LogEvents.UserLogin.InvalidCredentials,
                "User login failed due to invalid credentials.");

            throw new UnauthorizedAccessException(
                "ایمیل یا رمز عبور نامعتبر است .");
        }

        var token =
            _jwtTokenService.GenerateJwtToken(user);

        var refreshTokenValue =
            _jwtTokenService.GenerateRefreshToken();

        var refreshTokenHash =
            _hasher.Hash(refreshTokenValue);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = refreshTokenHash,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _refreshTokenRepository.AddAsync(refreshToken);
        await _unitOfWorkContract.SaveAsync();

        activity?.SetTag(
            "user.id",
            user.Id);

        activity?.SetStatus(
            ActivityStatusCode.Ok);

        _logger.LogInformation(
            LogEvents.UserLogin.Succeeded,
            "User login completed successfully. UserId: {UserId}",
            user.Id);

        return new LoginUserResponseDto(
            token,
            refreshTokenValue);
    }

    public async Task ChangeRoleTo(UserRole role)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(ChangeRoleTo));

        var userId = _userContext.UserId
                     ?? throw new UnauthorizedAccessException(
                         "کاربر احراز هویت نشده است.");

        activity?.SetTag("user.id", userId);
        activity?.SetTag("user.new_role", role.ToString());

        _logger.LogInformation(
            LogEvents.UserRoleChange.Started,
            "User role change started. UserId: {UserId}, NewRole: {NewRole}",
            userId,
            role);

        var user =
            await _userRepository.GetUserByIdAsync(userId);

        if (user is null)
        {
            _logger.LogWarning(
                LogEvents.UserRoleChange.UserNotFound,
                "User role change failed because user was not found. UserId: {UserId}",
                userId);

            throw new NotFoundException(
                "کاربر یافت نشد.");
        }

        user.ChangeRoleTo(role);

        await _unitOfWorkContract.SaveAsync();

        activity?.SetStatus(
            ActivityStatusCode.Ok);

        _logger.LogInformation(
            LogEvents.UserRoleChange.Succeeded,
            "User role changed successfully. UserId: {UserId}, NewRole: {NewRole}",
            userId,
            role);
    }

    public async Task<string> LogoutUserAsync()
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(LogoutUserAsync));

        var userId = _userContext.UserId
                     ?? throw new UnauthorizedAccessException(
                         "کاربر احراز هویت نشده است.");

        activity?.SetTag("user.id", userId);

        _logger.LogInformation(
            LogEvents.UserLogout.Started,
            "User logout started. UserId: {UserId}",
            userId);

        var refreshTokens =
            await _refreshTokenRepository
                .GetRefreshTokensByIdAsync(userId);

        foreach (var token in refreshTokens)
        {
            token.IsRevoked = true;
        }

        await _unitOfWorkContract.SaveAsync();

        activity?.SetTag(
            "refresh_tokens.revoked_count",
            refreshTokens.Count);

        activity?.SetStatus(
            ActivityStatusCode.Ok);

        _logger.LogInformation(
            LogEvents.UserLogout.Succeeded,
            "User logout completed successfully. UserId: {UserId}, RevokedRefreshTokens: {Count}",
            userId,
            refreshTokens.Count);

        return "User Logged out .";
    }

    public async Task<LoginUserResponseDto> RefreshTokenAsync(
        string refreshToken)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(RefreshTokenAsync));

        _logger.LogInformation(
            LogEvents.RefreshToken.Started,
            "Refresh token operation started.");

        int attempts = 0;
        const int maxAttempts = 4;

        var hashedToken =
            _hasher.Hash(refreshToken);

        while (attempts < maxAttempts)
        {
            try
            {
                await _unitOfWorkContract.BeginTransactionAsync();

                var storedToken =
                    await _refreshTokenRepository
                        .GetAsync(hashedToken);

                if (storedToken is null)
                {
                    _logger.LogWarning(
                        LogEvents.RefreshToken.Invalid,
                        "Refresh token was not found.");

                    throw new UnauthorizedAccessException(
                        "Invalid refresh token.");
                }

                activity?.SetTag(
                    "user.id",
                    storedToken.UserId);

                if (storedToken.IsRevoked)
                {
                    _logger.LogWarning(
                        LogEvents.RefreshToken.Revoked,
                        "Refresh token was already revoked.");

                    throw new UnauthorizedAccessException(
                        "Refresh token already used.");
                }

                if (storedToken.ExpiresAt <= DateTime.UtcNow)
                {
                    _logger.LogWarning(
                        LogEvents.RefreshToken.Expired,
                        "Refresh token expired. ExpiresAt: {ExpiresAt}",
                        storedToken.ExpiresAt);

                    throw new UnauthorizedAccessException(
                        "Refresh token expired.");
                }

                _logger.LogInformation(
                    LogEvents.RefreshToken.Generating,
                    "Generating new access token.");

                var newAccessToken =
                    _jwtTokenService.GenerateJwtToken(
                        storedToken.User);

                var rotateWindow =
                    TimeSpan.FromMinutes(5);

                string? newRefreshTokenValue = null;
                bool rotated = false;

                if (storedToken.ExpiresAt - DateTime.UtcNow
                    <= rotateWindow)
                {
                    storedToken.IsRevoked = true;

                    _logger.LogInformation(
                        LogEvents.RefreshToken.Rotated,
                        "Refresh token rotation triggered.");

                    newRefreshTokenValue =
                        _jwtTokenService.GenerateRefreshToken();

                    await _refreshTokenRepository.AddAsync(
                        new RefreshToken
                        {
                            Id = Guid.NewGuid(),
                            Token = _hasher.Hash(
                                newRefreshTokenValue),
                            UserId = storedToken.UserId,
                            ExpiresAt = DateTime.UtcNow.AddDays(7),
                            IsRevoked = false
                        });

                    rotated = true;
                }

                await _unitOfWorkContract.SaveAsync();

                await _unitOfWorkContract
                    .CommitTransactionAsync();

                activity?.SetTag(
                    "refresh_token.rotated",
                    rotated);

                activity?.SetStatus(
                    ActivityStatusCode.Ok);

                _logger.LogInformation(
                    LogEvents.RefreshToken.Succeeded,
                    "Refresh token operation completed successfully. UserId: {UserId}, Rotated: {Rotated}",
                    storedToken.UserId,
                    rotated);

                return new LoginUserResponseDto(
                    newAccessToken,
                    newRefreshTokenValue ?? refreshToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                attempts++;

                await _unitOfWorkContract
                    .RollbackTransactionAsync();

                _unitOfWorkContract
                    .ClearChangeTracker();

                _logger.LogWarning(
                    LogEvents.RefreshToken.ConcurrencyConflict,
                    "Refresh token concurrency conflict. Attempt: {Attempt}, MaxAttempts: {MaxAttempts}",
                    attempts,
                    maxAttempts);

                if (attempts == maxAttempts)
                {
                    _logger.LogError(
                        LogEvents.RefreshToken.Failed,
                        "Refresh token operation failed after maximum concurrency retries. Attempts: {Attempts}",
                        attempts);

                    throw new ConflictException(
                        "توکن در حال تغییر است. لطفاً دوباره تلاش کنید.");
                }
            }
            catch (Exception ex)
            {
                await _unitOfWorkContract
                    .RollbackTransactionAsync();

                _logger.LogError(
                    LogEvents.RefreshToken.Failed,
                    ex,
                    "Unexpected error occurred during refresh token operation.");

                throw;
            }
        }

        throw new InvalidOperationException(
            "خطای غیر منتظره");
    }

    public async Task<ProfileResponseDto> ViewProfileAsync()
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(ViewProfileAsync));

        var userId = _userContext.UserId
                     ?? throw new UnauthorizedAccessException(
                         "کاربر احراز هویت نشده است.");

        activity?.SetTag(
            "user.id",
            userId);

        var user =
            await _userRepository.GetUserByIdAsync(userId);

        if (user is null)
            throw new NotFoundException(
                "کاربر یافت نشد .");

        activity?.SetStatus(
            ActivityStatusCode.Ok);

        return new ProfileResponseDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role.ToString()
        };
    }

    public async Task<ProfileResponseDto> UpdateProfileAsync(
        UpdateProfileRequestDto dto)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(UpdateProfileAsync));

        _logger.LogInformation(
            LogEvents.Profile.UpdateStarted,
            "User profile update started.");

        var userId = _userContext.UserId
                     ?? throw new UnauthorizedAccessException(
                         "کاربر احراز هویت نشده است.");

        activity?.SetTag(
            "user.id",
            userId);

        var user =
            await _userRepository.GetUserByIdAsync(userId)
            ?? throw new NotFoundException(
                "کاربر یافت نشد .");

        user.UpdateProfile(
            dto.FullName,
            dto.PhoneNumber);

        await _unitOfWorkContract.SaveAsync();

        activity?.SetStatus(
            ActivityStatusCode.Ok);

        _logger.LogInformation(
            LogEvents.Profile.UpdateSucceeded,
            "User profile updated successfully. UserId: {UserId}",
            userId);

        return new ProfileResponseDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role.ToString()
        };
    }

    public async Task<List<ViewUser>> GetAllUsersAsync()
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(GetAllUsersAsync));

        var users =
            await _userRepository.GetAllUsersAsync();

        activity?.SetTag(
            "users.count",
            users.Count);

        return users;
    }
}