namespace ContractorIQ.API.DTOs.Jobs;

public class JobResponse
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = "";
    public string Source { get; set; } = "";
    public string Title { get; set; } = "";
    public string Company { get; set; } = "";
    public string Location { get; set; } = "";
    public bool IsRemote { get; set; }
    public bool IsHybrid { get; set; }
    public decimal? DayRateMin { get; set; }
    public decimal? DayRateMax { get; set; }
    public string Ir35Status { get; set; } = "";
    public string? ContractLength { get; set; }
    public string Description { get; set; } = "";
    public string TechStack { get; set; } = "";
    public string SourceUrl { get; set; } = "";
    public DateTime PostedAt { get; set; }
    public DateTime ScrapedAt { get; set; }
    public bool IsSaved { get; set; }
    public float? MatchScore { get; set; }
}