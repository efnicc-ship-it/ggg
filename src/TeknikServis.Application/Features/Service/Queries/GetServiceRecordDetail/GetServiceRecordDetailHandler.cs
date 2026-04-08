using MediatR;
using Microsoft.EntityFrameworkCore;
using TeknikServis.Application.Common.Interfaces;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Application.Features.Service.Queries.GetServiceRecordDetail;

public class GetServiceRecordDetailHandler : IRequestHandler<GetServiceRecordDetailQuery, ServiceRecordDetailDto?>
{
    private readonly IApplicationDbContext _db;

    public GetServiceRecordDetailHandler(IApplicationDbContext db) => _db = db;

    public async Task<ServiceRecordDetailDto?> Handle(GetServiceRecordDetailQuery request, CancellationToken cancellationToken)
    {
        var record = await _db.ServiceRecords
            .Include(s => s.Customer)
            .Include(s => s.DeviceModel).ThenInclude(m => m.Brand)
            .Include(s => s.DeviceModelVariant)
            .Include(s => s.StatusHistories)
            .Include(s => s.Parts).ThenInclude(p => p.Part)
            .Include(s => s.Payments)
            .Include(s => s.Notes)
            .Include(s => s.Media)
            .Include(s => s.TimeLogs)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (record == null) return null;

        return new ServiceRecordDetailDto
        {
            Id = record.Id,
            RecordNumber = record.RecordNumber,
            CustomerId = record.CustomerId,
            CustomerName = $"{record.Customer.FirstName} {record.Customer.LastName}",
            CustomerPhone = record.Customer.Phone,
            CustomerEmail = record.Customer.Email,
            DeviceModelId = record.DeviceModelId,
            DeviceModelVariantId = record.DeviceModelVariantId,
            DeviceBrand = record.DeviceModel.Brand.Name,
            DeviceModel = record.DeviceModel.Name,
            DeviceVariant = record.DeviceModelVariant?.VariantName,
            Imei1 = record.Imei1,
            Imei2 = record.Imei2,
            SerialNumber = record.SerialNumber,
            DeviceColor = record.DeviceColor,
            FaultDescription = record.FaultDescription,
            InternalNotes = record.InternalNotes,
            Status = record.Status,
            StatusDisplay = GetStatusDisplay(record.Status),
            StatusColor = GetStatusColor(record.Status),
            AssignedTechnicianId = record.AssignedTechnicianId,
            QuotedPrice = record.QuotedPrice,
            FinalPrice = record.FinalPrice,
            PriceApproved = record.PriceApproved,
            PaymentStatus = record.PaymentStatus,
            TotalPaid = record.TotalPaid,
            PartsCost = record.PartsCost,
            LaborCost = record.LaborCost,
            NetProfit = record.NetProfit,
            IsUnderWarranty = record.IsUnderWarranty,
            CreatedAt = record.CreatedAt,
            DaysOpen = (int)(DateTime.UtcNow - record.CreatedAt).TotalDays,
            DeliveredAt = record.DeliveredAt,
            LabelPrinted = record.LabelPrinted,
            LiveStreamPlanned = record.LiveStreamPlanned,
            ApprovalToken = record.ApprovalToken,
            StatusHistory = record.StatusHistories
                .OrderByDescending(h => h.CreatedAt)
                .Select(h => new StatusHistoryDto(h.OldStatus, h.NewStatus, h.Note, h.CreatedAt))
                .ToList(),
            Parts = record.Parts
                .Select(p => new ServicePartDto(p.Id, p.Part.Name, p.Quantity, p.UnitCost, p.TotalCost, p.IsWrittenOff, p.IsReplacementOrder))
                .ToList(),
            Payments = record.Payments
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new ServicePaymentDto(p.Id, p.Amount, p.PaymentMethod, p.CreatedAt))
                .ToList(),
            Notes = record.Notes
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new ServiceNoteDto(n.Id, n.Content, n.IsInternal, n.CreatedAt, null))
                .ToList(),
            Media = record.Media
                .OrderBy(m => m.IsBefore).ThenBy(m => m.CreatedAt)
                .Select(m => new ServiceMediaDto(m.Id, m.FilePath, m.FileType, m.IsBefore, m.Caption, m.CreatedAt))
                .ToList(),
            TimeLogs = record.TimeLogs
                .OrderByDescending(t => t.StartedAt)
                .Select(t => new ServiceTimeLogDto(t.Id, "", t.StartedAt, t.EndedAt, t.DurationMinutes))
                .ToList()
        };
    }

    private static string GetStatusDisplay(ServiceStatus status) => status switch
    {
        ServiceStatus.Received => "Teslim Alındı",
        ServiceStatus.Diagnosing => "Arıza Tespiti",
        ServiceStatus.AwaitingParts => "Parça Bekleniyor",
        ServiceStatus.InRepair => "Onarımda",
        ServiceStatus.AwaitingApproval => "Onay Bekleniyor",
        ServiceStatus.Ready => "Hazır",
        ServiceStatus.Delivered => "Teslim Edildi",
        ServiceStatus.Cancelled => "İptal",
        ServiceStatus.Unrepairable => "Onarım Mümkün Değil",
        ServiceStatus.ReturnedUnrepaired => "İade Edildi",
        _ => status.ToString()
    };

    private static string GetStatusColor(ServiceStatus status) => status switch
    {
        ServiceStatus.Received => "blue",
        ServiceStatus.Diagnosing => "purple",
        ServiceStatus.AwaitingParts => "orange",
        ServiceStatus.InRepair => "yellow",
        ServiceStatus.AwaitingApproval => "amber",
        ServiceStatus.Ready => "green",
        ServiceStatus.Delivered => "gray",
        ServiceStatus.Cancelled => "red",
        ServiceStatus.Unrepairable => "rose",
        ServiceStatus.ReturnedUnrepaired => "slate",
        _ => "gray"
    };
}
