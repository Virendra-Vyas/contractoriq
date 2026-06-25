namespace ContractorIQ.API.DTOs.Market;

public class MarketRateResponse
{
    public string TechStack { get; set; } = "";
    public string Location { get; set; } = "";
    public string Ir35Status { get; set; } = "";
    public decimal Median { get; set; }
    public decimal Mean { get; set; }
    public decimal P25 { get; set; }
    public decimal P75 { get; set; }
    public decimal Min { get; set; }
    public decimal Max { get; set; }
    public int SampleSize { get; set; }
    public int? JobPercentile { get; set; }   // where this job's rate sits
    public string? PercentileLabel { get; set; } // "top 20%", "below average" etc
}