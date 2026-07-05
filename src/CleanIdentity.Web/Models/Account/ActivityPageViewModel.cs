using CleanIdentity.UseCases.Activities;

namespace CleanIdentity.Web.Models.Account;

public sealed class ActivityPageViewModel
{
    public bool ShowAfterLogin { get; set; }
    public IReadOnlyList<UserActivityDto> Activities { get; set; } = Array.Empty<UserActivityDto>();
}
