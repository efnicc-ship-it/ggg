using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TeknikServis.Application.Common.Interfaces;
using TeknikServis.Domain.Entities.EnsarAI;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Infrastructure.Jobs;

/// <summary>
/// Stok threshold altına düşen parçaları tespit eder ve AI uyarısı oluşturur.
/// </summary>
public class LowStockAlertJob
{
    private readonly IApplicationDbContext _db;
    private readonly ILogger<LowStockAlertJob> _logger;

    public LowStockAlertJob(IApplicationDbContext db, ILogger<LowStockAlertJob> logger)
    {
        _db = db;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 2)]
    public async Task CheckLowStock(CancellationToken ct = default)
    {
        _logger.LogInformation("Düşük stok kontrolü başlatıldı.");

        var lowStockItems = await _db.StockItems
            .Include(s => s.Part)
            .Where(s => s.Part != null &&
                        s.Part.LowStockThreshold > 0 &&
                        s.AvailableQuantity <= s.Part.LowStockThreshold)
            .ToListAsync(ct);

        foreach (var item in lowStockItems)
        {
            // Aynı parça için son 24 saatte uyarı oluşturulduysa atla
            var recentAlert = await _db.AiAlerts
                .AnyAsync(a => a.Message.Contains(item.Part!.Name) &&
                               a.CreatedAt > DateTime.UtcNow.AddHours(-24) &&
                               !a.IsResolved, ct);

            if (recentAlert) continue;

            var alert = new AiAlert
            {
                RuleId = 0, // Sistem kuralı
                Severity = AlertSeverity.Warning,
                Title = "Kritik Stok Uyarısı",
                Message = $"'{item.Part!.Name}' stoku kritik seviyede: {item.AvailableQuantity} adet kaldı (eşik: {item.Part.LowStockThreshold})",
                EntityType = "StockItem",
                EntityId = item.Id,
                IsResolved = false
            };

            _db.AiAlerts.Add(alert);
            _logger.LogWarning("Düşük stok: {PartName} — {Qty} adet", item.Part!.Name, item.AvailableQuantity);
        }

        if (lowStockItems.Count > 0)
            await _db.SaveChangesAsync(ct);
    }
}
