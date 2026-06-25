using System.Security.Claims;
using ContractorIQ.API.DTOs.Profile;
using ContractorIQ.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContractorIQ.API.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var profile = await _profileService.GetProfileAsync(userId);
        if (profile == null) return NotFound();

        return Ok(profile);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        try
        {
            var profile = await _profileService.UpdateProfileAsync(userId, request);
            return Ok(profile);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("cv")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadCv(IFormFile file)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file provided." });

        try
        {
            var fileName = await _profileService.UploadCvAsync(userId, file);
            return Ok(new { message = "CV uploaded successfully.", fileName });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("cv")]
    public async Task<IActionResult> DownloadCv()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        try
        {
            var (bytes, fileName, contentType) = await _profileService.DownloadCvAsync(userId);
            return File(bytes, contentType, fileName);
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}