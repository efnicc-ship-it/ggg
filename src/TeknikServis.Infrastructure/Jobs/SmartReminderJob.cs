using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TeknikServis.Application.Common.Interfaces;
using TeknikServis.Domain.Entities.EnsarAI;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Infrastructure.Jobs;

/// <summary>
/// Akıllı hatırlatıcılar:
/// - Servis kaydı 24h+ hareketsiz → teknisyene uyarı
/// - Parça siparişi girilmemiş 4h+ → teknisyene uyarı
/// - Müşteri memnuniyet anketi (teslimden 24h sonra)
/// </summary>
public class SmartReminderJob
{
    private readonly IApplicationDbContext _db;
    private readonly IWhatsAppService _whatsApp;
    private readonly ISmsService _sms;
    private readonly ILogger<SmartReminderJob> _logger;

    public SmartReminderJob(
        IApplicationDbContext db,
        IWhatsAppService whatsApp,
        ISmsService sms,
        ILogger<SmartReminderJob> logger)
    {
        _db = db;
        _whatsApp = whatsApp;
        _sms = sms;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 2)]
    public async Task CheckIdleServiceRecords(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var threshold24h = now.AddHours(-24);
        var threshold48h = now.AddHours(-48);

        var activeStatuses = new[]
        {
            ServiceStatus.Received, ServiceStatus.Diagnosing,
            ServiceStatus.InRepair, ServiceStatus.AwaitingParts
        };

        var idleRecords = await _db.ServiceRecords
            .Where(s => activeStatuses.Contains(s.Status) && s.UpdatedAt < threshold24h)
            .Include(s => s.Customer)
            .ToListAsync(ct);

        foreach (var record in idleRecords)
        {
            var hours = (now - record.UpdatedAt).TotalHours;
            var severity = hours >= 48 ? AlertSeverity.Critical : AlertSeverity.Warning;
            var title = hours >= 48
                ? $"Servis {record.RecordNumber} — 48 saat hareketsiz"
                : $"Servis {record.RecordNumber} — 24 saat hareketsiz";

            // Aynı kayıt için son 4 saatte uyarı oluşturulduysa atla
            var exists = await _db.AiAlerts.AnyAsync(
                a => a.EntityId == record.Id && a.EntityType == "ServiceRecord" &&
                     a.CreatedAt > now.AddHours(-4) && !a.IsResolved, ct);
            if (exists) continue;

            _db.AiAlerts.Add(new AiAlert
            {
                Severity = severity,
                Title = title,
                Message = $"Servis kaydı {record.RecordNumber} ({record.Status}) {hours:F0} saattir güncellenmedi.",
                EntityType = "ServiceRecord",
                EntityId = record.Id,
                IsResolved = false
            });

            _logger.LogWarning("Hareketsiz servis: {RecordNumber} — {Hours}h", record.RecordNumber, (int)hours);
        }

        await _db.SaveChangesAsync(ct);
    }

    [AutomaticRetry(Attempts = 2)]
    public async Task SendSurveys(CancellationToken ct = default)
    {
        // Teslimden 24-26 saat sonra memnuniyet anketi gönder
        var from = DateTime.UtcNow.AddHours(-26);
        var to = DateTime.UtcNow.AddHours(-24);

        var delivered = await _db.ServiceRecords
            .Include(s => s.Customer)
            .Where(s => s.Status == ServiceStatus.Delivered &&
                        s.DeliveredAt.HasValue &&
                        s.DeliveredAt >= from &&
                        s.DeliveredAt <= to &&
                        s.SurveySent != true)
            .ToListAsync(ct);

        foreach (var record in delivered)
        {
            if (record.Customer == null) continue;
            if (record.Customer.SmsOptOut && record.Customer.WhatsAppOptOut) continue;

            var message = $"Sayın {record.Customer.FirstName}, {record.RecordNumber} numaralı cihazınıza verilen hizmetimizden memnun kaldınız mı? Değerlendirmeniz için: https://g.page/r/review";

            try
            {
                if (!record.Customer.WhatsAppOptOut &&
                    record.Customer.CommunicationPreference == Domain.Enums.CommunicationPreference.WhatsApp)
                {
                    await _whatsApp.SendAsync(record.Customer.Phone, message, ct);
                }
                else if (!record.Customer.SmsOptOut)
                {
                    await _sms.SendAsync(record.Customer.Phone, message, ct);
                }

                record.SurveySent = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Anket gönderilemedi: {Phone}", record.Customer.Phone);
            }
        }

        await _db.SaveChangesAsync(ct);
    }
}
