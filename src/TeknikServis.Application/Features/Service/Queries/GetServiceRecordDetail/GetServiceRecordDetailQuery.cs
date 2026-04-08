using MediatR;

namespace TeknikServis.Application.Features.Service.Queries.GetServiceRecordDetail;

public record GetServiceRecordDetailQuery(int Id) : IRequest<ServiceRecordDetailDto?>;
