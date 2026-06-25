using ContractorIQ.API.DTOs.Billing;
using ContractorIQ.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ContractorIQ.API.Controllers;

[ApiController]
[Route("api/billing")]
public class BillingController : ControllerBase
{
    private readonly IBillingService _billing;

    public BillingController(IBillingService billing)
    {
        _billing = billing;
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private string GetUserEmail() =>
        User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

    [Authorize]
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var status = await _billing.GetStatusAsync(GetUserId());
        return Ok(status);
    }

    [Authorize]
    [HttpPost("checkout")]
    public async Task<IActionResult> CreateCheckout([FromBody] CreateCheckoutSessionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Tier))
            return BadRequest("Tier is required.");

        var url = await _billing.CreateCheckoutSessionAsync(
            GetUserId(), request.Tier, GetUserEmail());

        return Ok(new CreateCheckoutSessionResponse(url));
    }

    [Authorize]
    [HttpPost("portal")]
    public async Task<IActionResult> CreatePortal()
    {
        var url = await _billing.CreatePortalSessionAsync(GetUserId());
        return Ok(new CreatePortalSessionResponse(url));
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook()
    {
        var json      = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault() ?? string.Empty;

        try
        {
            await _billing.HandleWebhookAsync(json, signature);
            return Ok();
        }
        catch (Stripe.StripeException)
        {
            return BadRequest("Webhook signature invalid.");
        }
    }

    [HttpGet("plans")]
    [AllowAnonymous]
    public IActionResult GetPlans()
    {
        var plans = new[]
        {
            new PlanDto("free", "Free", 0m, new[]
            {
                "5 job matches/day",
                "Basic CV upload",
                "Application tracker"
            }),
            new PlanDto("individual", "Individual", 29m, new[]
            {
                "Unlimited job matches",
                "AI CV tailoring (10/mo)",
                "IR35 screening (10/mo)",
                "Day rate intelligence",
                "Job alerts"
            }),
            new PlanDto("pro", "Pro", 79m, new[]
            {
                "Everything in Individual",
                "Unlimited CV tailoring",
                "Unlimited IR35 screening",
                "Priority support",
                "Early access features"
            })
        };

        return Ok(plans);
    }
}