namespace ContractorIQ.API.DTOs.Billing;

public record SubscriptionStatusDto(
    string Tier,
    string Status,
    DateTime? CurrentPeriodEnd,
    bool IsActive
);

public record CreateCheckoutSessionRequest(string Tier);

public record CreateCheckoutSessionResponse(string Url);

public record CreatePortalSessionResponse(string Url);

public record PlanDto(
    string Tier,
    string DisplayName,
    decimal MonthlyPriceGbp,
    string[] Features
);