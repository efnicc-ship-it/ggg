using MediatR;

namespace TeknikServis.Application.Features.Service.Commands.CreateServiceRecord;

public record CreateServiceRecordCommand(
    int CustomerId,
    int DeviceModelId,
    int? DeviceModelVariantId,
    string? Imei1,
    string? Imei2,
    string? SerialNumber,
    string? DeviceColor,
    string? DevicePasswordEncrypted,
    string FaultDescription,
    string? InternalNotes,
    int? AssignedTechnicianId,
    int? BranchId,
    bool IsUnderWarranty,
    int? RelatedWarrantyRecordId
) : IRequest<CreateServiceRecordResult>;

public record CreateServiceRecordResult(int Id, string RecordNumber);
