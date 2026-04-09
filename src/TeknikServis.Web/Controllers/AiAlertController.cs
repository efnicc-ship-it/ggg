using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeknikServis.Application.Common.Interfaces;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Web.Controllers;

[Authorize]
public class AiAlertController : Controller
{
    private readonly IApplicationDbContext _db;

    public AiAlertController(IApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Index(AlertSeverity? severity, bool? resolved, int page = 1, int pageSize = 25)
    {
        ViewData["Title"] = "Ensar AI Uyarıları";

        var query = _db.AiAlerts
            .Include(a => a.Rule)
            .AsNoTracking();

        if (severity.HasValue)
            query = query.Where(a => a.Severity == severity.Value);

        if (resolved.HasValue)
            query = query.Where(a => a.IsResolved == resolved.Value);
        else
            query = query.Where(a => !a.IsResolved); // varsayılan: çözümlenmemiş

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.Severity)
            .ThenByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Severity = severity;
        ViewBag.Resolved = resolved;
        ViewBag.Page = page;
        ViewBag.TotalCount = total;
        ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);

        return View(items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resolve(int id)
    {
        var alert = await _db.AiAlerts.FindAsync(id);
        if (alert == null) return NotFound();

        alert.IsResolved = true;
        alert.ResolvedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Uyarı çözümlendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResolveAll()
    {
        var unresolvedAlerts = await _db.AiAlerts
            .Where(a => !a.IsResolved)
            .ToListAsync();

        foreach (var alert in unresolvedAlerts)
        {
            alert.IsResolved = true;
            alert.ResolvedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = $"{unresolvedAlerts.Count} uyarı çözümlendi.";
        return RedirectToAction(nameof(Index));
    }
}
