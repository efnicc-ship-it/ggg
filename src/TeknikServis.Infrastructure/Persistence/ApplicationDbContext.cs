using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TeknikServis.Application.Common.Interfaces;
using TeknikServis.Domain.Entities.Appointment;
using TeknikServis.Domain.Entities.Audit;
using TeknikServis.Domain.Entities.Blacklist;
using TeknikServis.Domain.Entities.Branch;
using TeknikServis.Domain.Entities.Campaign;
using TeknikServis.Domain.Entities.CommonFault;
using TeknikServis.Domain.Entities.Customer;
using TeknikServis.Domain.Entities.Dealer;
using TeknikServis.Domain.Entities.Device;
using TeknikServis.Domain.Entities.EnsarAI;
using TeknikServis.Domain.Entities.Finance;
using TeknikServis.Domain.Entities.Identity;
using TeknikServis.Domain.Entities.InternalChat;
using TeknikServis.Domain.Entities.Notification;
using TeknikServis.Domain.Entities.PriceList;
using TeknikServis.Domain.Entities.Purchase;
using TeknikServis.Domain.Entities.Sale;
using TeknikServis.Domain.Entities.Service;
using TeknikServis.Domain.Entities.Stock;
using TeknikServis.Domain.Entities.Supplier;
using TeknikServis.Domain.Entities.Tenant;

