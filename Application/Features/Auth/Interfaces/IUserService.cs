using Application.Features.Auth.DTOs;
using Domain.Enums;

namespace Application.Features.Auth.Interfaces;

public interface IUserService
{
    Task<string> RegisterUserAsync(RegisterUserRequestDto registerUserRequestDto);
    Task<LoginUserResponseDto> LoginUserAsync(LoginUserRequestDto loginUserRequestDto);
    Task ChangeRoleTo(UserRole role);
    Task<string> LogoutUserAsync();
    Task<LoginUserResponseDto> RefreshTokenAsync(string refreshToken);
    Task<ProfileResponseDto> ViewProfileAsync();
    Task<ProfileResponseDto> UpdateProfileAsync(UpdateProfileRequestDto updateProfileRequestDto);
    Task<List<ViewUser>> GetAllUsersAsync();
}