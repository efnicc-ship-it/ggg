namespace TeknikServis.Application.Common.Interfaces;

public interface IQrCodeService
{
    byte[] GenerateQrCode(string content, int size = 200);
    string GenerateQrCodeBase64(string content, int size = 200);
}
