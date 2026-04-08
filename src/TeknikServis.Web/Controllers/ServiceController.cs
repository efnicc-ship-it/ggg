using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeknikServis.Application.Common.Interfaces;
using TeknikServis.Application.Features.Service.Commands.AddServicePart;
using TeknikServis.Application.Features.Service.Commands.CreateServiceRecord;
using TeknikServis.Application.Features.Service.Commands.UpdateServiceStatus;
using TeknikServis.Application.Features.Service.Queries.GetServiceRecordDetail;
using TeknikServis.Application.Features.Service.Queries.GetServiceRecords;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Web.Controllers;

[Authorize]
public class ServiceController : Controller
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _db;
    private readonly ILabelPrintService _labelPrint;

    public ServiceController(IMediator mediator, IApplicationDbContext db, ILabelPrintService labelPrint)
    {
        _mediator = mediator;
        _db = db;
        _labelPrint = labelPrint;
    }

    // GET: /Service
    [HttpGet]
    public async Task<IActionResult> Index(
        string? q, ServiceStatus? status, int? branchId, int? technicianId,
        DateTime? from, DateTime? to, int page = 1)
    {
        ViewData["Title"] = "Servis Kayıtları";
        ViewData["PageTitle"] = "Servis Kayıtları";

        var result = await _mediator.Send(new GetServiceRecordsQuery(
            Page: page, PageSize: 25, Search: q, Status: status,
            BranchId: branchId, TechnicianId: technicianId, From: from, To: to));

        // Filter options for dropdowns
        ViewBag.Technicians = await _db.ServiceRecords
            .Where(s => s.AssignedTechnicianId != null)
            .Select(s => s.AssignedTechnicianId!.Value)
            .Distinct().ToListAsync();

        ViewBag.Q = q;
        ViewBag.Status = status;
        ViewBag.From = from?.ToString("yyyy-MM-dd");
        ViewBag.To = to?.ToString("yyyy-MM-dd");

        return View(result);
    }

    // GET: /Service/5
    [HttpGet("{id:int}", Name = "ServiceDetail")]
    public async Task<IActionResult> Detail(int id)
    {
        var dto = await _mediator.Send(new GetServiceRecordDetailQuery(id));
        if (dto == null) return NotFound();

        ViewData["Title"] = $"Servis #{dto.RecordNumber}";
        ViewData["PageTitle"] = dto.RecordNumber;
        ViewData["Breadcrumb"] = $"Servis / {dto.RecordNumber}";

        return View(dto);
    }

    // GET: /Service/Create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Yeni Servis Kaydı";
        ViewData["PageTitle"] = "Yeni Servis Kaydı";

        await PopulateDropdownsAsync();
        return View(new CreateServiceViewModel());
    }

    // POST: /Service/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateServiceViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync();
            return View(model);
        }

        try
        {
            var result = await _mediator.Send(new CreateServiceRecordCommand(
                model.CustomerId, model.DeviceModelId, model.DeviceModelVariantId,
                model.Imei1, model.Imei2, model.SerialNumber, model.DeviceColor,
                model.DevicePassword, model.FaultDescription, model.InternalNotes,
                model.AssignedTechnicianId, null, model.IsUnderWarranty, null));

            TempData["Success"] = $"Servis kaydı oluşturuldu: {result.RecordNumber}";
            return RedirectToAction(nameof(Detail), new { id = result.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            await PopulateDropdownsAsync();
            return View(model);
        }
    }

    // POST: /Service/UpdateStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, ServiceStatus newStatus, string? note, int? technicianId)
    {
        try
        {
            await _mediator.Send(new UpdateServiceStatusCommand(id, newStatus, note, technicianId));
            TempData["Success"] = "Durum güncellendi.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Detail), new { id });
    }

    // POST: /Service/AddPart
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPart(int id, int partId, int quantity, decimal unitCost, bool isReplacement, int? replacesPartId)
    {
        try
        {
            var result = await _mediator.Send(new AddServicePartCommand(id, partId, quantity, unitCost, isReplacement, replacesPartId));
            TempData["Success"] = $"Parça eklendi. Toplam parça maliyeti: {result.NewTotalPartsCost:N2} TL";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Detail), new { id });
    }

    // POST: /Service/PrintLabel
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PrintLabel(int id)
    {
        var ok = await _labelPrint.PrintServiceLabelAsync(id);
        TempData[ok ? "Success" : "Error"] = ok ? "Etiket yazdırıldı." : "Yazıcı bağlantısı kurulamadı.";

        if (ok)
        {
            var record = await _db.ServiceRecords.FindAsync(id);
            if (record != null) { record.LabelPrinted = true; await _db.SaveChangesAsync(); }
        }
        return RedirectToAction(nameof(Detail), new { id });
    }

    // GET: /servis/{token} — Müşteri self-service (public, no login)
    [HttpGet("/servis/{token}")]
    [AllowAnonymous]
    public async Task<IActionResult> PublicQuery(string token)
    {
        var record = await _db.ServiceRecords
            .Include(s => s.Customer)
            .Include(s => s.DeviceModel).ThenInclude(m => m.Brand)
            .Include(s => s.StatusHistories)
            .FirstOrDefaultAsync(s => s.ApprovalToken == token);

        if (record == null) return View("PublicNotFound");

        return View("PublicQuery", record);
    }

    // POST: /servis/{token}/onayla
    [HttpPost("/servis/{token}/onayla")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApprovePrice(string token, bool approved)
    {
        var record = await _db.ServiceRecords.FirstOrDefaultAsync(s => s.ApprovalToken == token);
        if (record == null) return NotFound();

        record.PriceApproved = approved;
        record.ApprovalRespondedAt = DateTime.UtcNow;
        if (!approved)
            record.Status = ServiceStatus.Cancelled;

        await _db.SaveChangesAsync();

        return View("PublicApproved", new { Approved = approved, RecordNumber = record.RecordNumber });
    }

    private async Task PopulateDropdownsAsync()
    {
        ViewBag.DeviceTypes = await _db.DeviceTypes.Where(t => t.IsActive).OrderBy(t => t.SortOrder).ToListAsync();
        ViewBag.Parts = await _db.Parts.Where(p => p.IsActive).OrderBy(p => p.Name).Select(p => new { p.Id, p.Name, p.LastPurchasePrice }).ToListAsync();
    }
}

public class CreateServiceViewModel
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Müşteri seçilmelidir.")]
    public int CustomerId { get; set; }

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Cihaz modeli seçilmelidir.")]
    public int DeviceModelId { get; set; }

    public int? DeviceModelVariantId { get; set; }
    public string? Imei1 { get; set; }
    public string? Imei2 { get; set; }
    public string? SerialNumber { get; set; }
    public string? DeviceColor { get; set; }
    public string? DevicePassword { get; set; }

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Arıza açıklaması zorunludur.")]
    public string FaultDescription { get; set; } = string.Empty;

    public string? InternalNotes { get; set; }
    public int? AssignedTechnicianId { get; set; }
    public bool IsUnderWarranty { get; set; }
}
