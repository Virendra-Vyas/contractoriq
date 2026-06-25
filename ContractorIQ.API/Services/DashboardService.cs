using ContractorIQ.API.Data;
using ContractorIQ.API.DTOs.Dashboard;
using ContractorIQ.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContractorIQ.API.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardDto> GetDashboardAsync(Guid userId)
    {
        var applications = await _db.Applications
            .Where(a => a.UserId == userId)
            .Include(a => a.Job)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();

        var appStats = new ApplicationStatsDto(
            Total:        applications.Count,
            Saved:        applications.Count(a => a.Status == "saved"),
            Applied:      applications.Count(a => a.Status == "applied"),
            Interviewing: applications.Count(a =>
                a.Status == "recruiter_screen" || a.Status == "client_interview"),
            Offers:       applications.Count(a => a.Status == "offer"),
            Placed:       applications.Count(a => a.Status == "placed"),
            Rejected:     applications.Count(a => a.Status == "rejected")
        );

        var matchScores = applications
            .Where(a => a.Job?.MatchScore != null)
            .Select(a => (double)a.Job!.MatchScore!.Value)
            .ToList();

        var savedCount = applications.Count(a => a.Status == "saved");

        var jobStats = new JobStatsDto(
            TotalScored:       matchScores.Count,
            SavedJobs:         savedCount,
            AverageMatchScore: matchScores.Count > 0
                ? Math.Round(matchScores.Average(), 1) : 0,
            HighMatchCount:    matchScores.Count(s => s >= 70)
        );

        var ir35Count = await _db.Ir35Analyses
            .Where(a => a.Job.Applications.Any(ap => ap.UserId == userId))
            .CountAsync();

        var cvCount = await _db.Applications
            .CountAsync(a => a.UserId == userId && a.TailoredCvBlobUrl != null);

        var usageStats = new UsageStatsDto(
            CvsTailored: cvCount,
            Ir35Screens: ir35Count
        );

        var sub = await _db.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId);

        var subSummary = new SubscriptionSummaryDto(
            Tier:     sub?.Tier ?? "free",
            Status:   sub?.Status ?? "active",
            IsActive: sub?.Status == "active"
        );

        var recent = applications
            .Take(5)
            .Select(a => new RecentApplicationDto(
                Id:         a.Id,
                JobTitle:   a.Job?.Title ?? "Unknown",
                Company:    a.Job?.Company ?? "Unknown",
                Status:     a.Status,
                MatchScore: a.Job?.MatchScore.HasValue == true
                    ? (int?)Convert.ToInt32(a.Job.MatchScore.Value) : null,
                CreatedAt:  a.AppliedAt
            ));

        return new DashboardDto(appStats, jobStats, usageStats, subSummary, recent);
    }
}