using MediatR;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Application.Features.Sale.Commands.CreateSale;

public record CreateSaleCommand(
    int? CustomerId,
    bool IsAccessorySaleOnly,
    List<SaleItemInput> Items,
    PaymentStatus PaymentStatus,
    decimal PaidAmount,
    int? WarrantyDays,
    string? Notes,
    int? DiscountAmount
) : IRequest<CreateSaleResult>;

public record SaleItemInput(
    // Cihaz satışı
    int? DeviceInventoryId,
    // Stok satışı (aksesuar / parça)
    int? StockItemId,
    int Quantity,
    decimal UnitPrice,
    string Description
);

public class CreateSaleResult
{
    public int Id { get; set; }
    public string RecordNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}
