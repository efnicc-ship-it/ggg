using MediatR;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Application.Features.Service.Queries.GetServiceRecords;

public record GetServiceRecordsQuery(
    int Page = 1,
    int PageSize = 25,
    string? Search = null,
    ServiceStatus? Status = null,
    int? BranchId = null,
    int? TechnicianId = null,
    DateTime? From = null,
    DateTime? To = null
) : IRequest<GetServiceRecordsResult>;

public class GetServiceRecordsResult
{
    public List<ServiceRecordListDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
