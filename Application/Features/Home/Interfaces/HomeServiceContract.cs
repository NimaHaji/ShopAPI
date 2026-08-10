using Application.Features.Home.DTOs;

namespace Application.Features.Home.Interfaces;

public interface HomeServiceContract
{
    Task<HomeDto> GetHomeAsync();
}