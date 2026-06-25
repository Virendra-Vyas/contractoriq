using ContractorIQ.API.Data;
using ContractorIQ.API.DTOs.Market;
using ContractorIQ.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContractorIQ.API.Services;

public class MarketRateService : IMarketRateService
{
    private readonly AppDbContext _db;

    public MarketRateService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<MarketRateResponse?> GetMarketRateAsync(
        string? techStack,
        string? location,
        string? ir35Status,
        decimal? jobRate)
    {
        var query = _db.Jobs
            .Where(j => j.IsActive && j.DayRateMax.HasValue && j.DayRateMax > 0);

        // Filter by tech stack — match any of the provided techs
        if (!string.IsNullOrEmpty(techStack))
        {
            var techs = techStack.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim().ToLower())
                .Where(t => t.Length > 1)
                .ToList();

            if (techs.Any())
            {
                query = query.Where(j =>
                    techs.Any(t => j.TechStack.ToLower().Contains(t)));
            }
        }

        // Filter by location (loose match)
        if (!string.IsNullOrEmpty(location))
        {
            var loc = location.ToLower();
            // Remote jobs always included
            query = query.Where(j =>
                j.Location.ToLower().Contains(loc) ||
                j.IsRemote ||
                j.Location.ToLower().Contains("remote") ||
                j.Location.ToLower().Contains("uk"));
        }

        // Filter by IR35 if specified
        if (!string.IsNullOrEmpty(ir35Status) && ir35Status != "unknown")
        {
            query = query.Where(j => j.Ir35Status == ir35Status);
        }

        var rates = await query
            .Select(j => j.DayRateMax!.Value)
            .ToListAsync();

        // Fall back to broader search if too few results
        if (rates.Count < 3)
        {
            rates = await _db.Jobs
                .Where(j => j.IsActive && j.DayRateMax.HasValue && j.DayRateMax > 0)
                .Select(j => j.DayRateMax!.Value)
                .ToListAsync();
        }

        if (!rates.Any()) return null;

        rates.Sort();

        var median = Percentile(rates, 50);
        var p25 = Percentile(rates, 25);
        var p75 = Percentile(rates, 75);
        var mean = rates.Average();
        var min = rates.Min();
        var max = rates.Max();

        int? percentile = null;
        string? percentileLabel = null;

        if (jobRate.HasValue && jobRate > 0)
        {
            var belowCount = rates.Count(r => r < jobRate.Value);
            percentile = (int)Math.Round((double)belowCount / rates.Count * 100);

            percentileLabel = percentile switch
            {
                >= 90 => "top 10% — excellent rate",
                >= 75 => "top 25% — above average",
                >= 50 => "above median",
                >= 25 => "below median",
                _ => "bottom 25% — below average"
            };
        }

        return new MarketRateResponse
        {
            TechStack = techStack ?? "",
            Location = location ?? "",
            Ir35Status = ir35Status ?? "",
            Median = Math.Round(median, 0),
            Mean = Math.Round((decimal)mean, 0),
            P25 = Math.Round(p25, 0),
            P75 = Math.Round(p75, 0),
            Min = min,
            Max = max,
            SampleSize = rates.Count,
            JobPercentile = percentile,
            PercentileLabel = percentileLabel,
        };
    }

    private static decimal Percentile(List<decimal> sortedData, int percentile)
    {
        if (!sortedData.Any()) return 0;
        var index = (percentile / 100.0) * (sortedData.Count - 1);
        var lower = (int)Math.Floor(index);
        var upper = (int)Math.Ceiling(index);
        if (lower == upper) return sortedData[lower];
        var fraction = (decimal)(index - lower);
        return sortedData[lower] + fraction * (sortedData[upper] - sortedData[lower]);
    }
}