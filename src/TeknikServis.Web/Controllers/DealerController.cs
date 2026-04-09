using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeknikServis.Application.Common.Interfaces;
using TeknikServis.Domain.Entities.Dealer;
using TeknikServis.Domain.Entities.Service;
using TeknikServis.Domain.Enums;

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

    // ─── Admin: Bayi Listesi ─────────────────────────────────────────────────

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
        string? email, string? address, string? taxNumber, decimal creditLimit,
        decimal? autoApprovalLimit, bool defaultDealerIsContact = true)
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
            IsActive = true,
            AutoApprovalLimit = autoApprovalLimit > 0 ? autoApprovalLimit : null,
            DefaultDealerIsContact = defaultDealerIsContact,
            ShowMaskedStatusOnly = true
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = $"'{companyName}' bayisi oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Dealer/5 — Bayi detay (admin)
    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var dealer = await _db.Dealers.FindAsync(id);
        if (dealer == null) return NotFound();

        var records = await _db.ServiceRecords
            .Include(s => s.Customer)
            .Include(s => s.DeviceModel).ThenInclude(m => m.Brand)
            .Where(s => s.DealerId == id)
            .OrderByDescending(s => s.CreatedAt)
            .Take(50)
            .AsNoTracking()
            .ToListAsync();

        var batches = await _db.DealerBatchShipments
            .Where(b => b.DealerId == id)
            .OrderByDescending(b => b.ShippedAt)
            .Take(10)
            .AsNoTracking()
            .ToListAsync();

        ViewBag.Records = records;
        ViewBag.Batches = batches;
        return View(dealer);
    }

    // POST: /Dealer/Edit — Bayi güncelle (oto onay limiti + iletişim tercihi)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, decimal? autoApprovalLimit,
        bool defaultDealerIsContact, bool showMaskedStatusOnly, decimal creditLimit)
    {
        var dealer = await _db.Dealers.FindAsync(id);
        if (dealer == null) return NotFound();

        dealer.AutoApprovalLimit = autoApprovalLimit > 0 ? autoApprovalLimit : null;
        dealer.DefaultDealerIsContact = defaultDealerIsContact;
        dealer.ShowMaskedStatusOnly = showMaskedStatusOnly;
        dealer.CreditLimit = creditLimit;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Bayi ayarları güncellendi.";
        return RedirectToAction(nameof(Detail), new { id });
    }

    // ─── Bayi Portali ────────────────────────────────────────────────────────
    // Not: Bayi portali /bayi/* prefix'i altında ayrı controller'a taşınabilir.
    // Şimdilik bayi kaydını admin controller üzerinden de açılabilir.

    // GET: /Dealer/PortalRegister — Bayiden servis kaydı aç (admin adına)
    [HttpGet]
    public async Task<IActionResult> PortalRegister(int dealerId)
    {
        var dealer = await _db.Dealers.FindAsync(dealerId);
        if (dealer == null) return NotFound();

        ViewBag.Dealer = dealer;
        ViewBag.DeviceTypes = await _db.DeviceTypes.Where(t => t.IsActive).OrderBy(t => t.SortOrder).ToListAsync();
        ViewBag.Customers = await _db.Customers
            .OrderBy(c => c.FirstName)
            .Select(c => new { c.Id, FullName = c.FirstName + " " + c.LastName, c.Phone })
            .AsNoTracking()
            .ToListAsync();
        return View();
    }

    // POST: /Dealer/PortalRegister — Bayi adına servis kaydı oluştur
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PortalRegister(
        int dealerId, int customerId, int deviceModelId, int? deviceModelVariantId,
        string? imei1, string? imei2, string? serialNumber, string? deviceColor,
        string faultDescription, string? internalNotes,
        bool dealerIsContactPerson,
        string? inboundCargoTrackingNumber, string? inboundCargoCompany)
    {
        var dealer = await _db.Dealers.FindAsync(dealerId);
        if (dealer == null) return NotFound();

        // Kayıt numarası üret
        var count = await _db.ServiceRecords.CountAsync() + 1;
        var recordNumber = $"SRV-{DateTime.UtcNow.Year}-{count:D6}";

        var record = new ServiceRecord
        {
            RecordNumber = recordNumber,
            CustomerId = customerId,
            DealerId = dealerId,
            DeviceModelId = deviceModelId,
            DeviceModelVariantId = deviceModelVariantId,
            Imei1 = imei1,
            Imei2 = imei2,
            SerialNumber = serialNumber,
            DeviceColor = deviceColor,
            FaultDescription = faultDescription,
            InternalNotes = internalNotes,
            DealerIsContactPerson = dealerIsContactPerson,
            InboundCargoTrackingNumber = inboundCargoTrackingNumber,
            InboundCargoCompany = inboundCargoCompany,
            // Kargo bilgisi varsa DealerInTransit, yoksa DealerRegistered
            Status = !string.IsNullOrWhiteSpace(inboundCargoTrackingNumber)
                ? ServiceStatus.DealerInTransit
                : ServiceStatus.DealerRegistered,
            ApprovalToken = Guid.NewGuid().ToString("N")
        };

        _db.ServiceRecords.Add(record);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Bayi servis kaydı açıldı: {recordNumber}";
        return RedirectToAction("Detail", "Service", new { id = record.Id });
    }
}
