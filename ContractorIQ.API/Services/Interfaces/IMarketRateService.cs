using ContractorIQ.API.DTOs.Market;

namespace ContractorIQ.API.Services.Interfaces;

public interface IMarketRateService
{
    Task<MarketRateResponse?> GetMarketRateAsync(
        string? techStack,
        string? location,
        string? ir35Status,
        decimal? jobRate);
}