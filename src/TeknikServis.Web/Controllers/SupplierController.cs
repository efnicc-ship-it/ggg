using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeknikServis.Application.Common.Interfaces;
using TeknikServis.Domain.Entities.Supplier;

namespace TeknikServis.Web.Controllers;

[Authorize]
public class SupplierController : Controller
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _user;

    public SupplierController(IApplicationDbContext db, ICurrentUserService user)
    {
        _db = db;
        _user = user;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 25)
    {
        var query = _db.Suppliers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(s =>
                s.CompanyName.Contains(q) ||
                s.ContactName.Contains(q) ||
                (s.Phone != null && s.Phone.Contains(q)));

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(s => s.CompanyName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Q = q;
        ViewBag.Page = page;
        ViewBag.TotalCount = total;
        ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);
        return View(items);
    }

    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string companyName, string contactName, string phone,
        string? whatsAppPhone, string? email, string? address, string? taxNumber,
        bool autoSendWhatsApp, string? notes)
    {
        _db.Suppliers.Add(new Supplier
        {
            CompanyName = companyName,
            ContactName = contactName,
            Phone = phone,
            WhatsAppPhone = whatsAppPhone,
            Email = email,
            Address = address,
            TaxNumber = taxNumber,
            AutoSendWhatsApp = autoSendWhatsApp,
            Notes = notes,
            IsActive = true
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = $"'{companyName}' tedarikçisi oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    // Siparişler
    [HttpGet]
    public async Task<IActionResult> Orders(int page = 1, int pageSize = 25)
    {
        var query = _db.PartOrders
            .Include(o => o.Supplier)
            .AsNoTracking();

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Page = page;
        ViewBag.TotalCount = total;
        ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);
        return View(items);
    }
}
