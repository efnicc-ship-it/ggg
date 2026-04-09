using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeknikServis.Application.Common.Interfaces;
using TeknikServis.Domain.Enums;
using TeknikServis.Web.Models;

namespace TeknikServis.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _user;

    public HomeController(IApplicationDbContext db, ICurrentUserService user)
    {
        _db = db;
        _user = user;
    }

    public async Task<IActionResult> Index()
    {
        var today = DateTime.UtcNow.Date;
        var weekStart = today.AddDays(-(int)today.DayOfWeek + 1);
        var monthStart = new DateTime(today.Year, today.Month, 1);

        var branchFilter = !_user.CanViewAllBranches && _user.BranchId.HasValue
            ? _user.BranchId
            : (int?)null;

        // Service records query base
        var serviceQ = _db.ServiceRecords.AsNoTracking();
        if (branchFilter.HasValue)
            serviceQ = serviceQ.Where(s => s.BranchId == branchFilter);

        // KPIs
        var openCount     = await serviceQ.CountAsync(s => s.Status != ServiceStatus.Delivered && s.Status != ServiceStatus.Cancelled && s.Status != ServiceStatus.Unrepairable && s.Status != ServiceStatus.ReturnedUnrepaired);
        var todayNew      = await serviceQ.CountAsync(s => s.CreatedAt.Date == today);
        var todayDelivered= await serviceQ.CountAsync(s => s.DeliveredAt.HasValue && s.DeliveredAt.Value.Date == today);
        var awaitingApproval = await serviceQ.CountAsync(s => s.Status == ServiceStatus.AwaitingApproval);
        var awaitingParts = await serviceQ.CountAsync(s => s.Status == ServiceStatus.AwaitingParts);
        var inRepair      = await serviceQ.CountAsync(s => s.Status == ServiceStatus.InRepair);

        // Revenue — this month (delivered)
        var monthRevenue = await _db.ServicePayments
            .AsNoTracking()
            .Where(p => p.CreatedAt >= monthStart)
            .SumAsync(p => (decimal?)p.Amount) ?? 0;

        // Today purchases
        var todayPurchases = await _db.PurchaseRecords
            .AsNoTracking()
            .CountAsync(p => p.CreatedAt.Date == today);

        // AI alerts (unread)
        var aiAlertCount = await _db.AiAlerts
            .AsNoTracking()
            .CountAsync(a => !a.IsResolved);

        // Recent 10 service records
        var recentServices = await serviceQ
            .Include(s => s.Customer)
            .Include(s => s.DeviceModel).ThenInclude(m => m.Brand)
            .OrderByDescending(s => s.CreatedAt)
            .Take(10)
            .Select(s => new DashboardServiceDto
            {
                Id = s.Id,
                RecordNumber = s.RecordNumber,
                CustomerName = s.Customer.FirstName + " " + s.Customer.LastName,
                DeviceBrand = s.DeviceModel.Brand.Name,
                DeviceModel = s.DeviceModel.Name,
                Status = s.Status,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();

        var vm = new DashboardViewModel
        {
            OpenServiceCount = openCount,
            TodayNewCount = todayNew,
            TodayDeliveredCount = todayDelivered,
            AwaitingApprovalCount = awaitingApproval,
            AwaitingPartsCount = awaitingParts,
            InRepairCount = inRepair,
            MonthRevenue = monthRevenue,
            TodayPurchaseCount = todayPurchases,
            UnreadAiAlertCount = aiAlertCount,
            RecentServices = recentServices
        };

        return View(vm);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}

public class DashboardViewModel
{
    public int OpenServiceCount { get; set; }
    public int TodayNewCount { get; set; }
    public int TodayDeliveredCount { get; set; }
    public int AwaitingApprovalCount { get; set; }
    public int AwaitingPartsCount { get; set; }
    public int InRepairCount { get; set; }
    public decimal MonthRevenue { get; set; }
    public int TodayPurchaseCount { get; set; }
    public int UnreadAiAlertCount { get; set; }
    public List<DashboardServiceDto> RecentServices { get; set; } = new();
}

public class DashboardServiceDto
{
    public int Id { get; set; }
    public string RecordNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string DeviceBrand { get; set; } = string.Empty;
    public string DeviceModel { get; set; } = string.Empty;
    public ServiceStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
