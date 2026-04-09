using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeknikServis.Application.Common.Interfaces;
using TeknikServis.Domain.Entities.Device;

namespace TeknikServis.Web.Controllers;

[Authorize]
public class CatalogController : Controller
{
    private readonly IApplicationDbContext _db;

    public CatalogController(IApplicationDbContext db) => _db = db;

    // ────── ANA SAYFA ──────
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var types = await _db.DeviceTypes
            .Include(t => t.Brands).ThenInclude(b => b.Models)
            .AsNoTracking()
            .OrderBy(t => t.SortOrder).ThenBy(t => t.Name)
            .ToListAsync();

        return View(types);
    }

    // ────── DEVICE TYPES ──────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateType(string name, string? description, int sortOrder = 0)
    {
        _db.DeviceTypes.Add(new DeviceType { Name = name, Description = description, SortOrder = sortOrder, IsActive = true });
        await _db.SaveChangesAsync();
        TempData["Success"] = $"'{name}' cihaz türü oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleType(int id)
    {
        var t = await _db.DeviceTypes.FindAsync(id);
        if (t != null) { t.IsActive = !t.IsActive; await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }

    // ────── BRANDS ──────
    [HttpGet]
    public async Task<IActionResult> Brands(int typeId)
    {
        var type = await _db.DeviceTypes.FindAsync(typeId);
        if (type == null) return NotFound();

        var brands = await _db.DeviceBrands
            .Where(b => b.DeviceTypeId == typeId)
            .OrderBy(b => b.SortOrder).ThenBy(b => b.Name)
            .AsNoTracking()
            .ToListAsync();

        ViewBag.DeviceType = type;
        return View(brands);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBrand(int typeId, string name, int sortOrder = 0)
    {
        _db.DeviceBrands.Add(new DeviceBrand { DeviceTypeId = typeId, Name = name, SortOrder = sortOrder, IsActive = true });
        await _db.SaveChangesAsync();
        TempData["Success"] = $"'{name}' markası oluşturuldu.";
        return RedirectToAction(nameof(Brands), new { typeId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleBrand(int id, int typeId)
    {
        var b = await _db.DeviceBrands.FindAsync(id);
        if (b != null) { b.IsActive = !b.IsActive; await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Brands), new { typeId });
    }

    // ────── MODELS ──────
    [HttpGet]
    public async Task<IActionResult> Models(int brandId)
    {
        var brand = await _db.DeviceBrands
            .Include(b => b.DeviceType)
            .FirstOrDefaultAsync(b => b.Id == brandId);
        if (brand == null) return NotFound();

        var models = await _db.DeviceModels
            .Where(m => m.BrandId == brandId)
            .Include(m => m.Variants)
            .OrderBy(m => m.SortOrder).ThenBy(m => m.Name)
            .AsNoTracking()
            .ToListAsync();

        ViewBag.Brand = brand;
        return View(models);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateModel(int brandId, string name, string? modelCode,
        bool hasVariants, string? description, int sortOrder = 0)
    {
        _db.DeviceModels.Add(new DeviceModel
        {
            BrandId = brandId,
            Name = name,
            ModelCode = modelCode,
            HasVariants = hasVariants,
            Description = description,
            SortOrder = sortOrder,
            IsActive = true
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = $"'{name}' modeli oluşturuldu.";
        return RedirectToAction(nameof(Models), new { brandId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleModel(int id, int brandId)
    {
        var m = await _db.DeviceModels.FindAsync(id);
        if (m != null) { m.IsActive = !m.IsActive; await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Models), new { brandId });
    }

    // ────── VARIANTS ──────
    [HttpGet]
    public async Task<IActionResult> Variants(int modelId)
    {
        var model = await _db.DeviceModels
            .Include(m => m.Brand).ThenInclude(b => b.DeviceType)
            .FirstOrDefaultAsync(m => m.Id == modelId);
        if (model == null) return NotFound();

        var variants = await _db.DeviceModelVariants
            .Where(v => v.ModelId == modelId)
            .OrderBy(v => v.SortOrder).ThenBy(v => v.VariantName)
            .AsNoTracking()
            .ToListAsync();

        ViewBag.Model = model;
        return View(variants);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateVariant(int modelId, string variantName, string modelCode,
        string? storage, string? ram, string? color, int sortOrder = 0)
    {
        _db.DeviceModelVariants.Add(new DeviceModelVariant
        {
            ModelId = modelId,
            VariantName = variantName,
            ModelCode = modelCode,
            Storage = storage,
            Ram = ram,
            Color = color,
            SortOrder = sortOrder,
            IsActive = true
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = $"'{variantName}' varyantı oluşturuldu.";
        return RedirectToAction(nameof(Variants), new { modelId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleVariant(int id, int modelId)
    {
        var v = await _db.DeviceModelVariants.FindAsync(id);
        if (v != null) { v.IsActive = !v.IsActive; await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Variants), new { modelId });
    }

    // ─────────────────────────────────────────────────────────────
    // AJAX — Cascading dropdowns (Servis/Alış/Satış formlarında)
    // ─────────────────────────────────────────────────────────────

    // GET: /Catalog/Ajax/Types
    [HttpGet("/catalog/ajax/types")]
    public async Task<IActionResult> AjaxTypes()
    {
        var types = await _db.DeviceTypes
            .Where(t => t.IsActive)
            .OrderBy(t => t.SortOrder).ThenBy(t => t.Name)
            .Select(t => new { t.Id, t.Name })
            .AsNoTracking()
            .ToListAsync();
        return Json(types);
    }

    // GET: /Catalog/Ajax/Brands?typeId=1
    [HttpGet("/catalog/ajax/brands")]
    public async Task<IActionResult> AjaxBrands(int typeId)
    {
        var brands = await _db.DeviceBrands
            .Where(b => b.DeviceTypeId == typeId && b.IsActive)
            .OrderBy(b => b.SortOrder).ThenBy(b => b.Name)
            .Select(b => new { b.Id, b.Name })
            .AsNoTracking()
            .ToListAsync();
        return Json(brands);
    }

    // GET: /Catalog/Ajax/Models?brandId=5
    [HttpGet("/catalog/ajax/models")]
    public async Task<IActionResult> AjaxModels(int brandId)
    {
        var models = await _db.DeviceModels
            .Where(m => m.BrandId == brandId && m.IsActive)
            .OrderBy(m => m.SortOrder).ThenBy(m => m.Name)
            .Select(m => new { m.Id, m.Name, m.ModelCode, m.HasVariants })
            .AsNoTracking()
            .ToListAsync();
        return Json(models);
    }

    // GET: /Catalog/Ajax/Variants?modelId=12
    [HttpGet("/catalog/ajax/variants")]
    public async Task<IActionResult> AjaxVariants(int modelId)
    {
        var variants = await _db.DeviceModelVariants
            .Where(v => v.ModelId == modelId && v.IsActive)
            .OrderBy(v => v.SortOrder).ThenBy(v => v.VariantName)
            .Select(v => new { v.Id, v.VariantName, v.ModelCode, v.Storage, v.Ram, v.Color })
            .AsNoTracking()
            .ToListAsync();
        return Json(variants);
    }

    // GET: /Catalog/Ajax/CustomerSearch?q=ahmet
    [HttpGet("/catalog/ajax/customers")]
    public async Task<IActionResult> AjaxCustomers(string? q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Json(Array.Empty<object>());

        var customers = await _db.Customers
            .Where(c => !c.IsDeleted &&
                (c.Phone.Contains(q) || c.FirstName.Contains(q) || c.LastName.Contains(q)))
            .Take(10)
            .Select(c => new { c.Id, fullName = c.FirstName + " " + c.LastName, c.Phone })
            .AsNoTracking()
            .ToListAsync();
        return Json(customers);
    }

    // GET: /Catalog/Ajax/Parts?q=ekran&modelId=12
    [HttpGet("/catalog/ajax/parts")]
    public async Task<IActionResult> AjaxParts(string? q, int? modelId)
    {
        var query = _db.Parts.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(p => p.Name.Contains(q) || p.Barcode!.Contains(q));

        if (modelId.HasValue)
            query = query.Where(p => _db.PartCompatibilities
                .Any(pc => pc.PartId == p.Id && pc.DeviceModelId == modelId.Value));

        var parts = await query
            .Take(15)
            .Select(p => new
            {
                p.Id, p.Name, p.Barcode,
                stock = _db.StockItems.Where(s => s.PartId == p.Id).Select(s => s.AvailableQuantity).FirstOrDefault()
            })
            .AsNoTracking()
            .ToListAsync();

        return Json(parts);
    }

    // GET: /catalog/ajax/faults — Yaygın arıza tipleri
    [HttpGet("/catalog/ajax/faults")]
    public async Task<IActionResult> AjaxFaults(string? q)
    {
        var query = _db.CommonFaultTypes.Where(f => f.IsActive);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(f => f.Name.Contains(q));

        var faults = await query
            .OrderBy(f => f.SortOrder).ThenBy(f => f.Name)
            .Take(20)
            .Select(f => new { f.Id, f.Name })
            .ToListAsync();

        return Json(faults);
    }

    // GET: /catalog/ajax/actions — Yaygın işlem tipleri
    [HttpGet("/catalog/ajax/actions")]
    public async Task<IActionResult> AjaxActions(string? q)
    {
        var query = _db.CommonActionTypes.Where(a => a.IsActive);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(a => a.Name.Contains(q));

        var actions = await query
            .OrderBy(a => a.SortOrder).ThenBy(a => a.Name)
            .Take(20)
            .Select(a => new { a.Id, a.Name })
            .ToListAsync();

        return Json(actions);
    }

    // ─── CommonFault / CommonAction Admin ────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Faults()
    {
        ViewData["Title"] = "Yaygın Arızalar";
        var faults = await _db.CommonFaultTypes.OrderBy(f => f.SortOrder).ThenBy(f => f.Name).ToListAsync();
        var actions = await _db.CommonActionTypes.OrderBy(a => a.SortOrder).ThenBy(a => a.Name).ToListAsync();
        ViewBag.Actions = actions;
        return View(faults);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFault(string name, string? description, int sortOrder = 0)
    {
        _db.CommonFaultTypes.Add(new Domain.Entities.CommonFault.CommonFaultType
        { Name = name, Description = description, SortOrder = sortOrder });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Arıza tipi eklendi.";
        return RedirectToAction(nameof(Faults));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFault(int id)
    {
        var f = await _db.CommonFaultTypes.FindAsync(id);
        if (f != null) { f.IsActive = !f.IsActive; await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Faults));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAction(string name, string? description, int sortOrder = 0)
    {
        _db.CommonActionTypes.Add(new Domain.Entities.CommonFault.CommonActionType
        { Name = name, Description = description, SortOrder = sortOrder });
        await _db.SaveChangesAsync();
        TempData["Success"] = "İşlem tipi eklendi.";
        return RedirectToAction(nameof(Faults));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAction(int id)
    {
        var a = await _db.CommonActionTypes.FindAsync(id);
        if (a != null) { a.IsActive = !a.IsActive; await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Faults));
    }
}
