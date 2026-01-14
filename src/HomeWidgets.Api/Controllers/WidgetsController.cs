using System.Security.Claims;
using HomeWidgets.Api.Contracts.Auth;
using HomeWidgets.Core.Entities;
using HomeWidgets.Core.Interfaces;
using HomeWidgets.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeWidgets.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WidgetsController : ControllerBase
{
    private readonly IWidgetRepository _widgetRepository;
    private readonly IUserRepository _userRepository;
    private readonly AppDbContext _context;
    
    public WidgetsController(
        IWidgetRepository widgetRepository,
        IUserRepository userRepository,
        AppDbContext context)
    {
        _widgetRepository = widgetRepository;
        _userRepository = userRepository;
        _context = context;

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
            var addedUserWidget = user.AddWidget(widget, request.Position);
            _context.Entry(addedUserWidget).State = EntityState.Added;
            await _context.SaveChangesAsync(cancellationToken);
            return CreatedAtAction(nameof(GetMyWidgets), null, widget);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("me/{widgetId}/activate")]
    [Authorize]
    public async Task<ActionResult> ActivateWidget(
        [FromRoute] Guid widgetId, CancellationToken cancellationToken)
    {
        if (GetCurrentUserId() is not Guid userId)
            return Unauthorized();
        
        var user = await _userRepository.GetByIdWithWidgetsAsync(userId, cancellationToken);
        if (user is null)
            return NotFound("User not found.");

        var widget = await _widgetRepository.GetByIdAsync(widgetId, cancellationToken);
        if (widget is null)
            return NotFound($"Widget with ID {widgetId} not found.");

        try
        {
            user.ActivateWidget(widget);
            await _context.SaveChangesAsync(cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("me/{widgetId}/deactivate")]
    [Authorize]
    public async Task<ActionResult> DeactivateWidget(
        [FromRoute] Guid widgetId, CancellationToken cancellationToken)
    {
        if (GetCurrentUserId() is not Guid userId)
            return Unauthorized();
        
        var user = await _userRepository.GetByIdWithWidgetsAsync(userId, cancellationToken);
        if (user is null)
            return NotFound("User not found.");

        var widget = await _widgetRepository.GetByIdAsync(widgetId, cancellationToken);
        if (widget is null)
            return NotFound($"Widget with ID {widgetId} not found.");

        try
        {
            user.DeactivateWidget(widget);
            await _context.SaveChangesAsync(cancellationToken);
            return NoContent();
        } catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("me/{widgetId}")]
    [Authorize]
    public async Task<ActionResult> RemoveWidgetFromDashboard(
        [FromRoute] Guid widgetId, CancellationToken cancellationToken)
    {
        if (GetCurrentUserId() is not Guid userId)
            return Unauthorized();
        
        var user = await _userRepository.GetByIdWithWidgetsAsync(userId, cancellationToken);
        if (user is null)
            return NotFound("User not found.");
        
        var widget = await _widgetRepository.GetByIdAsync(widgetId, cancellationToken);
        if (widget is null)
            return NotFound($"Widget with ID {widgetId} not found.");

        try
        {
            user.RemoveWidget(widget.Id);
            await _context.SaveChangesAsync(cancellationToken);
            return NoContent();
        } catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("me/reorder")]
    [Authorize]
    public async Task<ActionResult> ReorderWidgets(
        [FromBody] ReorderWidgetsRequest request, CancellationToken cancellationToken)
    {
        if (GetCurrentUserId() is not Guid userId)
            return Unauthorized();
        
        var user = await _userRepository.GetByIdWithWidgetsAsync(userId, cancellationToken);
        if (user is null)
            return NotFound("User not found.");

        try
        {
            user.ReorderWidgets(request.WidgetIdsInOrder);
            await _context.SaveChangesAsync(cancellationToken);
            return NoContent();
        } catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<Widget>>> GetAvailableWidgets(
        CancellationToken cancellationToken)
    {
        if (GetCurrentUserId() is null)
            return Unauthorized();
        
        var widgets = await _widgetRepository.GetAllAsync(cancellationToken);
        var activeWidgets = widgets
            .Where(w => w.IsActive)
            .OrderBy(w => w.DisplayOrder)
            .ToList();
        return Ok(activeWidgets);
    }
    
}
