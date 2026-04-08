namespace TeknikServis.Domain.Entities.Identity;

public class UserPermission
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Permission { get; set; } = string.Empty; // e.g. "Service.Create", "Finance.ViewReports"
    public bool IsGranted { get; set; } = true;
    public int? GrantedBy { get; set; }
    public DateTime GrantedAt { get; set; }

    public AppUser User { get; set; } = null!;
}
