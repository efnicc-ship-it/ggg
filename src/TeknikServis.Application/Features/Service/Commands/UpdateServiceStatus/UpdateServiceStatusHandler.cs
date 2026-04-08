using MediatR;
using Microsoft.EntityFrameworkCore;
using TeknikServis.Application.Common.Exceptions;
using TeknikServis.Application.Common.Interfaces;
using TeknikServis.Domain.Entities.Service;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Application.Features.Service.Commands.UpdateServiceStatus;

public class UpdateServiceStatusHandler : IRequestHandler<UpdateServiceStatusCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notifications;

    public UpdateServiceStatusHandler(IApplicationDbContext db, ICurrentUserService currentUser, INotificationService notifications)
    {
        _db = db;
        _currentUser = currentUser;
        _notifications = notifications;
    }

    public async Task<Unit> Handle(UpdateServiceStatusCommand request, CancellationToken cancellationToken)
    {
        var record = await _db.ServiceRecords
            .Include(s => s.Customer)
            .FirstOrDefaultAsync(s => s.Id == request.ServiceRecordId, cancellationToken)
            ?? throw new NotFoundException("ServiceRecord", request.ServiceRecordId);

        var oldStatus = record.Status;

        // Geçersiz durum geçişlerini engelle
        if (oldStatus == ServiceStatus.Delivered || oldStatus == ServiceStatus.Unrepairable)
            throw new ForbiddenException("Tamamlanmış veya iade edilmiş servis kaydının durumu değiştirilemez.");

        record.Status = request.NewStatus;

        if (request.AssignedTechnicianId.HasValue)
            record.AssignedTechnicianId = request.AssignedTechnicianId;

        if (request.NewStatus == ServiceStatus.Delivered)
            record.DeliveredAt = DateTime.UtcNow;

        // Maliyet hesapla
        if (request.NewStatus == ServiceStatus.Delivered || request.NewStatus == ServiceStatus.Ready)
            record.NetProfit = (record.FinalPrice ?? 0) - record.PartsCost - record.LaborCost;

        _db.ServiceStatusHistories.Add(new ServiceStatusHistory
        {
            ServiceRecordId = record.Id,
            OldStatus = oldStatus,
            NewStatus = request.NewStatus,
            Note = request.Note,
            ChangedBy = _currentUser.UserId ?? 0
        });

        await _db.SaveChangesAsync(cancellationToken);

        // Müşteri bildirim
        await SendStatusNotificationAsync(record, request.NewStatus, cancellationToken);

        return Unit.Value;
    }

    private async Task SendStatusNotificationAsync(ServiceRecord record, ServiceStatus newStatus, CancellationToken cancellationToken)
    {
        var eventType = newStatus switch
        {
            ServiceStatus.Ready => "ServiceReady",
            ServiceStatus.Delivered => "ServiceDelivered",
            ServiceStatus.AwaitingApproval => "PriceApproval",
            ServiceStatus.Unrepairable => "ServiceUnrepairable",
            ServiceStatus.ReturnedUnrepaired => "ServiceReturned",
            _ => null
        };

        if (eventType == null) return;

        await _notifications.SendAsync(eventType, record.CustomerId, null, new Dictionary<string, string>
        {
            ["RecordNumber"] = record.RecordNumber,
            ["CustomerName"] = $"{record.Customer.FirstName} {record.Customer.LastName}",
            ["ApprovalToken"] = record.ApprovalToken ?? "",
        }, cancellationToken);
    }
}
