using MediatR;

namespace TeknikServis.Application.Features.Customer.Queries.GetCustomers;

public record GetCustomersQuery(
    int Page = 1,
    int PageSize = 25,
    string? Search = null,
    bool? IsBlacklisted = null
) : IRequest<GetCustomersResult>;

public class GetCustomersResult
{
    public List<CustomerListDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class CustomerListDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? City { get; set; }
    public bool IsBlacklisted { get; set; }
    public decimal AverageRating { get; set; }
    public int RatingCount { get; set; }
    public bool PhoneVerified { get; set; }
    public int TotalServiceCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