namespace TeknikServis.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<AppUser, AppRole, int>, IApplicationDbContext
{
    private readonly ICurrentUserService _currentUserService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService) : base(options)
    {
        _currentUserService = currentUserService;
    }

    // Tenant
    public DbSet<Domain.Entities.Tenant.Tenant> Tenants => Set<Domain.Entities.Tenant.Tenant>();
    public DbSet<TenantModule> TenantModules => Set<TenantModule>();
    public DbSet<TenantSettings> TenantSettings => Set<TenantSettings>();

    // Identity
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<UserRoleAssignment> UserRoleAssignments => Set<UserRoleAssignment>();
    public DbSet<CustomRole> CustomRoles => Set<CustomRole>();

    // Branch
    public DbSet<Domain.Entities.Branch.Branch> Branches => Set<Domain.Entities.Branch.Branch>();
    public DbSet<BranchTransfer> BranchTransfers => Set<BranchTransfer>();
    public DbSet<ServiceBranchTransfer> ServiceBranchTransfers => Set<ServiceBranchTransfer>();

    // Customer
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerRating> CustomerRatings => Set<CustomerRating>();
    public DbSet<CustomerDocument> CustomerDocuments => Set<CustomerDocument>();
    public DbSet<CustomerSurveyResponse> CustomerSurveyResponses => Set<CustomerSurveyResponse>();

    // Device
    public DbSet<DeviceType> DeviceTypes => Set<DeviceType>();
    public DbSet<DeviceBrand> DeviceBrands => Set<DeviceBrand>();
    public DbSet<DeviceModel> DeviceModels => Set<DeviceModel>();
    public DbSet<DeviceModelVariant> DeviceModelVariants => Set<DeviceModelVariant>();
    public DbSet<DeviceCatalog> DeviceCatalogs => Set<DeviceCatalog>();
    public DbSet<DeviceInventory> DeviceInventories => Set<DeviceInventory>();
    public DbSet<IntakeChecklist> IntakeChecklists => Set<IntakeChecklist>();

    // Service
    public DbSet<ServiceRecord> ServiceRecords => Set<ServiceRecord>();
    public DbSet<ServiceStatusHistory> ServiceStatusHistories => Set<ServiceStatusHistory>();
    public DbSet<ServiceMedia> ServiceMedias => Set<ServiceMedia>();
    public DbSet<ServiceNote> ServiceNotes => Set<ServiceNote>();
    public DbSet<ServicePart> ServiceParts => Set<ServicePart>();
    public DbSet<ServicePayment> ServicePayments => Set<ServicePayment>();
    public DbSet<ServiceTimeLog> ServiceTimeLogs => Set<ServiceTimeLog>();
    public DbSet<LiveStreamSchedule> LiveStreamSchedules => Set<LiveStreamSchedule>();
    public DbSet<PriceApprovalLog> PriceApprovalLogs => Set<PriceApprovalLog>();

    // CommonFault
    public DbSet<CommonFaultType> CommonFaultTypes => Set<CommonFaultType>();
    public DbSet<CommonActionType> CommonActionTypes => Set<CommonActionType>();

    // PriceList
    public DbSet<RepairPriceList> RepairPriceLists => Set<RepairPriceList>();

    // Purchase
    public DbSet<PurchaseRecord> PurchaseRecords => Set<PurchaseRecord>();
    public DbSet<PurchaseDocument> PurchaseDocuments => Set<PurchaseDocument>();

    // Sale
    public DbSet<SaleRecord> SaleRecords => Set<SaleRecord>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<SalePayment> SalePayments => Set<SalePayment>();
    public DbSet<WarrantyRecord> WarrantyRecords => Set<WarrantyRecord>();
    public DbSet<DiscountRecord> DiscountRecords => Set<DiscountRecord>();
    public DbSet<OnlinePaymentRecord> OnlinePaymentRecords => Set<OnlinePaymentRecord>();

    // Campaign
    public DbSet<Campaign> Campaigns => Set<Campaign>();

    // Appointment
    public DbSet<AppointmentRecord> AppointmentRecords => Set<AppointmentRecord>();

    // Dealer
    public DbSet<Domain.Entities.Dealer.Dealer> Dealers => Set<Domain.Entities.Dealer.Dealer>();
    public DbSet<DealerDevice> DealerDevices => Set<DealerDevice>();

    // Supplier
    public DbSet<Domain.Entities.Supplier.Supplier> Suppliers => Set<Domain.Entities.Supplier.Supplier>();
    public DbSet<PartOrder> PartOrders => Set<PartOrder>();
    public DbSet<PartOrderItem> PartOrderItems => Set<PartOrderItem>();
    public DbSet<SupplierPartPrice> SupplierPartPrices => Set<SupplierPartPrice>();

    // Stock
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<PartCompatibility> PartCompatibilities => Set<PartCompatibility>();
    public DbSet<DeviceAccessory> DeviceAccessories => Set<DeviceAccessory>();
    public DbSet<DeviceCompanion> DeviceCompanions => Set<DeviceCompanion>();
    public DbSet<UniversalAccessory> UniversalAccessories => Set<UniversalAccessory>();
    public DbSet<StockItem> StockItems => Set<StockItem>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<StockWriteOff> StockWriteOffs => Set<StockWriteOff>();
    public DbSet<ScrapSaleRecord> ScrapSaleRecords => Set<ScrapSaleRecord>();

    // Finance
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<AccountTransaction> AccountTransactions => Set<AccountTransaction>();
    public DbSet<CustomerReceivable> CustomerReceivables => Set<CustomerReceivable>();
    public DbSet<DealerReceivable> DealerReceivables => Set<DealerReceivable>();
    public DbSet<SupplierPayable> SupplierPayables => Set<SupplierPayable>();

    // Notification
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();

    // Ensar AI
    public DbSet<AiRule> AiRules => Set<AiRule>();
    public DbSet<AiAlert> AiAlerts => Set<AiAlert>();
    public DbSet<EscalationPolicy> EscalationPolicies => Set<EscalationPolicy>();

    // Blacklist
    public DbSet<BlacklistedCustomer> BlacklistedCustomers => Set<BlacklistedCustomer>();
    public DbSet<BlacklistedImei> BlacklistedImeis => Set<BlacklistedImei>();

    // Audit
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // InternalChat
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationMember> ConversationMembers => Set<ConversationMember>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<MessageAttachment> MessageAttachments => Set<MessageAttachment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Rename Identity tables
        builder.Entity<AppUser>(b => b.ToTable("Users"));
        builder.Entity<AppRole>(b => b.ToTable("Roles"));
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<int>>(b => b.ToTable("UserClaims"));
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<int>>(b => b.ToTable("UserRoles"));
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<int>>(b => b.ToTable("UserLogins"));
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<int>>(b => b.ToTable("RoleClaims"));
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<int>>(b => b.ToTable("UserTokens"));

        // Global query filters — TenantId + SoftDelete + BranchId
        var tenantId = _currentUserService.TenantId;
        var branchId = _currentUserService.BranchId;
        var canViewAllBranches = _currentUserService.CanViewAllBranches;

        // Tenant-scoped entities with soft delete
        ApplyTenantFilters(builder, tenantId, branchId, canViewAllBranches);

        // Global decimal precision — decimal(18,4) for all money fields
        Configurations.ModelBuilderExtensions.ApplyGlobalDecimalPrecision(builder);
    }

    private static void ApplyTenantFilters(ModelBuilder builder, Guid tenantId, int? branchId, bool canViewAllBranches)
    {
        // ISoftDelete only (no TenantId)
        builder.Entity<AuditLog>().HasQueryFilter(x => x.TenantId == tenantId);

        // These entities use TenantId from BaseEntity
        // EF Core global query filters applied via configurations
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var userId = _currentUserService.UserId;
        var tenantId = _currentUserService.TenantId;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is Domain.Common.BaseEntity baseEntity)
            {
                if (entry.State == EntityState.Added)
                {
                    baseEntity.CreatedAt = now;
                    if (baseEntity.TenantId == Guid.Empty)
                        baseEntity.TenantId = tenantId;
                }
                else if (entry.State == EntityState.Modified)
                {
                    baseEntity.UpdatedAt = now;
                }
            }

            if (entry.Entity is Domain.Common.IAuditableEntity auditableEntity)
            {
                if (entry.State == EntityState.Added)
                    auditableEntity.CreatedBy = userId;
                else if (entry.State == EntityState.Modified)
                    auditableEntity.UpdatedBy = userId;
            }

            if (entry.Entity is Domain.Common.ISoftDelete softDeleteEntity && entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                softDeleteEntity.IsDeleted = true;
                softDeleteEntity.DeletedAt = now;
                softDeleteEntity.DeletedBy = userId;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
