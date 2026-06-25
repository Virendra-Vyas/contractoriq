using ContractorIQ.API.DTOs.Billing;

namespace ContractorIQ.API.Services.Interfaces;

public interface IBillingService
{
    Task<SubscriptionStatusDto> GetStatusAsync(Guid userId);
    Task<string> CreateCheckoutSessionAsync(Guid userId, string tier, string userEmail);
    Task<string> CreatePortalSessionAsync(Guid userId);
    Task HandleWebhookAsync(string json, string signature);
}