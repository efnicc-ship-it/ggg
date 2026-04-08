using MediatR;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Application.Features.Purchase.Queries.GetPurchaseRecords;

public record GetPurchaseRecordsQuery(
    int Page = 1,
    int PageSize = 25,
    string? Search = null,
    PurchaseType? PurchaseType = null,
    int? BranchId = null,
    DateTime? From = null,
    DateTime? To = null
) : IRequest<GetPurchaseRecordsResult>;

public class GetPurchaseRecordsResult
{
    public List<PurchaseRecordListDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class PurchaseRecordListDto
{
    public int Id { get; set; }
    public string RecordNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string DeviceBrand { get; set; } = string.Empty;
    public string DeviceModel { get; set; } = string.Empty;
    public string Imei1 { get; set; } = string.Empty;
    public PurchaseType PurchaseType { get; set; }
    public decimal PurchasePrice { get; set; }
    public bool ImeiChecked { get; set; }
    public bool AutoServiceCreated { get; set; }
    public int? AutoCreatedServiceRecordId { get; set; }
    public DateTime CreatedAt { get; set; }
}
