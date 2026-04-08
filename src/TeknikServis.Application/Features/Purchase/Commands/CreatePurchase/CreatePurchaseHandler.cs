using MediatR;
using Microsoft.EntityFrameworkCore;
using TeknikServis.Application.Common.Exceptions;
using TeknikServis.Application.Common.Interfaces;
using TeknikServis.Domain.Entities.Device;
using TeknikServis.Domain.Entities.Purchase;
using TeknikServis.Domain.Entities.Service;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Application.Features.Purchase.Commands.CreatePurchase;

public class CreatePurchaseHandler : IRequestHandler<CreatePurchaseCommand, CreatePurchaseResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _user;

    public CreatePurchaseHandler(IApplicationDbContext db, ICurrentUserService user)
    {
        _db = db;
        _user = user;
    }

    public async Task<CreatePurchaseResult> Handle(CreatePurchaseCommand req, CancellationToken ct)
    {
        // IMEI kara liste kontrolü
        var blacklisted = await _db.BlacklistedImeis
            .AnyAsync(b => b.Imei == req.Imei1, ct);
        if (blacklisted)
            throw new InvalidOperationException("IMEI kara listede kayıtlı. Bu cihaz alınamaz.");

        // Kayıt numarası üret: ALI-YYYY-NNNNNN
        var year = DateTime.UtcNow.Year;
        var count = await _db.PurchaseRecords.CountAsync(ct);
        var recordNumber = $"ALI-{year}-{(count + 1):D6}";

        var purchase = new PurchaseRecord
        {
            RecordNumber = recordNumber,
            CustomerId = req.CustomerId,
            DeviceModelId = req.DeviceModelId,
            DeviceModelVariantId = req.DeviceModelVariantId,
            PurchaseType = req.PurchaseType,
            Imei1 = req.Imei1,
            Imei2 = req.Imei2,
            SerialNumber = req.SerialNumber,
            Ram = req.Ram,
            Storage = req.Storage,
            Color = req.Color,
            BatteryHealth = req.BatteryHealth,
            PurchasePrice = req.PurchasePrice,
            PhysicalConditionNotes = req.PhysicalConditionNotes,
            RepairDescription = req.RepairDescription,
            RepairCostEstimate = req.RepairCostEstimate,
            BranchId = req.BranchId ?? _user.BranchId
        };

        int? autoServiceId = null;

        // Arızalı alış → otomatik servis kaydı oluştur
        if (req.PurchaseType == PurchaseType.Damaged)
        {
            // Şirketin kendi müşteri ID'sini tenant ayarlarından oku
            var ownCustomerSetting = await _db.TenantSettings
                .FirstOrDefaultAsync(t => t.Key == "OwnCustomerId", ct);

            int ownCustomerId = int.TryParse(ownCustomerSetting?.Value, out var cid)
                ? cid
                : req.CustomerId;

            var serviceCount = await _db.ServiceRecords.CountAsync(ct);
            var serviceNum = $"SRV-{year}-{(serviceCount + 1):D6}";

            var serviceRecord = new ServiceRecord
            {
                RecordNumber = serviceNum,
                CustomerId = ownCustomerId,
                DeviceModelId = req.DeviceModelId,
                DeviceModelVariantId = req.DeviceModelVariantId,
                Imei1 = req.Imei1,
                Imei2 = req.Imei2,
                SerialNumber = req.SerialNumber,
                DeviceColor = req.Color,
                FaultDescription = req.RepairDescription ?? "Arızalı satın alınan cihaz",
                InternalNotes = $"Alış kaydından otomatik oluşturuldu. Alış No: {recordNumber}",
                Status = ServiceStatus.Received,
                BranchId = req.BranchId ?? _user.BranchId,
                ApprovalToken = Guid.NewGuid().ToString("N"),
                PaymentStatus = PaymentStatus.Unpaid
            };

            _db.ServiceRecords.Add(serviceRecord);
            await _db.SaveChangesAsync(ct);

            purchase.AutoCreatedServiceRecordId = serviceRecord.Id;
            purchase.AutoServiceCreated = true;
            autoServiceId = serviceRecord.Id;
        }

        _db.PurchaseRecords.Add(purchase);
        await _db.SaveChangesAsync(ct);

        // Hurda değilse → DeviceInventory'ye ekle (kataloğu bul veya oluştur)
        if (req.PurchaseType != PurchaseType.Scrap)
        {
            // DeviceCatalog: ModelId + VariantId kombinasyonunu bul
            var catalog = await _db.DeviceCatalogs
                .FirstOrDefaultAsync(c =>
                    c.ModelId == req.DeviceModelId &&
                    c.VariantId == req.DeviceModelVariantId, ct);

            if (catalog == null)
            {
                catalog = new DeviceCatalog
                {
                    ModelId = req.DeviceModelId,
                    VariantId = req.DeviceModelVariantId,
                    IsActive = true
                };
                _db.DeviceCatalogs.Add(catalog);
                await _db.SaveChangesAsync(ct);
            }

            var inventory = new DeviceInventory
            {
                CatalogId = catalog.Id,
                BranchId = req.BranchId ?? _user.BranchId,
                Imei1 = req.Imei1,
                Imei2 = req.Imei2,
                SerialNumber = req.SerialNumber,
                Color = req.Color,
                Ram = req.Ram,
                Storage = req.Storage,
                BatteryHealth = req.BatteryHealth,
                PurchasePrice = req.PurchasePrice,
                LinkedPurchaseRecordId = purchase.Id,
                Source = InventorySource.CustomerPurchase,
                Status = req.PurchaseType == PurchaseType.Damaged
                    ? InventoryStatus.UnderRepair
                    : InventoryStatus.Available,
                Condition = DeviceCondition.SecondHand,
                PurchasedAt = DateTime.UtcNow,
                PurchasedFromCustomerId = req.CustomerId
            };

            _db.DeviceInventories.Add(inventory);
            await _db.SaveChangesAsync(ct);

            purchase.DeviceInventoryId = inventory.Id;
            await _db.SaveChangesAsync(ct);
        }

        return new CreatePurchaseResult(purchase.Id, purchase.RecordNumber, autoServiceId);
    }
}
