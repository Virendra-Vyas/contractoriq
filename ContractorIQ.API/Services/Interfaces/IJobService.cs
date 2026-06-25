using ContractorIQ.API.DTOs.Jobs;

namespace ContractorIQ.API.Services.Interfaces;

public interface IJobService
{
    Task<PagedResult<JobResponse>> GetJobsAsync(JobListRequest request, Guid userId);
    Task<JobResponse?> GetJobByIdAsync(Guid id, Guid userId);
    Task<bool> SaveJobAsync(Guid jobId, Guid userId);
    Task<bool> UnsaveJobAsync(Guid jobId, Guid userId);
    Task<PagedResult<JobResponse>> GetSavedJobsAsync(Guid userId, int page, int pageSize);
}