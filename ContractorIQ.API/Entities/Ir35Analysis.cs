namespace ContractorIQ.API.Entities;

public class Ir35Analysis
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid JobId { get; set; }
    public int RiskScore { get; set; }        // 0-100, higher = more inside IR35
    public string Verdict { get; set; } = ""; // "low", "medium", "high"
    public int SubstitutionScore { get; set; }
    public int ControlScore { get; set; }
    public int MooScore { get; set; }
    public string RedFlags { get; set; } = "[]";    // JSON array
    public string GreenFlags { get; set; } = "[]";  // JSON array
    public string Summary { get; set; } = "";
    public string SdcRisk { get; set; } = "";       // Supervision, Direction, Control
    public DateTime AnalysedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Job Job { get; set; } = null!;
}