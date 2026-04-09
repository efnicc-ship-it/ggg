using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeknikServis.Application.Common.Interfaces;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Web.Controllers;

[Authorize]
public class SaleController : Controller
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _user;

    public SaleController(IApplicationDbContext db, ICurrentUserService user)
    {
        _db = db;
        _user = user;
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
}
