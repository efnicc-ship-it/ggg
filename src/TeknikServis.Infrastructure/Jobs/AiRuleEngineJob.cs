using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TeknikServis.Application.Common.Interfaces;
using TeknikServis.Domain.Entities.EnsarAI;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Infrastructure.Jobs;

/// <summary>
/// Ensar AI kural motoru — Hangfire tarafından tetiklenir.
/// Zamanlanmış (Scheduled) kuralları çalıştırır.
/// Olay tabanlı (Event) kurallar Application/Infrastructure servislerinde doğrudan tetiklenir.
/// </summary>
public class AiRuleEngineJob
{
    private readonly IApplicationDbContext _db;
    private readonly ILogger<AiRuleEngineJob> _logger;

    public AiRuleEngineJob(IApplicationDbContext db, ILogger<AiRuleEngineJob> logger)
    {
        _db = db;
        _logger = logger;
    }

    // Kural 1: Teknisyen 24/48 saat hareketsiz
    [AutomaticRetry(Attempts = 2)]
    public async Task CheckIdleTechnicians(CancellationToken ct = default)
    {
        _logger.LogInformation("AI Kural: Teknisyen hareketsizlik kontrolü başlatıldı");

        var now = DateTime.UtcNow;
        var threshold24 = now.AddHours(-24);
        var threshold48 = now.AddHours(-48);

        var ruleId = await GetRuleIdAsync("Teknisyen 24 Saat Hareketsiz", ct);

        var records = await _db.ServiceRecords
            .Include(s => s.TimeLogs)
            .Where(s => s.Status == ServiceStatus.InRepair || s.Status == ServiceStatus.Diagnosing)
            .Where(s => s.AssignedTechnicianId.HasValue)
            .AsNoTracking()
            .ToListAsync(ct);

        foreach (var record in records)
        {
            var lastActivity = record.TimeLogs.Any()
                ? record.TimeLogs.Max(t => t.StartedAt)
                : record.CreatedAt;

            if (lastActivity < threshold48)
            {
                await CreateAlertAsync(ruleId, record.TenantId, record.Id,
                    $"TEKNİSYEN UYARISI: {record.RecordNumber} 48 saattir bekliyor",
                    AlertSeverity.Critical, ct);
            }
            else if (lastActivity < threshold24)
            {
                await CreateAlertAsync(ruleId, record.TenantId, record.Id,
                    $"Cihaza 24 saattir müdahale edilmedi: {record.RecordNumber}",
                    AlertSeverity.Warning, ct);
            }
        }
    }

    // Kural 2: Parça siparişi 4 saat verilmedi
    [AutomaticRetry(Attempts = 2)]
    public async Task CheckUnorderedParts(CancellationToken ct = default)
    {
        _logger.LogInformation("AI Kural: Parça sipariş kontrolü başlatıldı");

        var threshold = DateTime.UtcNow.AddHours(-4);
        var ruleId = await GetRuleIdAsync("Parça Siparişi 4 Saat Verilmedi", ct);

        var records = await _db.ServiceRecords
            .Where(s => s.Status == ServiceStatus.AwaitingParts && s.CreatedAt < threshold)
            .AsNoTracking()
            .ToListAsync(ct);

        foreach (var record in records)
        {
            var hasOrder = await _db.ServiceParts
                .AnyAsync(sp => sp.ServiceRecordId == record.Id && sp.PartOrderItemId.HasValue, ct);

            if (!hasOrder)
            {
                await CreateAlertAsync(ruleId, record.TenantId, record.Id,
                    $"Parça siparişi verilmedi: {record.RecordNumber} (4+ saat)",
                    AlertSeverity.Warning, ct);
            }
        }
    }

