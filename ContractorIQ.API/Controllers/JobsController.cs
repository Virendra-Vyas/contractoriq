using System.Security.Claims;
using ContractorIQ.API.DTOs.Jobs;
using ContractorIQ.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContractorIQ.API.Controllers;

[ApiController]
[Route("api/jobs")]
[Authorize]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;

    public JobsController(IJobService jobService)
    {
        _jobService = jobService;
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    [HttpGet]
    public async Task<IActionResult> GetJobs([FromQuery] JobListRequest request)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _jobService.GetJobsAsync(request, userId);
        return Ok(result);
    }

    [HttpGet("saved")]
    public async Task<IActionResult> GetSavedJobs([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _jobService.GetSavedJobsAsync(userId, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetJob(Guid id)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var job = await _jobService.GetJobByIdAsync(id, userId);
        if (job == null) return NotFound();

        return Ok(job);
    }

    [HttpPost("{id:guid}/save")]
    public async Task<IActionResult> SaveJob(Guid id)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var saved = await _jobService.SaveJobAsync(id, userId);
        if (!saved) return Conflict(new { message = "Job already saved." });

        return Ok(new { message = "Job saved." });
    }

    [HttpDelete("{id:guid}/save")]
    public async Task<IActionResult> UnsaveJob(Guid id)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var removed = await _jobService.UnsaveJobAsync(id, userId);
        if (!removed) return NotFound(new { message = "Saved job not found." });

        return Ok(new { message = "Job removed from saved." });
    }
}