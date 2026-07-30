using Application.Features.Auth.DTOs;
using Application.Features.Auth.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShopApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _service;
    private readonly IPasswordRecoveryService _passwordRecoveryService;
    public UserController(IUserService service, IPasswordRecoveryService passwordRecoveryService)
    {
        _service = service;
        _passwordRecoveryService = passwordRecoveryService;
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequestDto registerUserRequestDto)
    {
        var res = await _service.RegisterUserAsync(registerUserRequestDto);
        return Ok(res);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserRequestDto requestDto)
    {
        return Ok(await _service.LoginUserAsync(requestDto));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest dto)
    {
        return Ok(await _service.RefreshTokenAsync(dto.RefreshToken));
    }

    [HttpGet("ViewUsers")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ViewUsers()
    {
        var res = await _service.GetAllUsersAsync();
        return Ok(res);
    }

    [HttpPost("Logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var res = await _service.LogoutUserAsync();
        return Ok(res);
    }

    [HttpGet("Profile")]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var profile = await _service.ViewProfileAsync();
        return Ok(profile);
    }

    [HttpPatch("Profile")]
    [Authorize]
    public async Task<ProfileResponseDto> UpdateProfile([FromBody] UpdateProfileRequestDto updateProfileRequestDto)
    {
        return await _service.UpdateProfileAsync(updateProfileRequestDto);
    }
    
    [HttpPost("ForgetPassword")]
    public async Task<IActionResult> ForgetPassword(ForgetPasswordRequestDto dto)
    {
        var result= await _passwordRecoveryService.ForgetPasswordAsync(dto.Email);
        return Ok(new
        {
            message=result
        });
    }

    [HttpPost("ResetPassword")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequestDto dto)
    {
        var result= await _passwordRecoveryService.ResetPasswordAsync(
            dto.Email,
            dto.Code,
            dto.NewPassword);
        return Ok(new
        {
            message = result
        });
    }
    
}