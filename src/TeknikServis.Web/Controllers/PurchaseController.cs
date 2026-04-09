using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeknikServis.Application.Common.Interfaces;
using TeknikServis.Application.Features.Purchase.Commands.CreatePurchase;
using TeknikServis.Application.Features.Purchase.Queries.GetPurchaseRecords;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Web.Controllers;

[Authorize]
public class PurchaseController : Controller
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _db;

    public PurchaseController(IMediator mediator, IApplicationDbContext db)
    {
        _mediator = mediator;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var record = await _db.PurchaseRecords
            .Include(p => p.Customer)
            .Include(p => p.DeviceModel).ThenInclude(m => m.Brand)
            .Include(p => p.DeviceModelVariant)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (record == null) return NotFound();

        ViewData["Title"] = $"Alış {record.RecordNumber}";
        return View(record);
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? q, PurchaseType? type, int? branchId,
        string? from, string? to, int page = 1)
    {
        var fromDate = string.IsNullOrEmpty(from) ? (DateTime?)null : DateTime.Parse(from);
        var toDate   = string.IsNullOrEmpty(to)   ? (DateTime?)null : DateTime.Parse(to);

        var result = await _mediator.Send(new GetPurchaseRecordsQuery(
            Page: page, PageSize: 25,
            Search: q, PurchaseType: type,
            BranchId: branchId, From: fromDate, To: toDate));

        ViewBag.Q = q;
        ViewBag.Type = type;
        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePurchaseCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);

            TempData["Success"] = $"Alış kaydı oluşturuldu: {result.RecordNumber}";

            if (result.AutoServiceRecordId.HasValue)
                TempData["Info"] = $"Arızalı cihaz için otomatik servis kaydı açıldı.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            await PopulateDropdowns();
            return View(command);
        }
    }

    private async Task PopulateDropdowns()
    {
        // ViewBag populated with device types/brands/models/technicians
        // Actual data comes via API endpoints for cascading
        ViewBag.DeviceTypes = new List<dynamic>();
        ViewBag.Technicians = new List<dynamic>();
        ViewBag.Branches    = new List<dynamic>();
    }
}
