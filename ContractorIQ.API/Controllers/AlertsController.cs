using System.Security.Claims;
using ContractorIQ.API.DTOs.Alerts;
using ContractorIQ.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContractorIQ.API.Data;

namespace ContractorIQ.API.Controllers;

[ApiController]
[Route("api/alerts")]
[Authorize]
public class AlertsController : ControllerBase
{
    private readonly IAlertService _alertService;
    private readonly AppDbContext _db;

    public AlertsController(IAlertService alertService, AppDbContext db)
    {
        _alertService = alertService;
        _db = db;
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings()
    {
        var userId = GetUserId();
        var profile = await _db.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null) return NotFound();

        return Ok(new AlertSettingsResponse
        {
            AlertsEnabled = profile.AlertsEnabled,
            AlertKeywords = profile.AlertKeywords,
            AlertMinDayRate = profile.AlertMinDayRate,
            AlertIr35Preference = profile.AlertIr35Preference,
            AlertMinMatchScore = profile.AlertMinMatchScore,
            LastAlertSentAt = profile.LastAlertSentAt,
        });
    }

    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateAlertSettingsRequest request)
    {
        var userId = GetUserId();
        var profile = await _db.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null) return NotFound();

        profile.AlertsEnabled = request.AlertsEnabled;
        profile.AlertKeywords = request.AlertKeywords;
        profile.AlertMinDayRate = request.AlertMinDayRate;
        profile.AlertIr35Preference = request.AlertIr35Preference;
        profile.AlertMinMatchScore = request.AlertMinMatchScore;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Alert settings saved." });
    }

    [HttpPost("process")]
    [AllowAnonymous] // Called by Python scraper
    public async Task<IActionResult> Process()
    {
        await _alertService.ProcessAlertsAsync();
        return Ok(new { message = "Alerts processed." });
    }

    [HttpPost("test")]
    public async Task<IActionResult> SendTest()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();
        await _alertService.SendTestAlertAsync(userId);
        return Ok(new { message = "Test alert sent." });
    }
}