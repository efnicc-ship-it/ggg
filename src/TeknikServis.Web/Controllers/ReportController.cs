using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeknikServis.Application.Common.Interfaces;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Web.Controllers;

[Authorize]
public class ReportController : Controller
{
    private readonly IApplicationDbContext _db;

    public ReportController(IApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Raporlar";

        var today = DateTime.UtcNow.Date;
        var monthStart = new DateTime(today.Year, today.Month, 1);

        var summary = new ReportSummaryViewModel
        {
            TotalServicesThisMonth = await _db.ServiceRecords.CountAsync(s => s.CreatedAt >= monthStart),
            DeliveredThisMonth = await _db.ServiceRecords.CountAsync(s => s.DeliveredAt.HasValue && s.DeliveredAt >= monthStart),
            RevenueThisMonth = await _db.ServicePayments.Where(p => p.CreatedAt >= monthStart).SumAsync(p => (decimal?)p.Amount) ?? 0,
            PurchasesThisMonth = await _db.PurchaseRecords.CountAsync(p => p.CreatedAt >= monthStart),
            SalesThisMonth = await _db.SaleRecords.CountAsync(s => s.CreatedAt >= monthStart),
        };

        return View(summary);
    }
}

public class ReportSummaryViewModel
{
    public int TotalServicesThisMonth { get; set; }
    public int DeliveredThisMonth { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public int PurchasesThisMonth { get; set; }
    public int SalesThisMonth { get; set; }
}
