using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeknikServis.Application.Common.Interfaces;
using TeknikServis.Application.Features.Sale.Commands.CreateSale;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Web.Controllers;

[Authorize]
public class SaleController : Controller
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _user;
    private readonly IMediator _mediator;

    public SaleController(IApplicationDbContext db, ICurrentUserService user, IMediator mediator)
    {
        _db = db;
        _user = user;
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 25)
    {
        var query = _db.SaleRecords
            .Include(s => s.Customer)
            .Include(s => s.Items)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(s =>
                s.RecordNumber.Contains(q) ||
                (s.Customer != null && s.Customer.FirstName.Contains(q)));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Q = q;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalCount = total;
        ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> ScrapSales(int page = 1, int pageSize = 25)
    {
        var items = await _db.ScrapSaleRecords
            .AsNoTracking()
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var total = await _db.ScrapSaleRecords.CountAsync();
        ViewBag.TotalCount = total;
        ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);
        ViewBag.Page = page;
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? deviceInventoryId, int? customerId)
    {
        ViewData["Title"] = "Yeni Satış";

        // Seçili cihaz varsa bilgilerini getir
        if (deviceInventoryId.HasValue)
        {
            var inv = await _db.DeviceInventories
                .Include(i => i.Catalog)
                .FirstOrDefaultAsync(i => i.Id == deviceInventoryId.Value);
            ViewBag.SelectedDevice = inv;
        }

        // Seçili müşteri varsa bilgilerini getir
        if (customerId.HasValue)
        {
            var cust = await _db.Customers.FindAsync(customerId.Value);
            ViewBag.SelectedCustomer = cust;
        }

        // Mevcut cihaz envanteri (satılabilir)
        ViewBag.AvailableDevices = await _db.DeviceInventories
            .Include(i => i.Catalog)
            .Where(i => i.Status == InventoryStatus.Available)
            .OrderByDescending(i => i.CreatedAt)
            .Take(50)
            .AsNoTracking()
            .ToListAsync();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        int? customerId,
        bool isAccessorySaleOnly,
        List<int?> deviceInventoryIds,
        List<int?> stockItemIds,
        List<int> quantities,
        List<decimal> unitPrices,
        List<string> descriptions,
        PaymentStatus paymentStatus,
        decimal paidAmount,
        int? warrantyDays,
        string? notes,
        int? discountAmount)
    {
        try
        {
            var items = new List<SaleItemInput>();
            for (int i = 0; i < quantities.Count; i++)
            {
                items.Add(new SaleItemInput(
                    deviceInventoryIds.ElementAtOrDefault(i),
                    stockItemIds.ElementAtOrDefault(i),
                    quantities[i],
                    unitPrices[i],
                    descriptions.ElementAtOrDefault(i) ?? ""
                ));
            }

            var result = await _mediator.Send(new CreateSaleCommand(
                customerId, isAccessorySaleOnly, items,
                paymentStatus, paidAmount, warrantyDays, notes, discountAmount));

            TempData["Success"] = $"Satış kaydedildi: {result.RecordNumber} — Toplam: {result.TotalAmount:N2} ₺";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Create));
        }
    }
}
