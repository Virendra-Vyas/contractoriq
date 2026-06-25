namespace ContractorIQ.API.DTOs.Alerts;

public class UpdateAlertSettingsRequest
{
    public bool AlertsEnabled { get; set; }
    public string? AlertKeywords { get; set; }
    public decimal AlertMinDayRate { get; set; }
    public string? AlertIr35Preference { get; set; }
    public float AlertMinMatchScore { get; set; } = 60;
}