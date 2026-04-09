using MediatR;
using Microsoft.EntityFrameworkCore;
using TeknikServis.Application.Common.Interfaces;

namespace TeknikServis.Application.Features.Purchase.Queries.GetPurchaseRecords;

public class GetPurchaseRecordsHandler : IRequestHandler<GetPurchaseRecordsQuery, GetPurchaseRecordsResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _user;

    public GetPurchaseRecordsHandler(IApplicationDbContext db, ICurrentUserService user)
    {
        _db = db;
        _user = user;
    }

    public async Task<GetPurchaseRecordsResult> Handle(GetPurchaseRecordsQuery req, CancellationToken ct)
    {
        var query = _db.PurchaseRecords
            .Include(p => p.Customer)
            .Include(p => p.DeviceModel).ThenInclude(m => m.Brand)
            .AsNoTracking();

        if (!_user.CanViewAllBranches && _user.BranchId.HasValue)
            query = query.Where(p => p.BranchId == _user.BranchId);

        if (!string.IsNullOrWhiteSpace(req.Search))
            query = query.Where(p =>
                p.RecordNumber.Contains(req.Search) ||
                p.Imei1.Contains(req.Search) ||
                (p.Customer.FirstName + " " + p.Customer.LastName).Contains(req.Search));

        if (req.PurchaseType.HasValue)
            query = query.Where(p => p.PurchaseType == req.PurchaseType);

        if (req.BranchId.HasValue)
            query = query.Where(p => p.BranchId == req.BranchId);

        if (req.From.HasValue)
            query = query.Where(p => p.CreatedAt >= req.From.Value);

        if (req.To.HasValue)
            query = query.Where(p => p.CreatedAt <= req.To.Value.AddDays(1));

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(p => new PurchaseRecordListDto
            {
                Id = p.Id,
                RecordNumber = p.RecordNumber,
                CustomerName = p.Customer.FirstName + " " + p.Customer.LastName,
                DeviceBrand = p.DeviceModel.Brand.Name,
                DeviceModel = p.DeviceModel.Name,
                Imei1 = p.Imei1,
                PurchaseType = p.PurchaseType,
                PurchasePrice = p.PurchasePrice,
                ImeiChecked = p.ImeiChecked,
                AutoServiceCreated = p.AutoServiceCreated,
                AutoCreatedServiceRecordId = p.AutoCreatedServiceRecordId,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(ct);

        return new GetPurchaseRecordsResult
        {
            Items = items,
            TotalCount = total,
            Page = req.Page,
            PageSize = req.PageSize
        };
    }
}
