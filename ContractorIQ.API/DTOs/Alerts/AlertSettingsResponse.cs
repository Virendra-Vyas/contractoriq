namespace ContractorIQ.API.DTOs.Alerts;

public class AlertSettingsResponse
{
    public bool AlertsEnabled { get; set; }
    public string? AlertKeywords { get; set; }
    public decimal AlertMinDayRate { get; set; }
    public string? AlertIr35Preference { get; set; }
    public float AlertMinMatchScore { get; set; }
    public DateTime? LastAlertSentAt { get; set; }
}