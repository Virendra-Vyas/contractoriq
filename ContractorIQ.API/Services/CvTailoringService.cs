using System.Text;
using System.Text.Json;
using ContractorIQ.API.Data;
using ContractorIQ.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using UglyToad.PdfPig;

namespace ContractorIQ.API.Services;

public record TailoredCvData(
    string? Name, string? Email, string? Phone, string? Location,
    string? JobTitle, string? Summary,
    List<string>? Skills,
    List<TailoredExperience>? Experience,
    List<TailoredEducation>? Education
);

public record TailoredExperience(string? Company, string? Role, string? Period, List<string>? Bullets);
public record TailoredEducation(string? Institution, string? Qualification, string? Year);

public class CvTailoringService : ICvTailoringService
{
    private readonly AppDbContext _db;
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<CvTailoringService> _logger;

    public CvTailoringService(
        AppDbContext db,
        IHttpClientFactory factory,
        IConfiguration config,
        IWebHostEnvironment env,
        ILogger<CvTailoringService> logger)
    {
        _db = db;
        _http = factory.CreateClient("openai");
        _config = config;
        _env = env;
        _logger = logger;
    }

    public async Task<byte[]?> TailorCvAsync(Guid userId, Guid jobId)
    {
        try
        {
            var user = await _db.Users.FindAsync(userId);
            var profile = await _db.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);
            var job = await _db.Jobs.FindAsync(jobId);

            if (user == null || profile == null || job == null) return null;
            if (string.IsNullOrEmpty(profile.MasterCvFileName)) return null;

            var ext = Path.GetExtension(profile.MasterCvFileName);
            var cvPath = Path.Combine(_env.ContentRootPath, "uploads", "cvs", $"{userId}{ext}");
            if (!File.Exists(cvPath)) return null;

            var cvText = ExtractCvText(cvPath);
            var tailored = await CallGptAsync(cvText, job, user, profile);
            if (tailored == null) return null;

            return GeneratePdf(tailored);
        }
        catch (Exception ex)
        {
            _logger.LogError("CV tailoring failed for user {UserId} job {JobId}: {Error}", userId, jobId, ex.Message);
            return null;
        }
    }

    private string ExtractCvText(string path)
    {
        var ext = Path.GetExtension(path).ToLower();
        if (ext != ".pdf") return File.ReadAllText(path);

        var sb = new StringBuilder();
        using var doc = PdfDocument.Open(path);
        foreach (var page in doc.GetPages())
            sb.AppendLine(page.Text);
        return sb.ToString();
    }

    private async Task<TailoredCvData?> CallGptAsync(
        string cvText,
        Entities.Job job,
        Entities.User user,
        Entities.ContractorProfile profile)
    {
        var apiKey = _config["OpenAI:ApiKey"];

        var systemPrompt = """
            You are an expert UK IT contractor CV writer.
            Rewrite the provided CV to better match the given job description.
            Rules:
            - NEVER fabricate experience, companies, dates, or qualifications
            - Reorder and emphasise skills that match the job
            - Rewrite the summary to target this specific role
            - Keep bullet points concise and achievement-focused
            - Include ALL jobs from the candidate's CV, not just the most recent one
            - Include the full education section
            - If the CV has 3+ jobs, include all of them
            - Return ONLY valid JSON with no markdown, no backticks, no preamble

            Return this exact JSON structure:
            {
              "name": "full name",
              "email": "email",
              "phone": "phone or empty string",
              "location": "location",
              "jobTitle": "target job title for this role",
              "summary": "2-3 sentence professional summary targeting this role",
              "skills": ["skill1", "skill2", "skill3"],
              "experience": [
                {
                  "company": "company name",
                  "role": "job title",
                  "period": "date range e.g. Jan 2022 – Present",
                  "bullets": ["achievement 1", "achievement 2", "achievement 3"]
                }
              ],
              "education": [
                {
                  "institution": "university name",
                  "qualification": "degree title",
                  "year": "year"
                }
              ]
            }
            """;

        var userMessage = $"""
            JOB TITLE: {job.Title}
            COMPANY: {job.Company}
            LOCATION: {job.Location}
            IR35: {job.Ir35Status}
            TECH STACK: {job.TechStack}

            JOB DESCRIPTION:
            {job.Description[..Math.Min(job.Description.Length, 3000)]}

            ---

            CANDIDATE NAME: {user.FirstName} {user.LastName}
            CANDIDATE EMAIL: {user.Email}
            PREFERRED LOCATION: {profile.PreferredLocation}
            SKILLS: {profile.Skills}

            CANDIDATE CV TEXT:
            {cvText[..Math.Min(cvText.Length, 7000)]}
            """;

        var requestBody = new
        {
            model = "gpt-4o",
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userMessage }
            },
            response_format = new { type = "json_object" },
            max_tokens = 4000
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        request.Headers.Add("Authorization", $"Bearer {apiKey}");
        request.Content = JsonContent.Create(requestBody);

        var response = await _http.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("GPT-4o failed: {Status} {Error}", response.StatusCode, err);
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrEmpty(content)) return null;

        return JsonSerializer.Deserialize<TailoredCvData>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    private byte[] GeneratePdf(TailoredCvData cv)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(45);
                page.MarginVertical(40);
                page.DefaultTextStyle(x => x.FontSize(9.5f));

                page.Content().Column(col =>
                {
                    col.Spacing(14);

                    // Header
                    col.Item().Column(h =>
                    {
                        h.Item().Text(cv.Name ?? "").FontSize(24).Bold();
                        h.Item().PaddingTop(2).Text(cv.JobTitle ?? "").FontSize(13).FontColor("#6366f1");
                        var contact = string.Join("  |  ", new[] { cv.Email, cv.Phone, cv.Location }
                            .Where(s => !string.IsNullOrEmpty(s)));
                        h.Item().PaddingTop(5).Text(contact).FontSize(8.5f).FontColor("#6b7280");
                    });

                    col.Item().LineHorizontal(1.5f).LineColor("#6366f1");

                    // Summary
                    if (!string.IsNullOrEmpty(cv.Summary))
                    {
                        col.Item().Column(s =>
                        {
                            s.Item().Text("PROFESSIONAL SUMMARY").FontSize(8.5f).Bold().FontColor("#6366f1");
                            s.Item().PaddingTop(4).Text(cv.Summary).LineHeight(1.5f);
                        });
                    }

                    // Skills
                    if (cv.Skills?.Count > 0)
                    {
                        col.Item().Column(s =>
                        {
                            s.Item().Text("CORE SKILLS").FontSize(8.5f).Bold().FontColor("#6366f1");
                            s.Item().PaddingTop(4).Text(string.Join("   •   ", cv.Skills)).LineHeight(1.5f);
                        });
                    }

                    // Experience
                    if (cv.Experience?.Count > 0)
                    {
                        col.Item().Column(e =>
                        {
                            e.Item().Text("EXPERIENCE").FontSize(8.5f).Bold().FontColor("#6366f1");

                            foreach (var exp in cv.Experience)
                            {
                                e.Item().PaddingTop(8).Column(j =>
                                {
                                    j.Item().Row(r =>
                                    {
                                        r.RelativeItem().Text(exp.Role ?? "").Bold().FontSize(10.5f);
                                        r.AutoItem().AlignRight().Text(exp.Period ?? "").FontSize(8.5f).FontColor("#6b7280");
                                    });
                                    j.Item().PaddingTop(1).Text(exp.Company ?? "").FontSize(9f).FontColor("#6366f1");

                                    foreach (var bullet in exp.Bullets ?? [])
                                    {
                                        j.Item().PaddingTop(2).PaddingLeft(8).Row(r =>
                                        {
                                            r.ConstantItem(10).Text("•");
                                            r.RelativeItem().Text(bullet).LineHeight(1.4f);
                                        });
                                    }
                                });
                            }
                        });
                    }

                    // Education
                    if (cv.Education?.Count > 0)
                    {
                        col.Item().Column(edu =>
                        {
                            edu.Item().Text("EDUCATION").FontSize(8.5f).Bold().FontColor("#6366f1");

                            foreach (var ed in cv.Education)
                            {
                                edu.Item().PaddingTop(6).Row(r =>
                                {
                                    r.RelativeItem().Column(c =>
                                    {
                                        c.Item().Text(ed.Qualification ?? "").Bold();
                                        c.Item().PaddingTop(1).Text(ed.Institution ?? "").FontSize(8.5f).FontColor("#6b7280");
                                    });
                                    r.AutoItem().AlignRight().Text(ed.Year ?? "").FontSize(8.5f).FontColor("#6b7280");
                                });
                            }
                        });
                    }

                    // Footer
                    col.Item().AlignCenter().Text($"Tailored CV — {DateTime.UtcNow:dd MMM yyyy}")
                        .FontSize(7.5f).FontColor("#9ca3af");
                });
            });
        }).GeneratePdf();
    }
}