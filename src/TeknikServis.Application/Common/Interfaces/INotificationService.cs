using TeknikServis.Domain.Enums;

namespace TeknikServis.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendAsync(string eventType, int? customerId, int? userId, Dictionary<string, string> parameters, CancellationToken cancellationToken = default);
    Task SendToChannelAsync(NotificationChannel channel, string recipient, string title, string body, CancellationToken cancellationToken = default);
}
