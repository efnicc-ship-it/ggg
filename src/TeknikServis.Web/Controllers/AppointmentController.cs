using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeknikServis.Application.Common.Interfaces;

namespace TeknikServis.Web.Controllers;

[Authorize]
public class AppointmentController : Controller
{
    private readonly IApplicationDbContext _db;

    public AppointmentController(IApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Index(DateTime? date, int page = 1, int pageSize = 25)
    {
        ViewData["Title"] = "Randevular";

        var query = _db.AppointmentRecords
            .Include(a => a.Customer)
            .AsNoTracking();

        if (date.HasValue)
            query = query.Where(a => a.AppointmentDate.Date == date.Value.Date);
        else
            query = query.Where(a => a.AppointmentDate >= DateTime.UtcNow.Date);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(a => a.AppointmentDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Date = date?.ToString("yyyy-MM-dd");
        ViewBag.Page = page;
        ViewBag.TotalCount = total;
        ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);

        return View(items);
    }
}
