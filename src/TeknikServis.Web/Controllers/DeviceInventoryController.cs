using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeknikServis.Application.Common.Interfaces;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Web.Controllers;

[Authorize]
public class DeviceInventoryController : Controller
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _user;

    public DeviceInventoryController(IApplicationDbContext db, ICurrentUserService user)
    {
        _db = db;
        _user = user;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q, InventoryStatus? status, int page = 1, int pageSize = 25)
    {
        ViewData["Title"] = "Cihaz Envanteri";

        var query = _db.DeviceInventories
            .Include(d => d.Catalog).ThenInclude(c => c.Model).ThenInclude(m => m.Brand)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(d =>
                (d.Imei1 != null && d.Imei1.Contains(q)) ||
                (d.Imei2 != null && d.Imei2.Contains(q)) ||
                d.Catalog.Model.Name.Contains(q) ||
                d.Catalog.Model.Brand.Name.Contains(q));

        if (status.HasValue)
            query = query.Where(d => d.Status == status.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Q = q;
        ViewBag.Status = status;
        ViewBag.Page = page;
        ViewBag.TotalCount = total;
        ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);

        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var item = await _db.DeviceInventories
            .Include(d => d.Catalog).ThenInclude(c => c.Model).ThenInclude(m => m.Brand)
            .Include(d => d.PurchaseRecord)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (item == null) return NotFound();

        ViewData["Title"] = $"{item.Catalog?.Model?.Brand?.Name} {item.Catalog?.Model?.Name}";
        return View(item);
    }

    // POST: hızlı durum değiştir (Available/Reserved/Scrapped)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, InventoryStatus status, string? note)
    {
        var item = await _db.DeviceInventories.FindAsync(id);
        if (item == null) return NotFound();

        if (item.Status == InventoryStatus.Sold)
        {
            TempData["Error"] = "Satılmış cihazın durumu değiştirilemez.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        item.Status = status;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Cihaz durumu güncellendi.";
        return RedirectToAction(nameof(Detail), new { id });
    }
}
