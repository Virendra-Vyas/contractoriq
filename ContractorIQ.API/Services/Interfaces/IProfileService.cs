using ContractorIQ.API.DTOs.Profile;

namespace ContractorIQ.API.Services.Interfaces;

public interface IProfileService
{
    Task<ProfileResponse?> GetProfileAsync(Guid userId);
    Task<ProfileResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
    Task<string> UploadCvAsync(Guid userId, IFormFile file);
    Task<(byte[] bytes, string fileName, string contentType)> DownloadCvAsync(Guid userId);
}