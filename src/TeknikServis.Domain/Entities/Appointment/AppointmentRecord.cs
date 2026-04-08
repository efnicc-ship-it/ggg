using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Appointment;

public class AppointmentRecord : BaseEntity, IAuditableEntity
{
    public int? CustomerId { get; set; }              // Kayıtlı müşteri (opsiyonel)
    public int? BranchId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public DateTime AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; }
    public string Subject { get; set; } = string.Empty;  // "Ekran Tamiri", "Fiyat Alma"
    public string? Notes { get; set; }
    public string Status { get; set; } = "Pending";      // "Pending" | "Confirmed" | "Completed" | "Cancelled"
    public bool ReminderSent { get; set; } = false;
    public int? LinkedServiceRecordId { get; set; }

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public Customer.Customer? Customer { get; set; }
}
