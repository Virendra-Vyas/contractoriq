using ContractorIQ.API.DTOs.Dashboard;

namespace ContractorIQ.API.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync(Guid userId);
}