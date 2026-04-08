namespace TeknikServis.Application.Common.Interfaces;

public interface IWhatsAppService
{
    Task<bool> SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
    Task<bool> SendTemplateAsync(string phoneNumber, string templateName, Dictionary<string, string> parameters, CancellationToken cancellationToken = default);
}
