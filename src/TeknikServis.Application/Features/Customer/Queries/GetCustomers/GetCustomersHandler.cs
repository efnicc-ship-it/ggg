using MediatR;
using Microsoft.EntityFrameworkCore;
using TeknikServis.Application.Common.Interfaces;

namespace TeknikServis.Application.Features.Customer.Queries.GetCustomers;

public class GetCustomersHandler : IRequestHandler<GetCustomersQuery, GetCustomersResult>
{
    private readonly IApplicationDbContext _db;

    public GetCustomersHandler(IApplicationDbContext db) => _db = db;

    public async Task<GetCustomersResult> Handle(GetCustomersQuery req, CancellationToken ct)
    {
        var query = _db.Customers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(req.Search))
            query = query.Where(c =>
                c.FirstName.Contains(req.Search) ||
                c.LastName.Contains(req.Search) ||
                c.Phone.Contains(req.Search) ||
                (c.Email != null && c.Email.Contains(req.Search)));

        if (req.IsBlacklisted.HasValue)
            query = query.Where(c => c.IsBlacklisted == req.IsBlacklisted);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(c => new CustomerListDto
            {
                Id = c.Id,
                FullName = c.FirstName + " " + c.LastName,
                Phone = c.Phone,
                Email = c.Email,
                City = c.City,
                IsBlacklisted = c.IsBlacklisted,
                AverageRating = c.AverageRating,
                RatingCount = c.RatingCount,
                PhoneVerified = c.PhoneVerified,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(ct);

        return new GetCustomersResult
        {
            Items = items,
            TotalCount = total,
            Page = req.Page,
            PageSize = req.PageSize
        };
    }
}
