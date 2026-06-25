using System.Security.Claims;
using ContractorIQ.API.Middleware;
using ContractorIQ.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContractorIQ.API.Controllers;

[ApiController]
[Route("api/cv")]
[Authorize]
public class CvTailoringController : ControllerBase
{
    private readonly ICvTailoringService _service;

    public CvTailoringController(ICvTailoringService service)
    {
        _service = service;
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    [HttpPost("tailor/{jobId:guid}")]
    [RequiresPlan("individual", "pro")]
    public async Task<IActionResult> TailorCv(Guid jobId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var pdf = await _service.TailorCvAsync(userId, jobId);
        if (pdf == null)
            return BadRequest(new { message = "Could not tailor CV. Ensure your profile has a CV uploaded." });

        return File(pdf, "application/pdf", $"tailored-cv-{jobId}.pdf");
    }
}