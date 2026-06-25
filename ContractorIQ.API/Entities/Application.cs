namespace ContractorIQ.API.Entities;

public class Application
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid JobId { get; set; }
    public string Status { get; set; } = "applied";
    // "applied" → "recruiter_screen" → "client_interview" → "offer" → "placed" / "rejected"
    public decimal? DayRateQuoted { get; set; }
    public string? RecruiterName { get; set; }
    public string? RecruiterEmail { get; set; }
    public string? RecruiterPhone { get; set; }
    public string? Notes { get; set; }
    public string? TailoredCvBlobUrl { get; set; }
    public string? CoverEmail { get; set; }
    public DateTime? FollowUpAt { get; set; }
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public Job? Job { get; set; } = null!;
}