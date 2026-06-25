namespace ContractorIQ.API.DTOs.Ir35;

public class Ir35AnalysisResponse
{
    public Guid JobId { get; set; }
    public int RiskScore { get; set; }
    public string Verdict { get; set; } = "";
    public int SubstitutionScore { get; set; }
    public int ControlScore { get; set; }
    public int MooScore { get; set; }
    public List<string> RedFlags { get; set; } = [];
    public List<string> GreenFlags { get; set; } = [];
    public string Summary { get; set; } = "";
    public string SdcRisk { get; set; } = "";
    public bool FromCache { get; set; }
    public DateTime AnalysedAt { get; set; }
}