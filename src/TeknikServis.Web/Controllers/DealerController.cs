using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeknikServis.Application.Common.Interfaces;
using TeknikServis.Domain.Entities.Dealer;

namespace TeknikServis.Web.Controllers;

[Authorize]
public class DealerController : Controller
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _user;

    public DealerController(IApplicationDbContext db, ICurrentUserService user)
    {
        _db = db;
        _user = user;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 25)
    {
        var query = _db.Dealers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(d =>
                d.CompanyName.Contains(q) ||
                d.ContactName.Contains(q) ||
                (d.Phone != null && d.Phone.Contains(q)));

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(d => d.CompanyName)
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
        string? email, string? address, string? taxNumber, decimal creditLimit)
    {
        _db.Dealers.Add(new Dealer
        {
            CompanyName = companyName,
            ContactName = contactName,
            Phone = phone,
            Email = email,
            Address = address,
            TaxNumber = taxNumber,
            CreditLimit = creditLimit,
            CurrentBalance = 0,
            IsActive = true
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = $"'{companyName}' bayisi oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }
}
