using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopfinity.API.Responses;
using Shopfinity.Domain.Constants;
using Shopfinity.Infrastructure.Data;
using System.Threading;

namespace Shopfinity.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = AppRoles.Admin)]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<DashboardMetricsDto>>> GetDashboardMetrics(CancellationToken ct)
    {
        var totalUsers = await _context.Users.CountAsync(ct);
        var totalOrders = await _context.Orders.CountAsync(ct);
        var totalRevenue = await _context.Orders.SumAsync(o => o.TotalAmount, ct);
        var today = DateTime.UtcNow.Date;
        var dailySales = await _context.Orders
            .Where(o => o.CreatedAt >= today)
            .SumAsync(o => o.TotalAmount, ct);

        var dto = new DashboardMetricsDto(totalUsers, totalOrders, totalRevenue, dailySales);
        return Ok(ApiResponse<DashboardMetricsDto>.SuccessResponse(dto));
    }
}
