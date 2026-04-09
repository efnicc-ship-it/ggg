using MediatR;
using Microsoft.EntityFrameworkCore;
using TeknikServis.Application.Common.Interfaces;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Application.Features.Customer.Queries.GetCustomerDetail;

public class GetCustomerDetailHandler : IRequestHandler<GetCustomerDetailQuery, CustomerDetailDto?>
{
    private readonly IApplicationDbContext _db;

    public GetCustomerDetailHandler(IApplicationDbContext db) => _db = db;

    public async Task<CustomerDetailDto?> Handle(GetCustomerDetailQuery req, CancellationToken ct)
    {
        var customer = await _db.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == req.Id, ct);

        if (customer == null) return null;

        // Service geçmişi
        var services = await _db.ServiceRecords
            .AsNoTracking()
            .Where(s => s.CustomerId == req.Id)
            .Include(s => s.DeviceModel).ThenInclude(m => m.Brand)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);

        var openStatuses = new[] { ServiceStatus.Received, ServiceStatus.Diagnosing, ServiceStatus.AwaitingParts, ServiceStatus.InRepair, ServiceStatus.AwaitingApproval, ServiceStatus.Ready };

        var serviceRevenue = await _db.ServicePayments
            .AsNoTracking()
            .Where(p => _db.ServiceRecords.Any(s => s.Id == p.ServiceRecordId && s.CustomerId == req.Id))
            .SumAsync(p => (decimal?)p.Amount, ct) ?? 0;

        // Purchase geçmişi
        var purchases = await _db.PurchaseRecords
            .AsNoTracking()
            .Where(p => p.CustomerId == req.Id)
            .ToListAsync(ct);

        // Sale geçmişi
        var sales = await _db.SaleRecords
            .AsNoTracking()
            .Where(s => s.CustomerId == req.Id)
            .ToListAsync(ct);

        var recentServices = services.Take(10).Select(s =>
        {
            var (color, display) = GetStatusDisplay(s.Status);
            return new CustomerServiceSummaryDto
            {
                Id = s.Id,
                RecordNumber = s.RecordNumber,
                DeviceBrand = s.DeviceModel?.Brand?.Name ?? "-",
                DeviceModel = s.DeviceModel?.Name ?? "-",
                FaultDescription = s.FaultDescription.Length > 60 ? s.FaultDescription[..60] + "…" : s.FaultDescription,
                Status = s.Status,
                StatusDisplay = display,
                StatusColor = color,
                FinalPrice = s.FinalPrice,
                PaymentStatus = s.PaymentStatus,
                CreatedAt = s.CreatedAt,
                DeliveredAt = s.DeliveredAt
            };
        }).ToList();

        return new CustomerDetailDto
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Phone = customer.Phone,
            Phone2 = customer.Phone2,
            Email = customer.Email,
            Address = customer.Address,
            City = customer.City,
            CommunicationPreference = customer.CommunicationPreference,
            SmsOptOut = customer.SmsOptOut,
            WhatsAppOptOut = customer.WhatsAppOptOut,
            EmailOptOut = customer.EmailOptOut,
            PhoneVerified = customer.PhoneVerified,
            EmailVerified = customer.EmailVerified,
            IsBlacklisted = customer.IsBlacklisted,
            BlacklistReason = customer.BlacklistReason,
            AverageRating = customer.AverageRating,
            RatingCount = customer.RatingCount,
            IsDealer = customer.IsDealer,
            CreatedAt = customer.CreatedAt,
            TotalServiceCount = services.Count,
            OpenServiceCount = services.Count(s => openStatuses.Contains(s.Status)),
            TotalServiceRevenue = serviceRevenue,
            RecentServices = recentServices,
            TotalPurchaseCount = purchases.Count,
            TotalPurchaseAmount = purchases.Sum(p => p.PurchasePrice),
            TotalSaleCount = sales.Count,
            TotalSaleAmount = sales.Sum(s => s.TotalAmount)
        };
    }

    private static (string color, string display) GetStatusDisplay(ServiceStatus status) => status switch
    {
        ServiceStatus.Received         => ("bg-blue-100 text-blue-700 dark:bg-blue-900/40 dark:text-blue-400", "Teslim Alındı"),
        ServiceStatus.Diagnosing       => ("bg-purple-100 text-purple-700 dark:bg-purple-900/40 dark:text-purple-400", "Teşhis"),
        ServiceStatus.AwaitingParts    => ("bg-yellow-100 text-yellow-700 dark:bg-yellow-900/40 dark:text-yellow-400", "Parça Bekleniyor"),
        ServiceStatus.InRepair         => ("bg-orange-100 text-orange-700 dark:bg-orange-900/40 dark:text-orange-400", "Onarımda"),
        ServiceStatus.AwaitingApproval => ("bg-indigo-100 text-indigo-700 dark:bg-indigo-900/40 dark:text-indigo-400", "Onay Bekleniyor"),
        ServiceStatus.Ready            => ("bg-green-100 text-green-700 dark:bg-green-900/40 dark:text-green-400", "Hazır"),
        ServiceStatus.Delivered        => ("bg-gray-100 text-gray-600 dark:bg-gray-700 dark:text-gray-300", "Teslim Edildi"),
        ServiceStatus.Cancelled        => ("bg-red-100 text-red-700 dark:bg-red-900/40 dark:text-red-400", "İptal"),
        ServiceStatus.Unrepairable     => ("bg-red-100 text-red-700 dark:bg-red-900/40 dark:text-red-400", "Onarılamaz"),
        ServiceStatus.ReturnedUnrepaired => ("bg-gray-100 text-gray-600 dark:bg-gray-700 dark:text-gray-300", "Onarılmadan İade"),
        _ => ("bg-gray-100 text-gray-600", status.ToString())
    };
}
