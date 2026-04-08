using MediatR;
using Microsoft.EntityFrameworkCore;
using TeknikServis.Application.Common.Exceptions;
using TeknikServis.Application.Common.Interfaces;
using TeknikServis.Domain.Entities.Service;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Application.Features.Service.Commands.AddServicePart;

public class AddServicePartHandler : IRequestHandler<AddServicePartCommand, AddServicePartResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notifications;

    public AddServicePartHandler(IApplicationDbContext db, ICurrentUserService currentUser, INotificationService notifications)
    {
        _db = db;
        _currentUser = currentUser;
        _notifications = notifications;
    }

    public async Task<AddServicePartResult> Handle(AddServicePartCommand request, CancellationToken cancellationToken)
    {
        var serviceRecord = await _db.ServiceRecords
            .Include(s => s.Parts)
            .FirstOrDefaultAsync(s => s.Id == request.ServiceRecordId, cancellationToken)
            ?? throw new NotFoundException("ServiceRecord", request.ServiceRecordId);

        var part = await _db.Parts.FindAsync(new object[] { request.PartId }, cancellationToken)
            ?? throw new NotFoundException("Part", request.PartId);

        var totalCost = request.Quantity * request.UnitCost;

        var servicePart = new ServicePart
        {
            ServiceRecordId = request.ServiceRecordId,
            PartId = request.PartId,
            Quantity = request.Quantity,
            UnitCost = request.UnitCost,
            TotalCost = totalCost,
            Status = PartOrderStatus.Ordered,
            IsReplacementOrder = request.IsReplacementOrder,
            ReplacesServicePartId = request.ReplacesServicePartId
        };

        _db.ServiceParts.Add(servicePart);

        // Toplam parça maliyetini güncelle
        serviceRecord.PartsCost += totalCost;
        serviceRecord.NetProfit = (serviceRecord.FinalPrice ?? 0) - serviceRecord.PartsCost - serviceRecord.LaborCost;

        // Eğer fire nedeniyle yeniden sipariş ise — Ensar AI manager'a bildirecek
        if (request.IsReplacementOrder && request.ReplacesServicePartId.HasValue)
        {
            var note = new ServiceNote
            {
                ServiceRecordId = request.ServiceRecordId,
                Content = $"⚠ Parça fire: {part.Name} takılırken hasar gördü. Yenisi sipariş edildi. Ek maliyet: {totalCost:N2} TL",
                IsInternal = true
            };
            _db.ServiceNotes.Add(note);

            // Manager'a bildirim — Ensar AI rule da tetiklenecek ayrıca
            await _notifications.SendAsync("PartWriteOffCostIncrease", null, null, new Dictionary<string, string>
            {
                ["RecordNumber"] = serviceRecord.RecordNumber,
                ["PartName"] = part.Name,
                ["Cost"] = totalCost.ToString("N2")
            }, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);

        return new AddServicePartResult(servicePart.Id, serviceRecord.PartsCost);
    }
}
