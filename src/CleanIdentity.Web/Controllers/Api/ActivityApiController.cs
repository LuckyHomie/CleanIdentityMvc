using System.Security.Claims;
using CleanIdentity.UseCases.Activities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanIdentity.Web.Controllers.Api;

[ApiController]
[Authorize]
[Route("api/activity")]
public sealed class ActivityApiController : ControllerBase
{
    private readonly IActivityQueryService _activityQueryService;

    public ActivityApiController(IActivityQueryService activityQueryService)
    {
        _activityQueryService = activityQueryService;
    }

    /// <summary>
    /// Zwraca ostatnie czynności aktualnie zalogowanego użytkownika.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(IReadOnlyList<UserActivityDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserActivityDto>>> GetMyActivity(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _activityQueryService.GetForUserAsync(userId, 100, cancellationToken);
        return Ok(result);
    }
}
