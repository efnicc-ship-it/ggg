namespace TeknikServis.Domain.Entities.Identity;

/// <summary>
/// Granular permission constants for the entire system.
/// Used in [RequirePermission] attribute and UserPermission table.
/// </summary>
public static class Permissions
{
    public static class Service
    {
        public const string Create = "Service.Create";
        public const string Edit = "Service.Edit";
        public const string Delete = "Service.Delete";
        public const string ViewAll = "Service.ViewAll";
        public const string ViewOwn = "Service.ViewOwn";
        public const string UpdateStatus = "Service.UpdateStatus";
        public const string ManageMedia = "Service.ManageMedia";
        public const string ManageParts = "Service.ManageParts";
        public const string RecordPayment = "Service.RecordPayment";
        public const string PrintLabel = "Service.PrintLabel";
        public const string SendNotification = "Service.SendNotification";
        public const string ViewSensitiveData = "Service.ViewSensitiveData";
    }

    public static class Customer
    {
        public const string Create = "Customer.Create";
        public const string Edit = "Customer.Edit";
        public const string Delete = "Customer.Delete";
        public const string ViewAll = "Customer.ViewAll";
        public const string ViewSensitive = "Customer.ViewSensitive"; // TC kimlik, kimlik foto
        public const string Blacklist = "Customer.Blacklist";
        public const string ManageRating = "Customer.ManageRating";
    }

    public static class Purchase
    {
        public const string Create = "Purchase.Create";
        public const string Edit = "Purchase.Edit";
        public const string Delete = "Purchase.Delete";
        public const string ViewAll = "Purchase.ViewAll";
        public const string GenerateContract = "Purchase.GenerateContract";
        public const string PrintLabel = "Purchase.PrintLabel";
    }

    public static class Sale
    {
        public const string Create = "Sale.Create";
        public const string Edit = "Sale.Edit";
        public const string Delete = "Sale.Delete";
        public const string ViewAll = "Sale.ViewAll";
        public const string ApplyDiscount = "Sale.ApplyDiscount";
        public const string GenerateContract = "Sale.GenerateContract";
        public const string SendPaymentLink = "Sale.SendPaymentLink";
    }

    public static class Stock
    {
        public const string View = "Stock.View";
        public const string Adjust = "Stock.Adjust";
        public const string Transfer = "Stock.Transfer";
        public const string ManageParts = "Stock.ManageParts";
        public const string ManageAccessories = "Stock.ManageAccessories";
    }

    public static class Finance
    {
        public const string ViewReports = "Finance.ViewReports";
        public const string EditEntries = "Finance.EditEntries";
        public const string ViewAllBranches = "Finance.ViewAllBranches";
        public const string ManageReceivables = "Finance.ManageReceivables";
        public const string ManagePayables = "Finance.ManagePayables";
    }

    public static class Branch
    {
        public const string View = "Branch.View";
        public const string Manage = "Branch.Manage";
        public const string InitiateTransfer = "Branch.InitiateTransfer";
        public const string ApproveTransfer = "Branch.ApproveTransfer";
    }

    public static class Dealer
    {
        public const string View = "Dealer.View";
        public const string Manage = "Dealer.Manage";
        public const string ManageCreditLimit = "Dealer.ManageCreditLimit";
    }

    public static class Supplier
    {
        public const string View = "Supplier.View";
        public const string Manage = "Supplier.Manage";
        public const string CreateOrder = "Supplier.CreateOrder";
        public const string ReceiveOrder = "Supplier.ReceiveOrder";
        public const string RecordPayment = "Supplier.RecordPayment";
    }

    public static class Admin
    {
        public const string ManageUsers = "Admin.ManageUsers";
        public const string ManageRoles = "Admin.ManageRoles";
        public const string ManageBranches = "Admin.ManageBranches";
        public const string ManageSettings = "Admin.ManageSettings";
        public const string ManageAiRules = "Admin.ManageAiRules";
        public const string ViewAuditLog = "Admin.ViewAuditLog";
        public const string ManageTemplates = "Admin.ManageTemplates";
        public const string ManagePriceList = "Admin.ManagePriceList";
        public const string ManageCommonFaults = "Admin.ManageCommonFaults";
    }

    public static class Report
    {
        public const string ViewService = "Report.ViewService";
        public const string ViewSale = "Report.ViewSale";
        public const string ViewStock = "Report.ViewStock";
        public const string ViewFinance = "Report.ViewFinance";
        public const string ViewCustomer = "Report.ViewCustomer";
        public const string ExportData = "Report.ExportData";
    }
}
