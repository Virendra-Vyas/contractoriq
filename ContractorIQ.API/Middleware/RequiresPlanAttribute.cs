namespace ContractorIQ.API.Middleware;

[AttributeUsage(AttributeTargets.Method)]
public class RequiresPlanAttribute : Attribute
{
    public string[] AllowedTiers { get; }

    public RequiresPlanAttribute(params string[] tiers)
    {
        AllowedTiers = tiers;
    }
}