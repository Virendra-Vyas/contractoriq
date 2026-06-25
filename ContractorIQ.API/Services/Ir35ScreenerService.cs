using System.Text.Json;
using ContractorIQ.API.Data;
using ContractorIQ.API.DTOs.Ir35;
using ContractorIQ.API.Entities;
using ContractorIQ.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContractorIQ.API.Services;

public class Ir35ScreenerService : IIr35ScreenerService
{
    private readonly AppDbContext _db;
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<Ir35ScreenerService> _logger;

    public Ir35ScreenerService(
        AppDbContext db,
        IHttpClientFactory factory,
        IConfiguration config,
        ILogger<Ir35ScreenerService> logger)
    {
        _db = db;
        _http = factory.CreateClient("openai");
        _config = config;
        _logger = logger;
    }

    public async Task<Ir35AnalysisResponse?> AnalyseJobAsync(Guid jobId)
    {
        // Return cached if exists
        var cached = await _db.Ir35Analyses.FirstOrDefaultAsync(a => a.JobId == jobId);
        if (cached != null)
            return MapToResponse(cached, fromCache: true);

        var job = await _db.Jobs.FindAsync(jobId);
        if (job == null) return null;

        var analysis = await CallGptAsync(job);
        if (analysis == null) return null;

        _db.Ir35Analyses.Add(analysis);
        await _db.SaveChangesAsync();

        return MapToResponse(analysis, fromCache: false);
    }

    private async Task<Ir35Analysis?> CallGptAsync(Job job)
    {
        var apiKey = _config["OpenAI:ApiKey"];

        var systemPrompt = """
            You are a UK IR35 employment status expert with deep knowledge of HMRC guidance and case law.
            Analyse the job description and assess IR35 risk across the three key tests.
            Return ONLY valid JSON with no markdown, no backticks, no preamble.

            Return this exact JSON structure:
            {
              "riskScore": 0-100,
              "verdict": "low|medium|high",
              "substitutionScore": 0-100,
              "controlScore": 0-100,
              "mooScore": 0-100,
              "sdcRisk": "low|medium|high",
              "redFlags": ["flag1", "flag2"],
              "greenFlags": ["flag1", "flag2"],
              "summary": "2-3 sentence plain English verdict explaining the IR35 status assessment"
            }

            Scoring guide:
            - riskScore: overall IR35 risk (0=definitely outside, 100=definitely inside)
            - substitutionScore: 0=right to substitute clearly present, 100=no substitution allowed
            - controlScore: 0=contractor has full control, 100=client controls how/when/where work done
            - mooScore: 0=no obligation to offer/accept work, 100=guaranteed work both ways
            - sdcRisk: Supervision Direction Control risk (HMRC's additional test for deemed employees)
            - verdict: low=outside IR35, medium=borderline, high=inside IR35
            - redFlags: specific phrases or clauses that indicate inside IR35
            - greenFlags: specific phrases or clauses that indicate outside IR35
            """;

        var userMessage = $"""
            JOB TITLE: {job.Title}
            COMPANY: {job.Company}
            LOCATION: {job.Location}
            STATED IR35 STATUS: {job.Ir35Status}
            CONTRACT LENGTH: {job.ContractLength ?? "not stated"}
            TECH STACK: {job.TechStack}

            JOB DESCRIPTION:
            {job.Description[..Math.Min(job.Description.Length, 4000)]}
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
            max_tokens = 1000
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        request.Headers.Add("Authorization", $"Bearer {apiKey}");
        request.Content = JsonContent.Create(requestBody);

        var response = await _http.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("GPT-4o IR35 analysis failed: {Status} {Error}", response.StatusCode, err);
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

        using var result = JsonDocument.Parse(content);
        var root = result.RootElement;

        return new Ir35Analysis
        {
            JobId = job.Id,
            RiskScore = root.GetProperty("riskScore").GetInt32(),
            Verdict = root.GetProperty("verdict").GetString() ?? "medium",
            SubstitutionScore = root.GetProperty("substitutionScore").GetInt32(),
            ControlScore = root.GetProperty("controlScore").GetInt32(),
            MooScore = root.GetProperty("mooScore").GetInt32(),
            SdcRisk = root.GetProperty("sdcRisk").GetString() ?? "medium",
            RedFlags = root.GetProperty("redFlags").GetRawText(),
            GreenFlags = root.GetProperty("greenFlags").GetRawText(),
            Summary = root.GetProperty("summary").GetString() ?? "",
            AnalysedAt = DateTime.UtcNow,
        };
    }

    private static Ir35AnalysisResponse MapToResponse(Ir35Analysis a, bool fromCache) => new()
    {
        JobId = a.JobId,
        RiskScore = a.RiskScore,
        Verdict = a.Verdict,
        SubstitutionScore = a.SubstitutionScore,
        ControlScore = a.ControlScore,
        MooScore = a.MooScore,
        SdcRisk = a.SdcRisk,
        RedFlags = JsonSerializer.Deserialize<List<string>>(a.RedFlags) ?? [],
        GreenFlags = JsonSerializer.Deserialize<List<string>>(a.GreenFlags) ?? [],
        Summary = a.Summary,
        FromCache = fromCache,
        AnalysedAt = a.AnalysedAt,
    };
}