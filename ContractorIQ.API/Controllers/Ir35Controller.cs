using ContractorIQ.API.Middleware;
using ContractorIQ.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContractorIQ.API.Controllers;

[ApiController]
[Route("api/ir35")]
[Authorize]
public class Ir35Controller : ControllerBase
{
    private readonly IIr35ScreenerService _screener;

    public Ir35Controller(IIr35ScreenerService screener)
    {
        _screener = screener;
    }

    [HttpGet("{jobId:guid}")]
    [RequiresPlan("individual", "pro")]
    public async Task<IActionResult> Analyse(Guid jobId)
    {
        var result = await _screener.AnalyseJobAsync(jobId);
        if (result == null) return NotFound();
        return Ok(result);
    }
}