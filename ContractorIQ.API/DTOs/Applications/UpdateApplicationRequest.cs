namespace ContractorIQ.API.DTOs.Applications;

public class UpdateApplicationRequest
{
    public string? Status { get; set; }
    public string? Notes { get; set; }
    public decimal? DayRateQuoted { get; set; }
    public string? RecruiterName { get; set; }
    public string? RecruiterEmail { get; set; }
    public string? RecruiterPhone { get; set; }
    public DateTime? FollowUpAt { get; set; }
}