using System.Security.Claims;
using ContractorIQ.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContractorIQ.API.Controllers;

[ApiController]
[Route("api/matching")]
[Authorize]
public class MatchingController : ControllerBase
{
    private readonly IMatchingService _matching;

    public MatchingController(IMatchingService matching)
    {
        _matching = matching;
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    [HttpPost("score-all")]
    public async Task<IActionResult> ScoreAll()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        await _matching.ScoreJobsForUserAsync(userId);
        return Ok(new { message = "Scoring complete." });
    }

    [HttpPost("score/{jobId:guid}")]
    public async Task<IActionResult> ScoreJob(Guid jobId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        await _matching.ScoreJobAsync(jobId, userId);
        return Ok(new { message = "Job scored." });
    }
}