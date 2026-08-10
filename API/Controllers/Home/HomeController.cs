using Application.Features.Home.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ShopApi.Controllers.Home;

[ApiController]
[Route("api/[controller]")]
public class HomeController : ControllerBase
{
    private readonly HomeServiceContract _homeServiceContract;

    public HomeController(HomeServiceContract homeServiceContract)
    {
        _homeServiceContract = homeServiceContract;
    }

    [HttpGet]
    public async Task<IActionResult> GetData()
    {
        var data = await _homeServiceContract.GetHomeAsync();
        return Ok(data);
    }
}