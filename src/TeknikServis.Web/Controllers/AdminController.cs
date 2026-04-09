using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeknikServis.Application.Common.Interfaces;
using TeknikServis.Domain.Entities.EnsarAI;
using TeknikServis.Domain.Entities.Identity;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Web.Controllers;

[Authorize]
public class AdminController : Controller
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _user;

    public AdminController(IApplicationDbContext db, ICurrentUserService user)
    {
        _db = db;
        _user = user;
    }

    [HttpGet]
    public IActionResult Index() => View();

    // Kullanıcılar
    [HttpGet]
    public async Task<IActionResult> Users()
    {
        var assignments = await _db.UserRoleAssignments
            .AsNoTracking()
            .ToListAsync();

        // Kullanıcı adlarını çek
        var userIds = assignments.Select(a => a.UserId).Distinct().ToList();
        var users = await _db.AppUsers
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FirstName, u.LastName, u.Email, u.UserName })
            .AsNoTracking()
            .ToListAsync();

        ViewBag.Users = users.ToDictionary(u => u.Id, u => $"{u.FirstName} {u.LastName}".Trim().Length > 0
            ? $"{u.FirstName} {u.LastName}"
            : u.UserName ?? u.Email ?? u.Id.ToString());

        return View(assignments);
    }

    // Rol Atamaları
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignRole(int userId, string roleName, DateTime? expiresAt)
    {
        var existing = await _db.UserRoleAssignments
            .FirstOrDefaultAsync(r => r.UserId == userId && r.RoleName == roleName);

        if (existing == null)
        {
            _db.UserRoleAssignments.Add(new UserRoleAssignment
            {
                UserId = userId,
                RoleName = roleName,
                ExpiresAt = expiresAt
            });
            await _db.SaveChangesAsync();
            TempData["Success"] = "Rol atandı.";
        }
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveRole(int assignmentId)
    {
        var assignment = await _db.UserRoleAssignments.FindAsync(assignmentId);
        if (assignment != null)
        {
            _db.UserRoleAssignments.Remove(assignment);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Rol kaldırıldı.";
        }
        return RedirectToAction(nameof(Users));
    }

    // Özel Roller
    [HttpGet]
    public async Task<IActionResult> CustomRoles()
    {
        var roles = await _db.CustomRoles.AsNoTracking().ToListAsync();
        return View(roles);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCustomRole(string name, string? description, string? permissionsJson)
    {
        _db.CustomRoles.Add(new CustomRole
        {
            Name = name,
            Description = description,
            PermissionsJson = permissionsJson ?? "[]"
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = $"'{name}' rolü oluşturuldu.";
        return RedirectToAction(nameof(CustomRoles));
    }

    // Ensar AI Kuralları
    [HttpGet]
    public async Task<IActionResult> AiRules()
    {
        var rules = await _db.AiRules.AsNoTracking().ToListAsync();
        return View(rules);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAiRule(int id)
    {
        var rule = await _db.AiRules.FindAsync(id);
        if (rule != null)
        {
            rule.IsActive = !rule.IsActive;
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Kural {(rule.IsActive ? "etkinleştirildi" : "devre dışı bırakıldı")}.";
        }
        return RedirectToAction(nameof(AiRules));
    }

    // AI Alerts
    [HttpGet]
    public async Task<IActionResult> Alerts()
    {
        var alerts = await _db.AiAlerts
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedAt)
            .Take(100)
            .ToListAsync();
        return View(alerts);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAlertRead(int id)
    {
        var alert = await _db.AiAlerts.FindAsync(id);
        if (alert != null)
        {
            alert.IsResolved = true;
            alert.ResolvedAt = DateTime.UtcNow;
            alert.ResolvedBy = _user.UserId;
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Alerts));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllAlertsRead()
    {
        var unread = await _db.AiAlerts.Where(a => !a.IsResolved).ToListAsync();
        foreach (var a in unread)
        {
            a.IsResolved = true;
            a.ResolvedAt = DateTime.UtcNow;
            a.ResolvedBy = _user.UserId;
        }
        await _db.SaveChangesAsync();
        TempData["Success"] = $"{unread.Count} uyarı çözümlendi.";
        return RedirectToAction(nameof(Alerts));
    }
}
