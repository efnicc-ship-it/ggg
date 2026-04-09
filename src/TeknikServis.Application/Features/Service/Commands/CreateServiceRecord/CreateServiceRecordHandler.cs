using MediatR;
using Microsoft.EntityFrameworkCore;
using TeknikServis.Application.Common.Exceptions;
using TeknikServis.Application.Common.Interfaces;
using TeknikServis.Domain.Entities.Service;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Application.Features.Service.Commands.CreateServiceRecord;

public class CreateServiceRecordHandler : IRequestHandler<CreateServiceRecordCommand, CreateServiceRecordResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IEncryptionService _encryption;

    public CreateServiceRecordHandler(IApplicationDbContext db, ICurrentUserService currentUser, IEncryptionService encryption)
    {
        _db = db;
        _currentUser = currentUser;
        _encryption = encryption;
    }

    public async Task<CreateServiceRecordResult> Handle(CreateServiceRecordCommand request, CancellationToken cancellationToken)
    {
        // Müşteri ve kara liste kontrolü
        var customer = await _db.Customers.FindAsync(new object[] { request.CustomerId }, cancellationToken)
            ?? throw new NotFoundException("Müşteri", request.CustomerId);

        var isBlacklisted = await _db.BlacklistedCustomers
            .AnyAsync(b => b.CustomerId == request.CustomerId && b.IsActive, cancellationToken);

        if (isBlacklisted)
            throw new ForbiddenException("Bu müşteri kara listede. Servis kaydı oluşturulamaz.");

        // IMEI kara liste kontrolü
        if (!string.IsNullOrWhiteSpace(request.Imei1))
        {
            var imeiBlacklisted = await _db.BlacklistedImeis
                .AnyAsync(b => b.Imei == request.Imei1 && b.IsActive, cancellationToken);
            if (imeiBlacklisted)
                throw new ForbiddenException($"IMEI {request.Imei1} kara listede. Yetkili birim bilgilendirildi.");
        }

        // Kayıt no üret: SRV-YYYY-NNNNNN
        var year = DateTime.UtcNow.Year;
        var count = await _db.ServiceRecords
            .Where(s => s.CreatedAt.Year == year)
            .CountAsync(cancellationToken);

        var recordNumber = $"SRV-{year}-{(count + 1):D6}";

        // PIN/şifre şifrele
        string? encryptedPassword = null;
        if (!string.IsNullOrEmpty(request.DevicePasswordEncrypted))
            encryptedPassword = _encryption.Encrypt(request.DevicePasswordEncrypted);

        var record = new ServiceRecord
        {
            RecordNumber = recordNumber,
            CustomerId = request.CustomerId,
            DeviceModelId = request.DeviceModelId,
            DeviceModelVariantId = request.DeviceModelVariantId,
            Imei1 = request.Imei1,
            Imei2 = request.Imei2,
            SerialNumber = request.SerialNumber,
            DeviceColor = request.DeviceColor,
            DevicePasswordEncrypted = encryptedPassword,
            FaultDescription = request.FaultDescription,
            InternalNotes = request.InternalNotes,
            AssignedTechnicianId = request.AssignedTechnicianId,
            BranchId = request.BranchId ?? _currentUser.BranchId,
            IsUnderWarranty = request.IsUnderWarranty,
            RelatedWarrantyRecordId = request.RelatedWarrantyRecordId,
            Status = ServiceStatus.Received,
            ApprovalToken = Guid.NewGuid().ToString("N"),
        };

        _db.ServiceRecords.Add(record);

        // İlk durum geçmişi
        _db.ServiceStatusHistories.Add(new ServiceStatusHistory
        {
            ServiceRecord = record,
            OldStatus = ServiceStatus.Received,
            NewStatus = ServiceStatus.Received,
            Note = "Kayıt oluşturuldu",
            ChangedBy = _currentUser.UserId ?? 0
        });

        await _db.SaveChangesAsync(cancellationToken);

        return new CreateServiceRecordResult(record.Id, record.RecordNumber);
    }
}
