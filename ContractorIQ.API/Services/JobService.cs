using ContractorIQ.API.Data;
using ContractorIQ.API.DTOs.Jobs;
using ContractorIQ.API.Entities;
using ContractorIQ.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContractorIQ.API.Services;

public class JobService : IJobService
{
    private readonly AppDbContext _db;

    public JobService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<JobResponse>> GetJobsAsync(JobListRequest request, Guid userId)
    {
        var query = _db.Jobs.Where(j => j.IsActive);

        // Filters
        if (!string.IsNullOrEmpty(request.Keywords))
        {
            var kw = request.Keywords.ToLower();
            query = query.Where(j =>
                j.Title.ToLower().Contains(kw) ||
                j.Description.ToLower().Contains(kw) ||
                j.TechStack.ToLower().Contains(kw) ||
                j.Company.ToLower().Contains(kw));
        }

        if (!string.IsNullOrEmpty(request.Location))
        {
            var loc = request.Location.ToLower();
            query = query.Where(j => j.Location.ToLower().Contains(loc));
        }

        if (!string.IsNullOrEmpty(request.Ir35Status))
            query = query.Where(j => j.Ir35Status == request.Ir35Status);

        if (!string.IsNullOrEmpty(request.Source))
            query = query.Where(j => j.Source == request.Source);

        if (request.DayRateMin.HasValue)
            query = query.Where(j => j.DayRateMax >= request.DayRateMin);

        if (request.DayRateMax.HasValue)
            query = query.Where(j => j.DayRateMin <= request.DayRateMax);

        if (request.RemoteOnly == true)
            query = query.Where(j => j.IsRemote);

        if (!string.IsNullOrEmpty(request.TechStack))
        {
            var techs = request.TechStack.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var tech in techs)
            {
                var t = tech.Trim().ToLower();
                query = query.Where(j => j.TechStack.ToLower().Contains(t));
            }
        }

        // Sort
        query = request.SortBy switch
        {
            "day_rate" => query.OrderByDescending(j => j.DayRateMax),
            "match_score" => query.OrderByDescending(j => j.MatchScore),
            _ => query.OrderByDescending(j => j.PostedAt)
        };

        var totalCount = await query.CountAsync();

        var jobs = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        // Get saved job IDs for this user
        var savedJobIds = await _db.Applications
            .Where(a => a.UserId == userId && a.Status == "saved")
            .Select(a => a.JobId)
            .ToHashSetAsync();

        return new PagedResult<JobResponse>
        {
            Items = jobs.Select(j => MapToResponse(j, savedJobIds.Contains(j.Id))).ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<JobResponse?> GetJobByIdAsync(Guid id, Guid userId)
    {
        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == id);
        if (job == null) return null;

        var isSaved = await _db.Applications
            .AnyAsync(a => a.UserId == userId && a.JobId == id && a.Status == "saved");

        return MapToResponse(job, isSaved);
    }

    public async Task<bool> SaveJobAsync(Guid jobId, Guid userId)
    {
        var exists = await _db.Applications
            .AnyAsync(a => a.UserId == userId && a.JobId == jobId);

        if (exists) return false;

        _db.Applications.Add(new Application
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            JobId = jobId,
            Status = "saved",
            AppliedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnsaveJobAsync(Guid jobId, Guid userId)
    {
        var application = await _db.Applications
            .FirstOrDefaultAsync(a => a.UserId == userId && a.JobId == jobId && a.Status == "saved");

        if (application == null) return false;

        _db.Applications.Remove(application);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<PagedResult<JobResponse>> GetSavedJobsAsync(Guid userId, int page, int pageSize)
    {
        var query = _db.Applications
            .Where(a => a.UserId == userId && a.Status == "saved")
            .Include(a => a.Job)
            .OrderByDescending(a => a.AppliedAt);

        var totalCount = await query.CountAsync();

        var applications = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<JobResponse>
        {
            Items = applications
                .Where(a => a.Job != null)
                .Select(a => MapToResponse(a.Job!, true))
                .ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    private static JobResponse MapToResponse(Job job, bool isSaved) => new()
    {
        Id = job.Id,
        ExternalId = job.ExternalId,
        Source = job.Source,
        Title = job.Title,
        Company = job.Company,
        Location = job.Location,
        IsRemote = job.IsRemote,
        IsHybrid = job.IsHybrid,
        DayRateMin = job.DayRateMin,
        DayRateMax = job.DayRateMax,
        Ir35Status = job.Ir35Status,
        ContractLength = job.ContractLength,
        Description = job.Description,
        TechStack = job.TechStack,
        SourceUrl = job.SourceUrl,
        PostedAt = job.PostedAt,
        ScrapedAt = job.ScrapedAt,
        MatchScore = job.MatchScore,
        IsSaved = isSaved
    };
}