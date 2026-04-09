using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TeknikServis.Application.Common.Interfaces;
using TeknikServis.Domain.Entities.Customer;
using TeknikServis.Domain.Entities.Notification;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Infrastructure.Services.Notifications;

/// <summary>
/// Bildirim servisi: Müşteri iletişimi (SMS/WhatsApp) + dahili bildirimler.
/// eventType'a göre NotificationTemplate lookup'ı yapar, parametreleri şablona enjekte eder.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _db;
    private readonly ISmsService _sms;
    private readonly IWhatsAppService _whatsApp;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IApplicationDbContext db,
        ISmsService sms,
        IWhatsAppService whatsApp,
        ILogger<NotificationService> logger)
    {
        _db = db;
        _sms = sms;
        _whatsApp = whatsApp;
        _logger = logger;
    }

    public async Task SendAsync(
        string eventType,
        int? customerId,
        int? userId,
        Dictionary<string, string> parameters,
        CancellationToken cancellationToken = default)
    {
        // Şablonu bul
        var template = await _db.NotificationTemplates
            .FirstOrDefaultAsync(t => t.EventType == eventType && t.IsActive, cancellationToken);

        if (template == null)
        {
            _logger.LogDebug("Bildirim şablonu bulunamadı: {EventType}", eventType);
            return;
        }

        // Şablona parametreleri enjekte et
        var body = ApplyParameters(template.Body, parameters);
        var title = ApplyParameters(template.Title, parameters);

        // Müşteri iletişimi
        if (customerId.HasValue)
        {
            var customer = await _db.Customers.FindAsync(new object[] { customerId.Value }, cancellationToken);
            if (customer != null && !string.IsNullOrEmpty(customer.Phone))
            {
                try
                {
                    bool sent = false;
                    var channelIsWhatsApp = template.Channel == NotificationChannel.WhatsApp;
                    var channelIsSms = template.Channel == NotificationChannel.SMS;

                    if (!customer.WhatsAppOptOut &&
                        customer.CommunicationPreference == CommunicationPreference.WhatsApp &&
                        channelIsWhatsApp)
                    {
                        sent = await _whatsApp.SendAsync(customer.Phone, body, cancellationToken);
                    }
                    else if (!customer.SmsOptOut && channelIsSms)
                    {
                        sent = await _sms.SendAsync(customer.Phone, body, cancellationToken);
                    }

                    if (sent)
                    {
                        _db.NotificationLogs.Add(new NotificationLog
                        {
                            RecipientPhone = customer.Phone,
                            RecipientCustomerId = customerId,
                            Title = title,
                            Body = body,
                            Channel = customer.CommunicationPreference == CommunicationPreference.WhatsApp
                                ? NotificationChannel.WhatsApp
                                : NotificationChannel.SMS,
                            IsSuccess = true,
                            SentAt = DateTime.UtcNow
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Bildirim gönderilemedi: {EventType} → {Phone}", eventType, customer.Phone);
                }
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task SendToChannelAsync(
        NotificationChannel channel,
        string recipient,
        string title,
        string body,
        CancellationToken cancellationToken = default)
    {
        try
        {
            bool sent = channel switch
            {
                NotificationChannel.SMS => await _sms.SendAsync(recipient, body, cancellationToken),
                NotificationChannel.WhatsApp => await _whatsApp.SendAsync(recipient, body, cancellationToken),
                _ => false
            };

            _logger.LogInformation("Kanal bildirimi: {Channel} → {Recipient} — {Success}", channel, recipient, sent);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Kanal bildirimi başarısız: {Channel} → {Recipient}", channel, recipient);
        }
    }

    private static string ApplyParameters(string template, Dictionary<string, string> parameters)
    {
        foreach (var (key, value) in parameters)
            template = template.Replace($"{{{{{key}}}}}", value);
        return template;
    }
}
