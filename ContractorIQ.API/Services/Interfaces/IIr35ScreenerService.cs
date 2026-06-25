using ContractorIQ.API.DTOs.Ir35;

namespace ContractorIQ.API.Services.Interfaces;

public interface IIr35ScreenerService
{
    Task<Ir35AnalysisResponse?> AnalyseJobAsync(Guid jobId);
}