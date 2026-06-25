using ContractorIQ.API.Data;
using ContractorIQ.API.DTOs.Profile;
using ContractorIQ.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContractorIQ.API.Services;

public class ProfileService : IProfileService
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public ProfileService(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public async Task<ProfileResponse?> GetProfileAsync(Guid userId)
    {
        var profile = await _db.Profiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null) return null;
        return MapToResponse(profile);
    }

    public async Task<ProfileResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
    {
        var profile = await _db.Profiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
            throw new InvalidOperationException("Profile not found.");

        profile.JobTitle = request.JobTitle?.Trim();
        profile.Summary = request.Summary?.Trim();
        profile.Skills = request.Skills?.Trim();
        profile.PreferredLocation = request.PreferredLocation?.Trim();
        profile.RemoteOnly = request.RemoteOnly;
        profile.DesiredDayRateMin = request.DesiredDayRateMin;
        profile.DesiredDayRateMax = request.DesiredDayRateMax;
        profile.Ir35Preference = request.Ir35Preference?.Trim();
        profile.NoticePeriod = request.NoticePeriod?.Trim();
        profile.LinkedInUrl = request.LinkedInUrl?.Trim();
        profile.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapToResponse(profile);
    }

    public async Task<string> UploadCvAsync(Guid userId, IFormFile file)
    {
        // Validate file
        if (file.Length == 0)
            throw new ArgumentException("File is empty.");

        if (file.Length > 10 * 1024 * 1024)
            throw new ArgumentException("File size exceeds 10MB limit.");

        var allowedTypes = new[] { ".pdf", ".doc", ".docx" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedTypes.Contains(ext))
            throw new ArgumentException("Only PDF, DOC, and DOCX files are allowed.");

        // Save to local uploads folder
        var uploadsPath = Path.Combine(_env.ContentRootPath, "uploads", "cvs");
        Directory.CreateDirectory(uploadsPath);

        var fileName = $"{userId}{ext}";
        var filePath = Path.Combine(uploadsPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Update profile
        var profile = await _db.Profiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
            throw new InvalidOperationException("Profile not found.");

        profile.MasterCvBlobUrl = filePath;
        profile.MasterCvFileName = file.FileName;
        profile.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return file.FileName;
    }

    public async Task<(byte[] bytes, string fileName, string contentType)> DownloadCvAsync(Guid userId)
    {
        var profile = await _db.Profiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null || string.IsNullOrEmpty(profile.MasterCvBlobUrl))
            throw new FileNotFoundException("No CV found for this user.");

        if (!File.Exists(profile.MasterCvBlobUrl))
            throw new FileNotFoundException("CV file not found on server.");

        var bytes = await File.ReadAllBytesAsync(profile.MasterCvBlobUrl);
        var ext = Path.GetExtension(profile.MasterCvFileName ?? "cv.pdf").ToLowerInvariant();

        var contentType = ext switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };

        return (bytes, profile.MasterCvFileName ?? "cv.pdf", contentType);
    }

    private ProfileResponse MapToResponse(Entities.ContractorProfile profile)
    {
        var score = CalculateCompletionScore(profile);
        return new ProfileResponse
        {
            Id = profile.Id,
            UserId = profile.UserId,
            JobTitle = profile.JobTitle,
            Summary = profile.Summary,
            Skills = profile.Skills,
            PreferredLocation = profile.PreferredLocation,
            RemoteOnly = profile.RemoteOnly,
            DesiredDayRateMin = profile.DesiredDayRateMin,
            DesiredDayRateMax = profile.DesiredDayRateMax,
            Ir35Preference = profile.Ir35Preference,
            NoticePeriod = profile.NoticePeriod,
            LinkedInUrl = profile.LinkedInUrl,
            MasterCvFileName = profile.MasterCvFileName,
            HasCv = !string.IsNullOrEmpty(profile.MasterCvBlobUrl),
            ProfileCompletionScore = score,
            UpdatedAt = profile.UpdatedAt
        };
    }

    private int CalculateCompletionScore(Entities.ContractorProfile profile)
    {
        var score = 0;
        if (!string.IsNullOrEmpty(profile.JobTitle)) score += 15;
        if (!string.IsNullOrEmpty(profile.Summary)) score += 15;
        if (!string.IsNullOrEmpty(profile.Skills)) score += 20;
        if (!string.IsNullOrEmpty(profile.PreferredLocation)) score += 10;
        if (profile.DesiredDayRateMin > 0) score += 15;
        if (!string.IsNullOrEmpty(profile.Ir35Preference)) score += 10;
        if (!string.IsNullOrEmpty(profile.NoticePeriod)) score += 5;
        if (!string.IsNullOrEmpty(profile.MasterCvBlobUrl)) score += 10;
        return score;
    }
}