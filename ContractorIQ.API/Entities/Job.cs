using Pgvector;

namespace ContractorIQ.API.Entities;

public class Job
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ExternalId { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string? Location { get; set; }
    public bool IsRemote { get; set; } = false;
    public bool IsHybrid { get; set; } = false;
    public decimal? DayRateMin { get; set; }
    public decimal? DayRateMax { get; set; }
    public string? Ir35Status { get; set; }
    public string? ContractLength { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? TechStack { get; set; }
    public string? RecruiterName { get; set; }
    public string? RecruiterEmail { get; set; }
    public string? RecruiterPhone { get; set; }
    public string SourceUrl { get; set; } = string.Empty;
    public string? Ir35RiskLevel { get; set; }
    public string? Ir35RiskFlags { get; set; }
    public Vector? DescriptionEmbedding { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime PostedAt { get; set; }
    public DateTime ScrapedAt { get; set; } = DateTime.UtcNow;
    public float? MatchScore { get; set; }
    public Ir35Analysis? Ir35Analysis { get; set; }

    // Navigation
    public ICollection<Application> Applications { get; set; } = new List<Application>();
}