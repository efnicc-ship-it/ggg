using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeknikServis.Application.Common.Interfaces;

namespace TeknikServis.Api.Controllers.v1;

[ApiController]
[Route("api/catalog")]
[Authorize]
public class CatalogController : ControllerBase
{
    private readonly IApplicationDbContext _db;

    public CatalogController(IApplicationDbContext db) => _db = db;

    [HttpGet("types")]
    public async Task<IActionResult> GetTypes(CancellationToken ct)
    {
        var types = await _db.DeviceTypes
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new { t.Id, t.Name })
            .ToListAsync(ct);
        return Ok(types);
    }

    [HttpGet("brands")]
    public async Task<IActionResult> GetBrands([FromQuery] int? typeId, CancellationToken ct)
    {
        var q = _db.DeviceBrands.AsNoTracking();
        if (typeId.HasValue)
            q = q.Where(b => b.DeviceTypeId == typeId);

        var brands = await q
            .OrderBy(b => b.Name)
            .Select(b => new { b.Id, b.Name })
            .ToListAsync(ct);
        return Ok(brands);
    }

    [HttpGet("models")]
    public async Task<IActionResult> GetModels([FromQuery] int? brandId, CancellationToken ct)
    {
        var q = _db.DeviceModels.AsNoTracking();
        if (brandId.HasValue)
            q = q.Where(m => m.BrandId == brandId);

        var models = await q
            .OrderBy(m => m.Name)
            .Select(m => new { m.Id, m.Name })
            .ToListAsync(ct);
        return Ok(models);
    }

    [HttpGet("variants")]
    public async Task<IActionResult> GetVariants([FromQuery] int? modelId, CancellationToken ct)
    {
        var q = _db.DeviceModelVariants.AsNoTracking();
        if (modelId.HasValue)
            q = q.Where(v => v.DeviceModelId == modelId);

        var variants = await q
            .OrderBy(v => v.Name)
            .Select(v => new { v.Id, v.Name, v.ModelCode })
            .ToListAsync(ct);
        return Ok(variants);
    }
}
