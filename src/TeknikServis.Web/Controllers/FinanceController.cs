using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeknikServis.Application.Common.Interfaces;

namespace TeknikServis.Web.Controllers;

[Authorize]
public class FinanceController : Controller
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _user;

    public FinanceController(IApplicationDbContext db, ICurrentUserService user)
    {
        _db = db;
        _user = user;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Hesap bakiyeleri
        var accounts = await _db.Accounts
            .Where(a => a.IsActive)
            .AsNoTracking()
            .ToListAsync();

        // Müşteri alacak özeti
        var customerReceivables = await _db.CustomerReceivables
            .Where(r => !r.IsSettled)
            .GroupBy(r => 1)
            .Select(g => new
            {
                Count = g.Count(),
                TotalRemaining = g.Sum(r => r.RemainingAmount),
                Overdue = g.Count(r => r.DueDate < DateTime.UtcNow)
            })
            .FirstOrDefaultAsync();

        // Bayi alacak özeti
        var dealerReceivables = await _db.DealerReceivables
            .Where(r => !r.IsSettled)
            .GroupBy(r => 1)
            .Select(g => new
            {
                Count = g.Count(),
                TotalRemaining = g.Sum(r => r.RemainingAmount)
            })
            .FirstOrDefaultAsync();

        // Tedarikçi borç özeti
        var supplierPayables = await _db.SupplierPayables
            .Where(p => !p.IsSettled)
            .GroupBy(p => 1)
            .Select(g => new
            {
                Count = g.Count(),
                TotalRemaining = g.Sum(p => p.RemainingAmount)
            })
            .FirstOrDefaultAsync();

        ViewBag.Accounts = accounts;
        ViewBag.CustomerReceivablesCount = customerReceivables?.Count ?? 0;
        ViewBag.CustomerReceivablesTotal = customerReceivables?.TotalRemaining ?? 0m;
        ViewBag.CustomerReceivablesOverdue = customerReceivables?.Overdue ?? 0;
        ViewBag.DealerReceivablesCount = dealerReceivables?.Count ?? 0;
        ViewBag.DealerReceivablesTotal = dealerReceivables?.TotalRemaining ?? 0m;
        ViewBag.SupplierPayablesCount = supplierPayables?.Count ?? 0;
        ViewBag.SupplierPayablesTotal = supplierPayables?.TotalRemaining ?? 0m;

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Receivables(int page = 1, int pageSize = 25)
    {
        var query = _db.CustomerReceivables
            .Include(r => r.Customer)
            .Where(r => !r.IsSettled)
            .AsNoTracking();

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(r => r.DueDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Page = page;
        ViewBag.TotalCount = total;
        ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Payables(int page = 1, int pageSize = 25)
    {
        var query = _db.SupplierPayables
            .Include(p => p.Supplier)
            .Where(p => !p.IsSettled)
            .AsNoTracking();

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.DueDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Page = page;
        ViewBag.TotalCount = total;
        ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);
        return View(items);
    }
}
