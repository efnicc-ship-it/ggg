namespace TeknikServis.Application.Common.Interfaces;

public interface ILabelPrintService
{
    Task<bool> PrintServiceLabelAsync(int serviceRecordId, CancellationToken cancellationToken = default);
    Task<bool> PrintAccessoryLabelAsync(int accessoryId, string accessoryType, CancellationToken cancellationToken = default);
    Task<bool> PrintDeviceInventoryLabelAsync(int deviceInventoryId, CancellationToken cancellationToken = default);
    Task<bool> PrintRawZplAsync(string zplContent, CancellationToken cancellationToken = default);
}
