using ContractorIQ.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ContractorIQ.API.Middleware;

public class PlanGatingFilter : IAsyncActionFilter
{
    private readonly AppDbContext _db;

    public PlanGatingFilter(AppDbContext db)
    {
        _db = db;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var attr = context.ActionDescriptor.EndpointMetadata
            .OfType<RequiresPlanAttribute>()
            .FirstOrDefault();

        if (attr == null)
        {
            await next();
            return;
        }

        var claim = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(claim, out var userId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var sub = await _db.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId);

        var isActive      = sub?.Status == "active";
        var effectiveTier = isActive ? (sub?.Tier ?? "free") : "free";

        if (!attr.AllowedTiers.Contains(effectiveTier))
        {
            context.Result = new ObjectResult(new
                {
                    error        = "upgrade_required",
                    message      = $"This feature requires one of: {string.Join(", ", attr.AllowedTiers)}",
                    requiredTiers = attr.AllowedTiers
                })
                { StatusCode = 402 };
            return;
        }

        await next();
    }
}