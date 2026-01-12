using System.Security.Claims;
using HomeWidgets.Api.Contracts.Auth;
using HomeWidgets.Core.Entities;
using HomeWidgets.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeWidgets.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WidgetsController : ControllerBase
{
    private readonly IWidgetRepository _widgetRepository;
    private readonly IUserRepository _userRepository;
    
    public WidgetsController(
        IWidgetRepository widgetRepository,
        IUserRepository userRepository)
    {
        _widgetRepository = widgetRepository;
        _userRepository = userRepository;
        
    }

    private Guid? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        return claim is not null && Guid.TryParse(claim.Value, out var userId)
            ? userId
            : null;
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Widget>>> GetMyWidgets(
        CancellationToken cancellationToken)
    {
        if (GetCurrentUserId() is not Guid userId)
        {
            return Unauthorized();
        }
        
        var user = await _userRepository.GetByIdWithWidgetsAsync(userId, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }
        var widgets = user.UserWidgets;
        var activeWidgets = widgets.Where(w => w.IsActive)
            .OrderBy(w => w.Order)
            .Select(w => w.Widget)
            .ToList();
        
        return Ok(activeWidgets);
    }

    [HttpPost("me")]
    [Authorize]
    public async Task<ActionResult> UpdateUserDashboard(
        [FromBody] AddWidgetRequest request, CancellationToken cancellationToken)
    {
        if (GetCurrentUserId() is not Guid userId)
        {
            return Unauthorized();
        }

        var user = await _userRepository.GetByIdWithWidgetsAsync(userId, cancellationToken);
        if (user is null)
        {
            return NotFound($"User not found.");
        }

        var widget = await _widgetRepository.GetByIdAsync(request.WidgetId, cancellationToken);
        if (widget is null)
        {
            return NotFound($"Widget with ID {request.WidgetId} not found.");
        }

        try
        {
            user.AddWidget(widget, request.Position);
            await _userRepository.UpdateAsync(user, cancellationToken);
            return CreatedAtAction(nameof(GetMyWidgets), null, widget);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
}
