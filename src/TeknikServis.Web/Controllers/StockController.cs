using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeknikServis.Application.Common.Interfaces;

namespace TeknikServis.Web.Controllers;

[Authorize]
public class StockController : Controller
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _user;

    public StockController(IApplicationDbContext db, ICurrentUserService user)
    {
        _db = db;
        _user = user;
    }

    // Stok ana sayfası — Parts'a yönlendir
    [HttpGet]
    public IActionResult Index() => RedirectToAction(nameof(Parts));

    // Parça stok listesi
    [HttpGet]
    public async Task<IActionResult> Parts(string? q, int page = 1, int pageSize = 25)
    {
        var query = _db.StockItems
            .Include(si => si.Part)
            .Where(si => si.PartId != null)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(si => si.Part != null && (si.Part.Name.Contains(q) ||
                (si.Part.Barcode != null && si.Part.Barcode.Contains(q))));

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(si => si.Part!.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(si => new
            {
                si.Id,
                PartName = si.Part!.Name,
                PartCode = si.Part.Barcode ?? si.Part.Sku ?? "—",
                Quantity = si.AvailableQuantity,
                ReservedQuantity = si.ReservedQuantity,
                ReorderThreshold = si.Part.LowStockThreshold,
                si.AverageCost,
                IsLow = si.AvailableQuantity <= si.Part.LowStockThreshold
            })
            .ToListAsync();

        ViewBag.Q = q;
        ViewBag.Page = page;
        ViewBag.TotalCount = total;
        ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);
        return View(items);
    }

    // Cihaz envanteri (ikinci el)
    [HttpGet]
    public async Task<IActionResult> Devices(string? q, int page = 1, int pageSize = 25)
    {
        var query = _db.DeviceInventories
            .Include(d => d.Catalog).ThenInclude(c => c.Model).ThenInclude(m => m.Brand)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(d =>
                (d.Imei1 != null && d.Imei1.Contains(q)) ||
                d.Catalog.Model.Name.Contains(q) ||
                d.Catalog.Model.Brand.Name.Contains(q));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Q = q;
        ViewBag.Page = page;
        ViewBag.TotalCount = total;
        ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);
        return View(items);
    }

    // Fire/israf kayıtları
    [HttpGet]
    public async Task<IActionResult> WriteOffs(int page = 1, int pageSize = 25)
    {
        var items = await _db.StockWriteOffs
            .Include(w => w.Part)
            .Include(w => w.DeviceAccessory)
            .Include(w => w.DeviceCompanion)
            .Include(w => w.UniversalAccessory)
            .AsNoTracking()
            .OrderByDescending(w => w.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var total = await _db.StockWriteOffs.CountAsync();
        ViewBag.TotalCount = total;
        ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);
        ViewBag.Page = page;
        return View(items);
    }
}
