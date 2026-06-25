using ContractorIQ.API.DTOs.Auth;

namespace ContractorIQ.API.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse?> GetCurrentUserAsync(Guid userId);
}