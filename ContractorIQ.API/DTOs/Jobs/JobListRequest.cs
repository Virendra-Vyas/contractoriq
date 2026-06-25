namespace ContractorIQ.API.DTOs.Jobs;

public class JobListRequest
{
    public string? Keywords { get; set; }
    public string? Location { get; set; }
    public string? Ir35Status { get; set; }    // "inside", "outside", "unknown"
    public string? Source { get; set; }         // "reed", "adzuna"
    public decimal? DayRateMin { get; set; }
    public decimal? DayRateMax { get; set; }
    public bool? RemoteOnly { get; set; }
    public string? TechStack { get; set; }      // comma-separated
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "posted_at"; // "posted_at", "day_rate"
}