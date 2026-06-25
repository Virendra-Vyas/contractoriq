using ContractorIQ.API.Configuration;
using ContractorIQ.API.Data;
using ContractorIQ.API.DTOs.Billing;
using ContractorIQ.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using EntitySubscription = ContractorIQ.API.Entities.Subscription;

namespace ContractorIQ.API.Services;

public class BillingService : IBillingService
{
    private readonly AppDbContext _db;
    private readonly StripeOptions _stripe;
    private readonly ILogger<BillingService> _logger;

    public BillingService(
        AppDbContext db,
        IOptions<StripeOptions> stripeOptions,
        ILogger<BillingService> logger)
    {
        _db = db;
        _stripe = stripeOptions.Value;
        _logger = logger;
        StripeConfiguration.ApiKey = _stripe.SecretKey;
    }

    // ── Public interface ────────────────────────────────────────────────────

    public async Task<SubscriptionStatusDto> GetStatusAsync(Guid userId)
    {
        EntitySubscription? sub = await _db.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (sub == null)
            return new SubscriptionStatusDto("free", "active", null, false);

        var isActive = sub.Status == "active";
        return new SubscriptionStatusDto(
            sub.Tier,
            sub.Status,
            sub.CurrentPeriodEnd,
            isActive
        );
    }

    public async Task<string> CreateCheckoutSessionAsync(Guid userId, string tier, string userEmail)
    {
        tier = tier.ToLowerInvariant();

        if (!_stripe.Plans.TryGetValue(tier, out var priceId))
            throw new InvalidOperationException($"Unknown tier: {tier}");

        EntitySubscription? sub = await _db.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId);

        string? customerId = sub?.StripeCustomerId;

        if (string.IsNullOrEmpty(customerId))
        {
            var customerService = new CustomerService();
            var customer = await customerService.CreateAsync(new CustomerCreateOptions
            {
                Email = userEmail,
                Metadata = new Dictionary<string, string>
                {
                    ["userId"] = userId.ToString()
                }
            });
            customerId = customer.Id;

            if (sub == null)
            {
                sub = new EntitySubscription
                {
                    UserId           = userId,
                    StripeCustomerId = customerId,
                    Tier             = "free",
                    Status           = "active"
                };
                _db.Subscriptions.Add(sub);
            }
            else
            {
                sub.StripeCustomerId = customerId;
                sub.UpdatedAt        = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
        }

        var options = new SessionCreateOptions
        {
            Customer           = customerId,
            PaymentMethodTypes = new List<string> { "card" },
            LineItems          = new List<SessionLineItemOptions>
            {
                new() { Price = priceId, Quantity = 1 }
            },
            Mode       = "subscription",
            SuccessUrl = "http://localhost:5173/billing/success?session_id={CHECKOUT_SESSION_ID}",
            CancelUrl  = "http://localhost:5173/pricing",
            Metadata   = new Dictionary<string, string>
            {
                ["userId"] = userId.ToString(),
                ["tier"]   = tier
            }
        };

        var sessionService = new SessionService();
        var session        = await sessionService.CreateAsync(options);
        return session.Url;
    }

    public async Task<string> CreatePortalSessionAsync(Guid userId)
    {
        EntitySubscription? sub = await _db.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (sub == null || string.IsNullOrEmpty(sub.StripeCustomerId))
            throw new InvalidOperationException("No billing record found for this user.");

        var options = new Stripe.BillingPortal.SessionCreateOptions
        {
            Customer  = sub.StripeCustomerId,
            ReturnUrl = "http://localhost:5173/pricing"
        };

        var service = new Stripe.BillingPortal.SessionService();
        var session = await service.CreateAsync(options);
        return session.Url;
    }

    public async Task HandleWebhookAsync(string json, string signature)
    {
        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(json, signature, _stripe.WebhookSecret);
        }
        catch (StripeException ex)
        {
            _logger.LogWarning("Webhook signature validation failed: {Message}", ex.Message);
            throw;
        }

        _logger.LogInformation("Stripe webhook: {Type}", stripeEvent.Type);

        switch (stripeEvent.Type)
        {
            case "checkout.session.completed":
                await HandleCheckoutCompletedAsync((Session)stripeEvent.Data.Object);
                break;

            case "customer.subscription.created":
            case "customer.subscription.updated":
                await HandleSubscriptionUpdatedAsync((Stripe.Subscription)stripeEvent.Data.Object);
                break;

            case "customer.subscription.deleted":
                await HandleSubscriptionDeletedAsync((Stripe.Subscription)stripeEvent.Data.Object);
                break;

            case "invoice.payment_failed":
                await HandlePaymentFailedAsync((Invoice)stripeEvent.Data.Object);
                break;

            default:
                _logger.LogDebug("Unhandled Stripe event: {Type}", stripeEvent.Type);
                break;
        }
    }

    // ── Private webhook handlers ────────────────────────────────────────────

    private async Task HandleCheckoutCompletedAsync(Session session)
    {
        var meta = session.Metadata;
        if (meta == null || !Guid.TryParse(meta.GetValueOrDefault("userId"), out var userId))
        {
            _logger.LogWarning("checkout.session.completed: missing or invalid userId metadata");
            return;
        }

        var tier = meta.GetValueOrDefault("tier") ?? "individual";

        EntitySubscription? sub = await _db.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (sub == null)
        {
            sub = new EntitySubscription { UserId = userId };
            _db.Subscriptions.Add(sub);
        }

        sub.StripeCustomerId     = session.CustomerId;
        sub.StripeSubscriptionId = session.SubscriptionId;
        sub.Tier                 = tier;
        sub.Status               = "active";
        sub.UpdatedAt            = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Subscription activated — userId {UserId}, tier {Tier}", userId, tier);
    }

    private async Task HandleSubscriptionUpdatedAsync(Stripe.Subscription stripeSub)
    {
        EntitySubscription? sub = await _db.Subscriptions
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSub.Id);

        if (sub == null)
        {
            _logger.LogWarning("subscription.updated: unknown StripeSubscriptionId {Id}", stripeSub.Id);
            return;
        }

        sub.Status    = stripeSub.Status == "active"   ? "active"
            : stripeSub.Status == "past_due"  ? "past_due"
            : "cancelled";
        sub.UpdatedAt = DateTime.UtcNow;

        // CurrentPeriodEnd lives on the subscription item in newer Stripe SDK versions
        var item = stripeSub.Items?.Data?.FirstOrDefault();
        if (item != null)
        {
            if (item.CurrentPeriodEnd != default)
                sub.CurrentPeriodEnd = item.CurrentPeriodEnd.ToUniversalTime();

            var matched = _stripe.Plans.FirstOrDefault(p => p.Value == item.Price.Id);
            if (!string.IsNullOrEmpty(matched.Key))
                sub.Tier = matched.Key;
        }

        await _db.SaveChangesAsync();
    }

    private async Task HandleSubscriptionDeletedAsync(Stripe.Subscription stripeSub)
    {
        EntitySubscription? sub = await _db.Subscriptions
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSub.Id);

        if (sub == null) return;

        sub.Status    = "cancelled";
        sub.Tier      = "free";
        sub.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Subscription cancelled — userId {UserId}", sub.UserId);
    }

    private async Task HandlePaymentFailedAsync(Invoice invoice)
    {
        EntitySubscription? sub = await _db.Subscriptions
            .FirstOrDefaultAsync(s => s.StripeCustomerId == invoice.CustomerId);

        if (sub == null) return;

        sub.Status    = "past_due";
        sub.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogWarning("Payment failed — userId {UserId}", sub.UserId);
    }
}