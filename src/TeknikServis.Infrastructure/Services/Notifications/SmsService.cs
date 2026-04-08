using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TeknikServis.Application.Common.Interfaces;

namespace TeknikServis.Infrastructure.Services.Notifications;

public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;
    private readonly string _provider;
    private readonly string _apiUrl;
    private readonly string _username;
    private readonly string _password;
    private readonly string _sender;
    private readonly HttpClient _httpClient;

    public SmsService(IConfiguration configuration, ILogger<SmsService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
        _provider = configuration["Sms:Provider"] ?? "NetGSM";
        _apiUrl = configuration["Sms:ApiUrl"] ?? "";
        _username = configuration["Sms:Username"] ?? "";
        _password = configuration["Sms:Password"] ?? "";
        _sender = configuration["Sms:Sender"] ?? "TEKNIKSERVIS";
    }

    public async Task<bool> SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending SMS to {Phone} via {Provider}", phoneNumber, _provider);

            // Provider-agnostic HTTP call — configured by admin panel
            if (string.IsNullOrEmpty(_apiUrl))
            {
                _logger.LogWarning("SMS provider not configured. Skipping SMS to {Phone}", phoneNumber);
                return false;
            }

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("user", _username),
                new KeyValuePair<string, string>("password", _password),
                new KeyValuePair<string, string>("gsmno", phoneNumber.Replace("+", "").Replace(" ", "")),
                new KeyValuePair<string, string>("message", message),
                new KeyValuePair<string, string>("msgheader", _sender),
            });

            var response = await _httpClient.PostAsync(_apiUrl, content, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMS send failed to {Phone}", phoneNumber);
            return false;
        }
    }

    public async Task<bool> SendOtpAsync(string phoneNumber, string otp, CancellationToken cancellationToken = default)
    {
        var message = $"TeknikServis dogrulama kodunuz: {otp}. Bu kod 5 dakika gecerlidir.";
        return await SendAsync(phoneNumber, message, cancellationToken);
    }
}
