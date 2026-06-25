namespace ContractorIQ.API.DTOs.Profile;

public class ProfileResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? JobTitle { get; set; }
    public string? Summary { get; set; }
    public string? Skills { get; set; }
    public string? PreferredLocation { get; set; }
    public bool RemoteOnly { get; set; }
    public decimal DesiredDayRateMin { get; set; }
    public decimal DesiredDayRateMax { get; set; }
    public string? Ir35Preference { get; set; }
    public string? NoticePeriod { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? MasterCvFileName { get; set; }
    public bool HasCv { get; set; }
    public int ProfileCompletionScore { get; set; }
    public DateTime UpdatedAt { get; set; }
}