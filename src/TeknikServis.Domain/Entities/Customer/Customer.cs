using TeknikServis.Domain.Common;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Domain.Entities.Customer;

public class Customer : BaseEntity, IAuditableEntity, ISoftDelete
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty; // unique per tenant
    public string? Phone2 { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public CommunicationPreference CommunicationPreference { get; set; } = CommunicationPreference.SMS;

    // KVKK — stored AES-256 encrypted
    public string? NationalIdEncrypted { get; set; }      // TC Kimlik No (encrypted)
    public string? IdCardPhotoPath { get; set; }           // Encrypted file path

    // OTP verification
    public bool PhoneVerified { get; set; }
    public bool EmailVerified { get; set; }

    // Blacklist
    public bool IsBlacklisted { get; set; }
    public string? BlacklistReason { get; set; }
    public DateTime? BlacklistedAt { get; set; }
    public int? BlacklistedBy { get; set; }

    // Rating
    public decimal AverageRating { get; set; }
    public int RatingCount { get; set; }

    // SMS/WA Opt-out (KVKK)
    public bool SmsOptOut { get; set; }
    public bool WhatsAppOptOut { get; set; }
    public bool EmailOptOut { get; set; }

    // Linked to dealer (if customer is also a dealer)
    public int? DealerId { get; set; }

    // IAuditableEntity
    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }

    public ICollection<CustomerRating> Ratings { get; set; } = new List<CustomerRating>();
    public ICollection<CustomerDocument> Documents { get; set; } = new List<CustomerDocument>();
}
