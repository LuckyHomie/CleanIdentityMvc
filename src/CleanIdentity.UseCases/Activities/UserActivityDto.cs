namespace CleanIdentity.UseCases.Activities;

public sealed record UserActivityDto(
    int Id,
    DateTimeOffset CreatedAt,
    string Action,
    string? Details,
    string? IpAddress,
    string? UserAgent);
