namespace ContractorIQ.API.Services.Interfaces;

public interface IMatchingService
{
    Task ScoreJobsForUserAsync(Guid userId);
    Task ScoreJobAsync(Guid jobId, Guid userId);
}