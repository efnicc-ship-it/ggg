using MediatR;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Application.Features.Purchase.Commands.CreatePurchase;

public record CreatePurchaseCommand(
    int CustomerId,
    int DeviceModelId,
    int? DeviceModelVariantId,
    PurchaseType PurchaseType,
    string Imei1,
    string? Imei2,
    string? SerialNumber,
    string? Ram,
    string? Storage,
    string? Color,
    int? BatteryHealth,
    decimal PurchasePrice,
    string? PhysicalConditionNotes,
    string? RepairDescription,
    decimal? RepairCostEstimate,
    int? BranchId
) : IRequest<CreatePurchaseResult>;

public record CreatePurchaseResult(int Id, string RecordNumber, int? AutoServiceRecordId);
