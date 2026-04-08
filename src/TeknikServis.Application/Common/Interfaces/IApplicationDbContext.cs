using Microsoft.EntityFrameworkCore;
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

namespace TeknikServis.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    // Tenant
    DbSet<Domain.Entities.Tenant.Tenant> Tenants { get; }
    DbSet<TenantModule> TenantModules { get; }
    DbSet<TenantSettings> TenantSettings { get; }

    // Identity
    DbSet<UserPermission> UserPermissions { get; }
    DbSet<UserRoleAssignment> UserRoleAssignments { get; }
    DbSet<CustomRole> CustomRoles { get; }

    // Branch
    DbSet<Domain.Entities.Branch.Branch> Branches { get; }
    DbSet<BranchTransfer> BranchTransfers { get; }
    DbSet<ServiceBranchTransfer> ServiceBranchTransfers { get; }

    // Customer
    DbSet<Customer> Customers { get; }
    DbSet<CustomerRating> CustomerRatings { get; }
    DbSet<CustomerDocument> CustomerDocuments { get; }
    DbSet<CustomerSurveyResponse> CustomerSurveyResponses { get; }

    // Device
    DbSet<DeviceType> DeviceTypes { get; }
    DbSet<DeviceBrand> DeviceBrands { get; }
    DbSet<DeviceModel> DeviceModels { get; }
    DbSet<DeviceModelVariant> DeviceModelVariants { get; }
    DbSet<DeviceCatalog> DeviceCatalogs { get; }
    DbSet<DeviceInventory> DeviceInventories { get; }
    DbSet<IntakeChecklist> IntakeChecklists { get; }

    // Service
    DbSet<ServiceRecord> ServiceRecords { get; }
    DbSet<ServiceStatusHistory> ServiceStatusHistories { get; }
    DbSet<ServiceMedia> ServiceMedias { get; }
    DbSet<ServiceNote> ServiceNotes { get; }
    DbSet<ServicePart> ServiceParts { get; }
    DbSet<ServicePayment> ServicePayments { get; }
    DbSet<ServiceTimeLog> ServiceTimeLogs { get; }
    DbSet<LiveStreamSchedule> LiveStreamSchedules { get; }
    DbSet<PriceApprovalLog> PriceApprovalLogs { get; }

    // CommonFault
    DbSet<CommonFaultType> CommonFaultTypes { get; }
    DbSet<CommonActionType> CommonActionTypes { get; }

    // PriceList
    DbSet<RepairPriceList> RepairPriceLists { get; }

    // Purchase
    DbSet<PurchaseRecord> PurchaseRecords { get; }
    DbSet<PurchaseDocument> PurchaseDocuments { get; }

    // Sale
    DbSet<SaleRecord> SaleRecords { get; }
    DbSet<SaleItem> SaleItems { get; }
    DbSet<SalePayment> SalePayments { get; }
    DbSet<WarrantyRecord> WarrantyRecords { get; }
    DbSet<DiscountRecord> DiscountRecords { get; }
    DbSet<OnlinePaymentRecord> OnlinePaymentRecords { get; }

    // Campaign
    DbSet<Campaign> Campaigns { get; }

    // Appointment
    DbSet<AppointmentRecord> AppointmentRecords { get; }

    // Dealer
    DbSet<Domain.Entities.Dealer.Dealer> Dealers { get; }
    DbSet<DealerDevice> DealerDevices { get; }

    // Supplier
    DbSet<Domain.Entities.Supplier.Supplier> Suppliers { get; }
    DbSet<PartOrder> PartOrders { get; }
    DbSet<PartOrderItem> PartOrderItems { get; }
    DbSet<SupplierPartPrice> SupplierPartPrices { get; }

    // Stock
    DbSet<Part> Parts { get; }
    DbSet<PartCompatibility> PartCompatibilities { get; }
    DbSet<DeviceAccessory> DeviceAccessories { get; }
    DbSet<DeviceCompanion> DeviceCompanions { get; }
    DbSet<UniversalAccessory> UniversalAccessories { get; }
    DbSet<StockItem> StockItems { get; }
    DbSet<StockMovement> StockMovements { get; }
    DbSet<StockWriteOff> StockWriteOffs { get; }

    // Sale (Scrap)
    DbSet<ScrapSaleRecord> ScrapSaleRecords { get; }

    // Finance
    DbSet<Account> Accounts { get; }
    DbSet<AccountTransaction> AccountTransactions { get; }
    DbSet<CustomerReceivable> CustomerReceivables { get; }
    DbSet<DealerReceivable> DealerReceivables { get; }
    DbSet<SupplierPayable> SupplierPayables { get; }

    // Notification
    DbSet<NotificationTemplate> NotificationTemplates { get; }
    DbSet<NotificationLog> NotificationLogs { get; }

    // Ensar AI
    DbSet<AiRule> AiRules { get; }
    DbSet<AiAlert> AiAlerts { get; }
    DbSet<EscalationPolicy> EscalationPolicies { get; }

    // Blacklist
    DbSet<BlacklistedCustomer> BlacklistedCustomers { get; }
    DbSet<BlacklistedImei> BlacklistedImeis { get; }

    // Audit
    DbSet<AuditLog> AuditLogs { get; }

    // InternalChat
    DbSet<Conversation> Conversations { get; }
    DbSet<ConversationMember> ConversationMembers { get; }
    DbSet<Message> Messages { get; }
    DbSet<MessageAttachment> MessageAttachments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
