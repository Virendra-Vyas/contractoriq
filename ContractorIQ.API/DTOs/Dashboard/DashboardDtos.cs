namespace ContractorIQ.API.DTOs.Dashboard;

public record DashboardDto(
    ApplicationStatsDto ApplicationStats,
    JobStatsDto JobStats,
    UsageStatsDto UsageStats,
    SubscriptionSummaryDto Subscription,
    IEnumerable<RecentApplicationDto> RecentApplications
);

public record ApplicationStatsDto(
    int Total,
    int Saved,
    int Applied,
    int Interviewing,
    int Offers,
    int Placed,
    int Rejected
);

public record JobStatsDto(
    int TotalScored,
    int SavedJobs,
    double AverageMatchScore,
    int HighMatchCount
);

public record UsageStatsDto(
    int CvsTailored,
    int Ir35Screens
);

public record SubscriptionSummaryDto(
    string Tier,
    string Status,
    bool IsActive
);

public record RecentApplicationDto(
    Guid Id,
    string JobTitle,
    string Company,
    string Status,
    int? MatchScore,
    DateTime CreatedAt
);