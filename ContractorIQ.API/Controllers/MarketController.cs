using ContractorIQ.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContractorIQ.API.Controllers;

[ApiController]
[Route("api/market")]
[Authorize]
public class MarketController : ControllerBase
{
    private readonly IMarketRateService _marketRate;

    public MarketController(IMarketRateService marketRate)
    {
        _marketRate = marketRate;
    }

    [HttpGet("rates")]
    public async Task<IActionResult> GetRates(
        [FromQuery] string? techStack,
        [FromQuery] string? location,
        [FromQuery] string? ir35Status,
        [FromQuery] decimal? jobRate)
    {
        var result = await _marketRate.GetMarketRateAsync(techStack, location, ir35Status, jobRate);
        if (result == null) return NotFound(new { message = "Not enough data." });
        return Ok(result);
    }
}