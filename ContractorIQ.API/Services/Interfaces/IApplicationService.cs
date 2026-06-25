using ContractorIQ.API.DTOs.Applications;

namespace ContractorIQ.API.Services.Interfaces;

public interface IApplicationService
{
    Task<List<ApplicationResponse>> GetAllAsync(Guid userId);
    Task<ApplicationResponse?> UpdateAsync(Guid applicationId, Guid userId, UpdateApplicationRequest request);
    Task<ApplicationResponse?> ApplyAsync(Guid jobId, Guid userId);
    Task<bool> DeleteAsync(Guid applicationId, Guid userId);
}