namespace TeknikServis.Application.Common.Interfaces;

public interface IPdfService
{
    Task<byte[]> GenerateServiceContractAsync(int serviceRecordId, CancellationToken cancellationToken = default);
    Task<byte[]> GeneratePurchaseContractAsync(int purchaseRecordId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateSaleContractAsync(int saleRecordId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateReportAsync(string reportType, object data, CancellationToken cancellationToken = default);
}
