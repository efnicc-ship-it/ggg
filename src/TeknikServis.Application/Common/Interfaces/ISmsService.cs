namespace TeknikServis.Application.Common.Interfaces;

public interface ISmsService
{
    Task<bool> SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
    Task<bool> SendOtpAsync(string phoneNumber, string otp, CancellationToken cancellationToken = default);
}
