using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using TeknikServis.Application.Common.Interfaces;

namespace TeknikServis.Infrastructure.Services.Notifications;

public class WhatsAppService : IWhatsAppService
{
    private readonly ILogger<WhatsAppService> _logger;
    private readonly string _apiUrl;
    private readonly string _token;
    private readonly HttpClient _httpClient;

    public WhatsAppService(IConfiguration configuration, ILogger<WhatsAppService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
        _apiUrl = configuration["WhatsApp:ApiUrl"] ?? "";
        _token = configuration["WhatsApp:Token"] ?? "";
    }

    public async Task<bool> SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_apiUrl))
            {
                _logger.LogWarning("WhatsApp provider not configured. Skipping message to {Phone}", phoneNumber);
                return false;
            }

            var payload = new
            {
                messaging_product = "whatsapp",
                to = phoneNumber.Replace("+", "").Replace(" ", ""),
                type = "text",
                text = new { body = message }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            if (!string.IsNullOrEmpty(_token))
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

            var response = await _httpClient.PostAsync(_apiUrl, content, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WhatsApp send failed to {Phone}", phoneNumber);
            return false;
        }
    }

    public async Task<bool> SendTemplateAsync(string phoneNumber, string templateName, Dictionary<string, string> parameters, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_apiUrl))
                return false;

            var components = new[]
            {
                new
                {
                    type = "body",
                    parameters = parameters.Select(p => new { type = "text", text = p.Value }).ToArray()
                }
            };

            var payload = new
            {
                messaging_product = "whatsapp",
                to = phoneNumber.Replace("+", "").Replace(" ", ""),
                type = "template",
                template = new
                {
                    name = templateName,
                    language = new { code = "tr" },
                    components
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_apiUrl, content, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WhatsApp template send failed to {Phone}", phoneNumber);
            return false;
        }
    }
}
