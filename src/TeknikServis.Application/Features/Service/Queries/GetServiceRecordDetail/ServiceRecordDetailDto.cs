using TeknikServis.Domain.Enums;

namespace TeknikServis.Application.Features.Service.Queries.GetServiceRecordDetail;

public class ServiceRecordDetailDto
{
    public int Id { get; set; }
    public string RecordNumber { get; set; } = string.Empty;

    // Müşteri
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }

    // Cihaz
    public int DeviceModelId { get; set; }
    public int? DeviceModelVariantId { get; set; }
    public string DeviceBrand { get; set; } = string.Empty;
    public string DeviceModel { get; set; } = string.Empty;
    public string? DeviceVariant { get; set; }
    public string? Imei1 { get; set; }
    public string? Imei2 { get; set; }
    public string? SerialNumber { get; set; }
    public string? DeviceColor { get; set; }

    // Arıza
    public string FaultDescription { get; set; } = string.Empty;
    public string? InternalNotes { get; set; }

    // Durum
    public ServiceStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;
    public int? AssignedTechnicianId { get; set; }
    public string? TechnicianName { get; set; }

    // Fiyat
    public decimal? QuotedPrice { get; set; }
    public decimal? FinalPrice { get; set; }
    public bool? PriceApproved { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public decimal TotalPaid { get; set; }

    // Maliyet
    public decimal PartsCost { get; set; }
    public decimal LaborCost { get; set; }
    public decimal NetProfit { get; set; }

    // Garanti
    public bool IsUnderWarranty { get; set; }

    // Tarihler
    public DateTime CreatedAt { get; set; }
    public int DaysOpen { get; set; }
    public DateTime? DeliveredAt { get; set; }

    // İkincil
    public bool LabelPrinted { get; set; }
    public bool LiveStreamPlanned { get; set; }
    public string? ApprovalToken { get; set; }

    // İlişkili listeler
    public List<StatusHistoryDto> StatusHistory { get; set; } = new();
    public List<ServicePartDto> Parts { get; set; } = new();
    public List<ServicePaymentDto> Payments { get; set; } = new();
    public List<ServiceNoteDto> Notes { get; set; } = new();
    public List<ServiceMediaDto> Media { get; set; } = new();
    public List<ServiceTimeLogDto> TimeLogs { get; set; } = new();
}

public record StatusHistoryDto(ServiceStatus OldStatus, ServiceStatus NewStatus, string? Note, DateTime ChangedAt);
public record ServicePartDto(int Id, string PartName, int Quantity, decimal UnitCost, decimal TotalCost, bool IsWrittenOff, bool IsReplacementOrder);
public record ServicePaymentDto(int Id, decimal Amount, string PaymentMethod, DateTime CreatedAt);
public record ServiceNoteDto(int Id, string Content, bool IsInternal, DateTime CreatedAt, string? AuthorName);
public record ServiceMediaDto(int Id, string FilePath, string FileType, bool IsBefore, string? Caption, DateTime CreatedAt);
public record ServiceTimeLogDto(int Id, string TechnicianName, DateTime StartedAt, DateTime? EndedAt, int? DurationMinutes);
