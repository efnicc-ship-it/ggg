using MediatR;
using Microsoft.EntityFrameworkCore;
using TeknikServis.Application.Common.Interfaces;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Application.Features.Service.Queries.GetServiceRecords;

public class GetServiceRecordsHandler : IRequestHandler<GetServiceRecordsQuery, GetServiceRecordsResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetServiceRecordsHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<GetServiceRecordsResult> Handle(GetServiceRecordsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.ServiceRecords
            .Include(s => s.Customer)
            .Include(s => s.DeviceModel).ThenInclude(m => m.Brand)
            .AsQueryable();

        // Branch filter (non-managers see only their branch)
        if (!_currentUser.CanViewAllBranches && _currentUser.BranchId.HasValue)
            query = query.Where(s => s.BranchId == _currentUser.BranchId);
        else if (request.BranchId.HasValue)
            query = query.Where(s => s.BranchId == request.BranchId);

        if (request.Status.HasValue)
            query = query.Where(s => s.Status == request.Status);

        if (request.TechnicianId.HasValue)
            query = query.Where(s => s.AssignedTechnicianId == request.TechnicianId);

        if (request.From.HasValue)
            query = query.Where(s => s.CreatedAt >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(s => s.CreatedAt <= request.To.Value.AddDays(1));

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim().ToLower();
            query = query.Where(x =>
                x.RecordNumber.ToLower().Contains(s) ||
                x.Customer.FirstName.ToLower().Contains(s) ||
                x.Customer.LastName.ToLower().Contains(s) ||
                x.Customer.Phone.Contains(s) ||
                (x.Imei1 != null && x.Imei1.Contains(s)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new ServiceRecordListDto
            {
                Id = s.Id,
                RecordNumber = s.RecordNumber,
                CustomerName = s.Customer.FirstName + " " + s.Customer.LastName,
                CustomerPhone = s.Customer.Phone,
                DeviceBrand = s.DeviceModel.Brand.Name,
                DeviceModel = s.DeviceModel.Name,
                Status = s.Status,
                StatusDisplay = GetStatusDisplay(s.Status),
                StatusColor = GetStatusColor(s.Status),
                PaymentStatus = s.PaymentStatus,
                FinalPrice = s.FinalPrice,
                TotalPaid = s.TotalPaid,
                CreatedAt = s.CreatedAt,
                DaysOpen = (int)(DateTime.UtcNow - s.CreatedAt).TotalDays,
                LabelPrinted = s.LabelPrinted,
                BranchId = s.BranchId,
                IsWarrantyService = s.IsUnderWarranty
            })
            .ToListAsync(cancellationToken);

        return new GetServiceRecordsResult
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    private static string GetStatusDisplay(ServiceStatus status) => status switch
    {
        ServiceStatus.Received => "Teslim Alındı",
        ServiceStatus.Diagnosing => "Arıza Tespiti",
        ServiceStatus.AwaitingParts => "Parça Bekleniyor",
        ServiceStatus.InRepair => "Onarımda",
        ServiceStatus.AwaitingApproval => "Onay Bekleniyor",
        ServiceStatus.Ready => "Hazır",
        ServiceStatus.Delivered => "Teslim Edildi",
        ServiceStatus.Cancelled => "İptal",
        ServiceStatus.Unrepairable => "Onarım Mümkün Değil",
        ServiceStatus.ReturnedUnrepaired => "İade Edildi",
        _ => status.ToString()
    };

    private static string GetStatusColor(ServiceStatus status) => status switch
    {
        ServiceStatus.Received => "blue",
        ServiceStatus.Diagnosing => "purple",
        ServiceStatus.AwaitingParts => "orange",
        ServiceStatus.InRepair => "yellow",
        ServiceStatus.AwaitingApproval => "amber",
        ServiceStatus.Ready => "green",
        ServiceStatus.Delivered => "gray",
        ServiceStatus.Cancelled => "red",
        ServiceStatus.Unrepairable => "rose",
        ServiceStatus.ReturnedUnrepaired => "slate",
        _ => "gray"
    };
}
