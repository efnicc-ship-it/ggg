using TeknikServis.Domain.Enums;

namespace TeknikServis.Application.Features.Service.Queries.GetServiceRecords;

public class ServiceRecordListDto
{
    public int Id { get; set; }
    public string RecordNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string DeviceBrand { get; set; } = string.Empty;
    public string DeviceModel { get; set; } = string.Empty;
    public string? TechnicianName { get; set; }
    public ServiceStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;   // Tailwind color class
    public PaymentStatus PaymentStatus { get; set; }
    public decimal? FinalPrice { get; set; }
    public decimal TotalPaid { get; set; }
    public DateTime CreatedAt { get; set; }
    public int DaysOpen { get; set; }
    public bool LabelPrinted { get; set; }
    public int? BranchId { get; set; }
    public bool IsWarrantyService { get; set; }
}
