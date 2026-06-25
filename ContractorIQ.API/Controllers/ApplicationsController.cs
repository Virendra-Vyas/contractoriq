using System.Security.Claims;
using ContractorIQ.API.DTOs.Applications;
using ContractorIQ.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContractorIQ.API.Controllers;

[ApiController]
[Route("api/applications")]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _service;

    public ApplicationsController(IApplicationService service)
    {
        _service = service;
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();
        var result = await _service.GetAllAsync(userId);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateApplicationRequest request)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();
        var result = await _service.UpdateAsync(id, userId, request);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("apply/{jobId:guid}")]
    public async Task<IActionResult> Apply(Guid jobId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();
        var result = await _service.ApplyAsync(jobId, userId);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();
        var deleted = await _service.DeleteAsync(id, userId);
        if (!deleted) return NotFound();
        return Ok(new { message = "Removed." });
    }
}