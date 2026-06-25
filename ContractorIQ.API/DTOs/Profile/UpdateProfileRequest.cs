namespace ContractorIQ.API.DTOs.Profile;

public class UpdateProfileRequest
{
    public string? JobTitle { get; set; }
    public string? Summary { get; set; }
    public string? Skills { get; set; }           // comma-separated: "C#,.NET,React"
    public string? PreferredLocation { get; set; }
    public bool RemoteOnly { get; set; } = false;
    public decimal DesiredDayRateMin { get; set; }
    public decimal DesiredDayRateMax { get; set; }
    public string? Ir35Preference { get; set; }   // "outside", "inside", "both"
    public string? NoticePeriod { get; set; }
    public string? LinkedInUrl { get; set; }
}