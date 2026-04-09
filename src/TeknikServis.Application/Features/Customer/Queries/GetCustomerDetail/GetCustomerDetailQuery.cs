using MediatR;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Application.Features.Customer.Queries.GetCustomerDetail;

public record GetCustomerDetailQuery(int Id) : IRequest<CustomerDetailDto?>;

public class CustomerDetailDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Phone { get; set; } = string.Empty;
    public string? Phone2 { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public CommunicationPreference CommunicationPreference { get; set; }
    public bool SmsOptOut { get; set; }
    public bool WhatsAppOptOut { get; set; }
    public bool EmailOptOut { get; set; }
    public bool PhoneVerified { get; set; }
    public bool EmailVerified { get; set; }
    public bool IsBlacklisted { get; set; }
    public string? BlacklistReason { get; set; }
    public decimal AverageRating { get; set; }
    public int RatingCount { get; set; }
    public bool IsDealer { get; set; }
    public DateTime CreatedAt { get; set; }

    // Service history
    public int TotalServiceCount { get; set; }
    public int OpenServiceCount { get; set; }
    public decimal TotalServiceRevenue { get; set; }
    public List<CustomerServiceSummaryDto> RecentServices { get; set; } = new();

    // Purchase history
    public int TotalPurchaseCount { get; set; }
    public decimal TotalPurchaseAmount { get; set; }

    // Sale history
    public int TotalSaleCount { get; set; }
    public decimal TotalSaleAmount { get; set; }
}

public class CustomerServiceSummaryDto
{
    public int Id { get; set; }
    public string RecordNumber { get; set; } = string.Empty;
    public string DeviceBrand { get; set; } = string.Empty;
    public string DeviceModel { get; set; } = string.Empty;
    public string FaultDescription { get; set; } = string.Empty;
    public ServiceStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;
    public decimal? FinalPrice { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
}
