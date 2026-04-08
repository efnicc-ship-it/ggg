using MediatR;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Application.Features.Service.Commands.UpdateServiceStatus;

public record UpdateServiceStatusCommand(
    int ServiceRecordId,
    ServiceStatus NewStatus,
    string? Note,
    int? AssignedTechnicianId = null
) : IRequest<Unit>;
