namespace ContractorIQ.API.Services.Interfaces;

public interface ICvTailoringService
{
    Task<byte[]?> TailorCvAsync(Guid userId, Guid jobId);
}