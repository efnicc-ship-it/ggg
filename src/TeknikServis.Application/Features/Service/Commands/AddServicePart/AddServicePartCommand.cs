using MediatR;

namespace TeknikServis.Application.Features.Service.Commands.AddServicePart;

public record AddServicePartCommand(
    int ServiceRecordId,
    int PartId,
    int Quantity,
    decimal UnitCost,
    bool IsReplacementOrder = false,
    int? ReplacesServicePartId = null
) : IRequest<AddServicePartResult>;

public record AddServicePartResult(int ServicePartId, decimal NewTotalPartsCost);
