using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeknikServis.Application.Common.Interfaces;

namespace TeknikServis.Api.Controllers.v1;

[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly IApplicationDbContext _db;

    public CustomersController(IApplicationDbContext db) => _db = db;

    /// <summary>Autocomplete search for customer picker components</summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Ok(new List<object>());

        var results = await _db.Customers
            .AsNoTracking()
            .Where(c => !c.IsBlacklisted &&
                (c.FirstName.Contains(q) ||
                 c.LastName.Contains(q) ||
                 c.Phone.Contains(q)))
            .OrderBy(c => c.FirstName)
            .Take(10)
            .Select(c => new
            {
                c.Id,
                Name = c.FirstName + " " + c.LastName,
                c.Phone
            })
            .ToListAsync(ct);

        return Ok(results);
    }
}
