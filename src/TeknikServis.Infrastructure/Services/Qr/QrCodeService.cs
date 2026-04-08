using QRCoder;
using TeknikServis.Application.Common.Interfaces;

namespace TeknikServis.Infrastructure.Services.Qr;

public class QrCodeService : IQrCodeService
{
    public byte[] GenerateQrCode(string content, int size = 200)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrData);
        return qrCode.GetGraphic(5);
    }

    public string GenerateQrCodeBase64(string content, int size = 200)
    {
        var bytes = GenerateQrCode(content, size);
        return $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
    }
}