    // Kural 7: Cihaz 60 gün satılmadı
    [AutomaticRetry(Attempts = 2)]
    public async Task CheckUnsoldDevices(CancellationToken ct = default)
    {
        _logger.LogInformation("AI Kural: 60 gün satılmayan cihazlar kontrol ediliyor");

        var threshold = DateTime.UtcNow.AddDays(-60);
        var ruleId = await GetRuleIdAsync("Cihaz 60 Gün Satılmadı", ct);

        var devices = await _db.DeviceInventories
            .Include(d => d.Catalog).ThenInclude(c => c.Model).ThenInclude(m => m.Brand)
            .Where(d => d.Status == InventoryStatus.Available && d.CreatedAt < threshold)
            .AsNoTracking()
            .ToListAsync(ct);

        foreach (var device in devices)
        {
            var dayCount = (int)(DateTime.UtcNow - device.CreatedAt).TotalDays;
            await CreateAlertAsync(ruleId, device.TenantId, device.Id,
                $"FİYAT ÖNERİSİ: {device.Catalog.Model.Brand.Name} {device.Catalog.Model.Name} - {dayCount} gündür satılmadı",
                AlertSeverity.Info, ct);
        }
    }

    // Kural 6: Bayi 30 gün ödeme yok
    [AutomaticRetry(Attempts = 2)]
    public async Task CheckDealerPayments(CancellationToken ct = default)
    {
        _logger.LogInformation("AI Kural: Bayi ödeme kontrolü başlatıldı");

        var threshold = DateTime.UtcNow.AddDays(-30);
        var ruleId = await GetRuleIdAsync("Bayi 30 Gün Ödeme Yok", ct);

        var overdue = await _db.DealerReceivables
            .Include(r => r.Dealer)
            .Where(r => !r.IsSettled && r.DueDate < threshold)
            .AsNoTracking()
            .ToListAsync(ct);

        foreach (var receivable in overdue)
        {
            var dayCount = (int)(DateTime.UtcNow - receivable.DueDate).TotalDays;
            await CreateAlertAsync(ruleId, receivable.TenantId, receivable.Id,
                $"BAYİ BORCU: {receivable.Dealer.CompanyName} - {receivable.RemainingAmount:N2} TL - {dayCount} gün gecikmiş",
                AlertSeverity.Warning, ct);
        }
    }

    // Olay tetiklemeli: Fire maliyet artışı (AddServicePartHandler'dan çağrılır)
    public async Task NotifyFireCostIncrease(Guid tenantId, string recordNumber, string partName, decimal cost, CancellationToken ct = default)
    {
        var ruleId = await GetRuleIdAsync("Fire Maliyeti Artışı", ct);
        await CreateAlertAsync(ruleId, tenantId, null,
            $"MALİYET ARTIŞI: {recordNumber} - {partName} takılırken kırıldı. Ek maliyet: {cost:N2} TL",
            AlertSeverity.Warning, ct);
    }

    // Olay tetiklemeli: Stok negatife düştü
    public async Task NotifyNegativeStock(Guid tenantId, string itemName, CancellationToken ct = default)
    {
        var ruleId = await GetRuleIdAsync("Stok Negatife Düştü", ct);
        await CreateAlertAsync(ruleId, tenantId, null,
            $"KRİTİK: Stok negatife düştü - {itemName}",
            AlertSeverity.Critical, ct);
    }

    // Yardımcı: Kural ID'sini ada göre bul (yoksa 0 döner)
    private async Task<int> GetRuleIdAsync(string ruleName, CancellationToken ct)
    {
        var rule = await _db.AiRules
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == ruleName, ct);
        return rule?.Id ?? 0;
    }

    // Yardımcı: Alert oluştur (bugün aynı mesaj varsa atla)
    private async Task CreateAlertAsync(int ruleId, Guid tenantId, int? referenceId,
        string message, AlertSeverity severity, CancellationToken ct)
    {
        if (ruleId == 0) return; // Rule bulunamadı, uyarı oluşturma

        var today = DateTime.UtcNow.Date;
        var exists = await _db.AiAlerts.AnyAsync(a =>
            a.TenantId == tenantId &&
            a.Message == message &&
            a.CreatedAt >= today, ct);

        if (exists) return;

        _db.AiAlerts.Add(new AiAlert
        {
            RuleId = ruleId,
            Message = message,
            Severity = severity,
            IsResolved = false,
            ReferenceId = referenceId
        });

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("AI Alert [{Severity}]: {Message}", severity, message);
    }
}
