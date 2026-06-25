using System.Text.Json;
using ContractorIQ.API.Data;
using ContractorIQ.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContractorIQ.API.Services;

public class MatchingService : IMatchingService
{
    private readonly AppDbContext _db;
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<MatchingService> _logger;

    public MatchingService(AppDbContext db, IHttpClientFactory factory, IConfiguration config, ILogger<MatchingService> logger)
    {
        _db = db;
        _http = factory.CreateClient("openai");
        _config = config;
        _logger = logger;
    }

    public async Task ScoreJobsForUserAsync(Guid userId)
    {
        var profile = await _db.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null) return;

        var profileText = BuildProfileText(profile);
        var profileVector = await GetEmbeddingAsync(profileText);
        if (profileVector == null) return;

        var jobs = await _db.Jobs.Where(j => j.IsActive).ToListAsync();

        foreach (var job in jobs)
        {
            try
            {
                var jobText = $"{job.Title} {job.Company} {job.Description} {job.TechStack} {job.Location} {job.Ir35Status}";
                var jobVector = await GetEmbeddingAsync(jobText);
                if (jobVector == null) continue;

                var score = CosineSimilarity(profileVector, jobVector);
                // Normalise to 0-100, cosine similarity is -1 to 1
                job.MatchScore = (float)Math.Round((score + 1) / 2 * 100, 1);

                // Boost for IR35 match
                if (profile.Ir35Preference == "outside" && job.Ir35Status == "outside")
                    job.MatchScore = Math.Min(100, job.MatchScore.Value + 5);
                if (profile.Ir35Preference == "outside" && job.Ir35Status == "inside")
                    job.MatchScore = Math.Max(0, job.MatchScore.Value - 10);

                // Boost for day rate match
                if (job.DayRateMin.HasValue && profile.DesiredDayRateMin > 0)
                {
                    if (job.DayRateMax >= profile.DesiredDayRateMin)
                        job.MatchScore = Math.Min(100, job.MatchScore.Value + 3);
                    else
                        job.MatchScore = Math.Max(0, job.MatchScore.Value - 5);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to score job {JobId}: {Error}", job.Id, ex.Message);
            }
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Scored {Count} jobs for user {UserId}", jobs.Count, userId);
    }

    public async Task ScoreJobAsync(Guid jobId, Guid userId)
    {
        var profile = await _db.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);
        var job = await _db.Jobs.FindAsync(jobId);
        if (profile == null || job == null) return;

        var profileText = BuildProfileText(profile);
        var jobText = $"{job.Title} {job.Company} {job.Description} {job.TechStack} {job.Location}";

        var profileVector = await GetEmbeddingAsync(profileText);
        var jobVector = await GetEmbeddingAsync(jobText);
        if (profileVector == null || jobVector == null) return;

        var score = CosineSimilarity(profileVector, jobVector);
        job.MatchScore = (float)Math.Round((score + 1) / 2 * 100, 1);
        await _db.SaveChangesAsync();
    }

    private string BuildProfileText(Entities.ContractorProfile profile)
    {
        return $"{profile.JobTitle} {profile.Summary} {profile.Skills} " +
               $"location: {profile.PreferredLocation} " +
               $"ir35: {profile.Ir35Preference} " +
               $"rate: {profile.DesiredDayRateMin}-{profile.DesiredDayRateMax}";
    }

    private async Task<float[]?> GetEmbeddingAsync(string text)
    {
        var apiKey = _config["OpenAI:ApiKey"];
        var model = _config["OpenAI:EmbeddingModel"] ?? "text-embedding-3-small";

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/embeddings");
        request.Headers.Add("Authorization", $"Bearer {apiKey}");
        request.Content = JsonContent.Create(new { input = text[..Math.Min(text.Length, 8000)], model });

        var response = await _http.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("OpenAI embedding failed: {Status}", response.StatusCode);
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var embedding = doc.RootElement
            .GetProperty("data")[0]
            .GetProperty("embedding")
            .EnumerateArray()
            .Select(e => e.GetSingle())
            .ToArray();

        return embedding;
    }

    private static float CosineSimilarity(float[] a, float[] b)
    {
        float dot = 0, magA = 0, magB = 0;
        for (int i = 0; i < Math.Min(a.Length, b.Length); i++)
        {
            dot += a[i] * b[i];
            magA += a[i] * a[i];
            magB += b[i] * b[i];
        }
        if (magA == 0 || magB == 0) return 0;
        return dot / (float)(Math.Sqrt(magA) * Math.Sqrt(magB));
    }
}