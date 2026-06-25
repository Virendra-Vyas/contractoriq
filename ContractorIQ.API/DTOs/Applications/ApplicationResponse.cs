namespace ContractorIQ.API.DTOs.Applications;

public class ApplicationResponse
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public string Status { get; set; } = "";
    public decimal? DayRateQuoted { get; set; }
    public string? RecruiterName { get; set; }
    public string? RecruiterEmail { get; set; }
    public string? RecruiterPhone { get; set; }
    public string? Notes { get; set; }
    public DateTime? FollowUpAt { get; set; }
    public DateTime AppliedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string JobTitle { get; set; } = "";
    public string Company { get; set; } = "";
    public string Location { get; set; } = "";
    public string Ir35Status { get; set; } = "";
    public decimal? DayRateMin { get; set; }
    public decimal? DayRateMax { get; set; }
    public string TechStack { get; set; } = "";
    public string SourceUrl { get; set; } = "";
    public float? MatchScore { get; set; }
}