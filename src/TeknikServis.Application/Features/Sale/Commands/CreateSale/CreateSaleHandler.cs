using MediatR;
using Microsoft.EntityFrameworkCore;
using TeknikServis.Application.Common.Interfaces;
using TeknikServis.Domain.Entities.Sale;
using TeknikServis.Domain.Entities.Stock;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Application.Features.Sale.Commands.CreateSale;

public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, CreateSaleResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _user;

    public CreateSaleHandler(IApplicationDbContext db, ICurrentUserService user)
    {
        _db = db;
        _user = user;
    }

    public async Task<CreateSaleResult> Handle(CreateSaleCommand req, CancellationToken ct)
    {
        // Kayıt numarası üret: SAT-YYYY-XXXXXX
        var year = DateTime.UtcNow.Year;
        var count = await _db.SaleRecords.CountAsync(s => s.CreatedAt.Year == year, ct) + 1;
        var recordNumber = $"SAT-{year}-{count:D6}";

        // Toplam hesapla
        var subtotal = req.Items.Sum(i => i.Quantity * i.UnitPrice);
        var discount = req.DiscountAmount ?? 0;
        var total = subtotal - discount;

        var sale = new SaleRecord
        {
            RecordNumber = recordNumber,
            CustomerId = req.CustomerId,
            IsAccessorySaleOnly = req.IsAccessorySaleOnly,
            SubTotal = subtotal,
            DiscountAmount = discount,
            TotalAmount = total,
            PaymentStatus = req.PaymentStatus,
            PaidAmount = req.PaidAmount,
            Notes = req.Notes,
            WarrantyDays = req.WarrantyDays,
            WarrantyExpiresAt = req.WarrantyDays.HasValue
                ? DateTime.UtcNow.AddDays(req.WarrantyDays.Value)
                : null
        };

        _db.SaleRecords.Add(sale);
        await _db.SaveChangesAsync(ct); // Id oluşsun

        // Satır kalemleri
        foreach (var item in req.Items)
        {
            var saleItem = new SaleItem
            {
                SaleRecordId = sale.Id,
                DeviceInventoryId = item.DeviceInventoryId,
                StockItemId = item.StockItemId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.Quantity * item.UnitPrice,
                Description = item.Description
            };
            _db.SaleItems.Add(saleItem);

            // Cihaz envanterini "Satıldı" yap
            if (item.DeviceInventoryId.HasValue)
            {
                var inv = await _db.DeviceInventories.FindAsync(new object[] { item.DeviceInventoryId.Value }, ct);
                if (inv != null)
                {
                    inv.Status = InventoryStatus.Sold;
                    inv.LinkedSaleRecordId = sale.Id;
                }
            }

            // Stok düş (aksesuar/parça)
            if (item.StockItemId.HasValue)
            {
                var stockItem = await _db.StockItems.FindAsync(new object[] { item.StockItemId.Value }, ct);
                if (stockItem != null)
                {
                    stockItem.AvailableQuantity -= item.Quantity;
                    _db.StockMovements.Add(new StockMovement
                    {
                        StockItemId = stockItem.Id,
                        MovementType = "Sale",
                        Quantity = -item.Quantity,
                        ReferenceType = "SaleRecord",
                        ReferenceId = sale.Id,
                        Notes = $"Satış: {recordNumber}"
                    });
                }
            }
        }

        // Garanti kaydı (cihaz satışı ve garanti varsa)
        if (req.WarrantyDays.HasValue && req.WarrantyDays > 0)
        {
            var deviceItem = req.Items.FirstOrDefault(i => i.DeviceInventoryId.HasValue);
            if (deviceItem != null)
            {
                _db.WarrantyRecords.Add(new WarrantyRecord
                {
                    SaleRecordId = sale.Id,
                    CustomerId = req.CustomerId,
                    DeviceInventoryId = deviceItem.DeviceInventoryId,
                    WarrantyDays = req.WarrantyDays.Value,
                    StartsAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(req.WarrantyDays.Value),
                    Status = WarrantyStatus.Active
                });
            }
        }

        await _db.SaveChangesAsync(ct);

        return new CreateSaleResult
        {
            Id = sale.Id,
            RecordNumber = recordNumber,
            TotalAmount = total
        };
    }
}
