using ContractorIQ.API.Data;
using ContractorIQ.API.DTOs.Applications;
using ContractorIQ.API.Entities;
using ContractorIQ.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContractorIQ.API.Services;

public class ApplicationService : IApplicationService
{
    private readonly AppDbContext _db;

    public ApplicationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ApplicationResponse>> GetAllAsync(Guid userId)
    {
        var applications = await _db.Applications
            .Where(a => a.UserId == userId)
            .Include(a => a.Job)
            .OrderByDescending(a => a.UpdatedAt)
            .ToListAsync();

        return applications
            .Where(a => a.Job != null)
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<ApplicationResponse?> UpdateAsync(
        Guid applicationId, Guid userId, UpdateApplicationRequest request)
    {
        var application = await _db.Applications
            .Include(a => a.Job)
            .FirstOrDefaultAsync(a => a.Id == applicationId && a.UserId == userId);

        if (application == null) return null;

        if (request.Status != null) application.Status = request.Status;
        if (request.Notes != null) application.Notes = request.Notes;
        if (request.DayRateQuoted.HasValue) application.DayRateQuoted = request.DayRateQuoted;
        if (request.RecruiterName != null) application.RecruiterName = request.RecruiterName;
        if (request.RecruiterEmail != null) application.RecruiterEmail = request.RecruiterEmail;
        if (request.RecruiterPhone != null) application.RecruiterPhone = request.RecruiterPhone;
        if (request.FollowUpAt.HasValue) application.FollowUpAt = request.FollowUpAt;
        application.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapToResponse(application);
    }

    public async Task<ApplicationResponse?> ApplyAsync(Guid jobId, Guid userId)
    {
        var existing = await _db.Applications
            .Include(a => a.Job)
            .FirstOrDefaultAsync(a => a.JobId == jobId && a.UserId == userId);

        if (existing != null)
        {
            existing.Status = "applied";
            existing.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return MapToResponse(existing);
        }

        var job = await _db.Jobs.FindAsync(jobId);
        if (job == null) return null;

        var application = new Application
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            JobId = jobId,
            Status = "applied",
            AppliedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Job = job,
        };

        _db.Applications.Add(application);
        await _db.SaveChangesAsync();
        return MapToResponse(application);
    }

    public async Task<bool> DeleteAsync(Guid applicationId, Guid userId)
    {
        var application = await _db.Applications
            .FirstOrDefaultAsync(a => a.Id == applicationId && a.UserId == userId);

        if (application == null) return false;

        _db.Applications.Remove(application);
        await _db.SaveChangesAsync();
        return true;
    }

    private static ApplicationResponse MapToResponse(Application a) => new()
    {
        Id = a.Id,
        JobId = a.JobId,
        Status = a.Status,
        Notes = a.Notes,
        DayRateQuoted = a.DayRateQuoted,
        RecruiterName = a.RecruiterName,
        RecruiterEmail = a.RecruiterEmail,
        RecruiterPhone = a.RecruiterPhone,
        FollowUpAt = a.FollowUpAt,
        AppliedAt = a.AppliedAt,
        UpdatedAt = a.UpdatedAt,
        JobTitle = a.Job?.Title ?? "",
        Company = a.Job?.Company ?? "",
        Location = a.Job?.Location ?? "",
        Ir35Status = a.Job?.Ir35Status ?? "",
        DayRateMin = a.Job?.DayRateMin,
        DayRateMax = a.Job?.DayRateMax,
        TechStack = a.Job?.TechStack ?? "",
        SourceUrl = a.Job?.SourceUrl ?? "",
        MatchScore = a.Job?.MatchScore,
    };
}