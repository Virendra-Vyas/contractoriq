namespace ContractorIQ.API.Configuration;

public class StripeOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public string PublishableKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;

    /// <summary>
    /// Keys are tier names: "individual", "pro"
    /// Values are Stripe Price IDs: "price_xxx"
    /// </summary>
    public Dictionary<string, string> Plans { get; set; } = new();
}