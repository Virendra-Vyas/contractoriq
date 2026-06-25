using System.Text;
using System.Text.Json;
using ContractorIQ.API.Data;
using ContractorIQ.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContractorIQ.API.Services;

public class AlertService : IAlertService
{
    private readonly AppDbContext _db;
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<AlertService> _logger;

    public AlertService(
        AppDbContext db,
        IHttpClientFactory factory,
        IConfiguration config,
        ILogger<AlertService> logger)
    {
        _db = db;
        _http = factory.CreateClient("resend");
        _config = config;
        _logger = logger;
    }

    public async Task ProcessAlertsAsync()
    {
        // Get all users with alerts enabled
        var profiles = await _db.Profiles
            .Include(p => p.User)
            .Where(p => p.AlertsEnabled)
            .ToListAsync();

        if (!profiles.Any())
        {
            _logger.LogInformation("No users with alerts enabled.");
            return;
        }

        // Get jobs scraped in the last 24 hours
        var since = DateTime.UtcNow.AddHours(-24);
        var newJobs = await _db.Jobs
            .Where(j => j.IsActive && j.ScrapedAt >= since)
            .ToListAsync();

        if (!newJobs.Any())
        {
            _logger.LogInformation("No new jobs in last 24 hours.");
            return;
        }

        foreach (var profile in profiles)
        {
            try
            {
                var matchingJobs = newJobs.Where(job =>
                {
                    // Match score filter
                    if (job.MatchScore.HasValue && job.MatchScore < profile.AlertMinMatchScore)
                        return false;

                    // Min day rate filter
                    if (profile.AlertMinDayRate > 0 && job.DayRateMax.HasValue
                        && job.DayRateMax < profile.AlertMinDayRate)
                        return false;

                    // IR35 filter
                    if (!string.IsNullOrEmpty(profile.AlertIr35Preference)
                        && profile.AlertIr35Preference != "either"
                        && job.Ir35Status != "unknown"
                        && job.Ir35Status != profile.AlertIr35Preference)
                        return false;

                    // Keywords filter
                    if (!string.IsNullOrEmpty(profile.AlertKeywords))
                    {
                        var keywords = profile.AlertKeywords
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(k => k.Trim().ToLower());

                        var jobText = $"{job.Title} {job.TechStack} {job.Description}".ToLower();
                        if (!keywords.Any(k => jobText.Contains(k)))
                            return false;
                    }

                    return true;
                }).OrderByDescending(j => j.MatchScore ?? 0).Take(10).ToList();

                if (!matchingJobs.Any()) continue;

                await SendAlertEmailAsync(profile.User.Email,
                    profile.User.FirstName, matchingJobs);

                profile.LastAlertSentAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError("Alert failed for user {UserId}: {Error}",
                    profile.UserId, ex.Message);
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task SendTestAlertAsync(Guid userId)
    {
        var profile = await _db.Profiles
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null) return;

        var sampleJobs = await _db.Jobs
            .Where(j => j.IsActive)
            .OrderByDescending(j => j.MatchScore)
            .Take(3)
            .ToListAsync();

        if (!sampleJobs.Any()) return;

        await SendAlertEmailAsync(profile.User.Email, profile.User.FirstName, sampleJobs);
    }

    private async Task SendAlertEmailAsync(
        string toEmail,
        string firstName,
        List<Entities.Job> jobs)
    {
        var apiKey = _config["Resend:ApiKey"];
        var fromEmail = _config["Resend:FromEmail"] ?? "alerts@contractoriq.co.uk";
        var fromName = _config["Resend:FromName"] ?? "ContractorIQ";

        var html = BuildEmailHtml(firstName, jobs);

        var payload = new
        {
            from = $"{fromName} <{fromEmail}>",
            to = new[] { toEmail },
            subject = $"🎯 {jobs.Count} new contract jobs matching your profile",
            html
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
        request.Headers.Add("Authorization", $"Bearer {apiKey}");
        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        var response = await _http.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Resend failed: {Status} {Error}", response.StatusCode, err);
        }
        else
        {
            _logger.LogInformation("Alert email sent to {Email} with {Count} jobs",
                toEmail, jobs.Count);
        }
    }

    private static string BuildEmailHtml(string firstName, List<Entities.Job> jobs)
{
    var sb = new StringBuilder();

    sb.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><style>");
    sb.Append("body{font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;background:#f9fafb;margin:0;padding:20px;}");
    sb.Append(".container{max-width:600px;margin:0 auto;}");
    sb.Append(".header{background:#6366f1;color:white;padding:24px 28px;border-radius:10px 10px 0 0;}");
    sb.Append(".header h1{margin:0;font-size:20px;}");
    sb.Append(".header p{margin:6px 0 0;opacity:0.85;font-size:14px;}");
    sb.Append(".body{background:white;padding:20px 28px;border-radius:0 0 10px 10px;border:1px solid #e5e7eb;}");
    sb.Append(".job{border:1px solid #e5e7eb;border-radius:8px;padding:16px;margin-bottom:12px;}");
    sb.Append(".job-title{font-size:15px;font-weight:600;color:#111827;margin:0 0 4px;}");
    sb.Append(".job-company{font-size:13px;color:#6b7280;margin:0 0 10px;}");
    sb.Append(".badge{display:inline-block;border-radius:12px;padding:2px 10px;font-size:11px;font-weight:600;margin-right:6px;}");
    sb.Append(".outside{background:#dcfce7;color:#166534;}");
    sb.Append(".inside{background:#fee2e2;color:#991b1b;}");
    sb.Append(".unknown{background:#f3f4f6;color:#6b7280;}");
    sb.Append(".rate{background:#f3f4f6;color:#374151;}");
    sb.Append(".match{color:#166534;font-weight:700;}");
    sb.Append(".apply{display:inline-block;background:#6366f1;color:white;padding:8px 16px;border-radius:6px;text-decoration:none;font-size:13px;font-weight:600;margin-top:10px;}");
    sb.Append(".footer{text-align:center;padding:20px;color:#9ca3af;font-size:12px;}");
    sb.Append("</style></head><body><div class=\"container\">");

    sb.Append("<div class=\"header\">");
    sb.Append("<h1>🎯 New jobs matching your profile</h1>");
    sb.Append($"<p>Hi {firstName} — {jobs.Count} new contract role{(jobs.Count == 1 ? "" : "s")} found today</p>");
    sb.Append("</div><div class=\"body\">");

    foreach (var job in jobs)
    {
        var ir35Class = job.Ir35Status == "outside" ? "outside"
            : job.Ir35Status == "inside" ? "inside" : "unknown";
        var ir35Label = job.Ir35Status == "outside" ? "✓ Outside IR35"
            : job.Ir35Status == "inside" ? "✗ Inside IR35" : "IR35 Unknown";

        sb.Append("<div class=\"job\">");
        sb.Append($"<p class=\"job-title\">{job.Title}</p>");
        sb.Append($"<p class=\"job-company\">{job.Company} · {job.Location}</p>");
        sb.Append($"<span class=\"badge {ir35Class}\">{ir35Label}</span>");
        if (job.DayRateMax.HasValue)
            sb.Append($"<span class=\"badge rate\">£{job.DayRateMax}/day</span>");
        if (job.MatchScore.HasValue)
            sb.Append($"<span class=\"match\">{job.MatchScore}% match</span>");
        sb.Append($"<br><a href=\"{job.SourceUrl}\" class=\"apply\">View &amp; Apply →</a>");
        sb.Append("</div>");
    }

    sb.Append("<p style=\"color:#6b7280;font-size:13px;margin-top:20px;\">Log in to ");
    sb.Append("<a href=\"https://contractoriq.co.uk\" style=\"color:#6366f1\">ContractorIQ</a>");
    sb.Append(" to see AI match scores, IR35 analysis, and tailor your CV.</p>");
    sb.Append("</div>");
    sb.Append("<div class=\"footer\">ContractorIQ · You're receiving this because alerts are enabled on your account.</div>");
    sb.Append("</div></body></html>");

    return sb.ToString();
}
}