using Pgvector;

namespace ContractorIQ.API.Entities;

public class ContractorProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string? JobTitle { get; set; }
    public string? Summary { get; set; }
    public string? Skills { get; set; }
    public string? PreferredLocation { get; set; }
    public bool RemoteOnly { get; set; } = false;
    public decimal DesiredDayRateMin { get; set; }
    public decimal DesiredDayRateMax { get; set; }
    public string? Ir35Preference { get; set; }
    public string? NoticePeriod { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? MasterCvBlobUrl { get; set; }
    public string? MasterCvFileName { get; set; }
    public Vector? ProfileEmbedding { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public bool AlertsEnabled { get; set; } = false;
    public string? AlertKeywords { get; set; }
    public decimal AlertMinDayRate { get; set; } = 0;
    public string? AlertIr35Preference { get; set; }
    public float AlertMinMatchScore { get; set; } = 60;
    public DateTime? LastAlertSentAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}