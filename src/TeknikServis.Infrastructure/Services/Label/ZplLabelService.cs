using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TeknikServis.Application.Common.Interfaces;
using TeknikServis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace TeknikServis.Infrastructure.Services.Label;

public class ZplLabelService : ILabelPrintService
{
    private readonly ApplicationDbContext _db;
    private readonly IQrCodeService _qrCodeService;
    private readonly ILogger<ZplLabelService> _logger;
    private readonly string _printerHost;
    private readonly int _printerPort;
    private readonly string _baseUrl;

    public ZplLabelService(
        ApplicationDbContext db,
        IQrCodeService qrCodeService,
        IConfiguration configuration,
        ILogger<ZplLabelService> logger)
    {
        _db = db;
        _qrCodeService = qrCodeService;
        _logger = logger;
        _printerHost = configuration["ThermalPrinter:Host"] ?? "127.0.0.1";
        _printerPort = int.Parse(configuration["ThermalPrinter:Port"] ?? "9100");
        _baseUrl = configuration["App:BaseUrl"] ?? "https://localhost";
    }

    public async Task<bool> PrintServiceLabelAsync(int serviceRecordId, CancellationToken cancellationToken = default)
    {
        var record = await _db.ServiceRecords
            .Include(x => x.Customer)
            .Include(x => x.DeviceModel).ThenInclude(x => x.Brand)
            .FirstOrDefaultAsync(x => x.Id == serviceRecordId, cancellationToken);

        if (record == null) return false;

        var qrUrl = $"{_baseUrl}/c/{record.ApprovalToken}";
        var zpl = GenerateServiceLabel(
            qrUrl,
            $"{record.Customer.FirstName} {record.Customer.LastName}",
            record.Customer.Phone,
            $"{record.DeviceModel.Brand.Name} {record.DeviceModel.Name}",
            record.RecordNumber);

        return await PrintRawZplAsync(zpl, cancellationToken);
    }

    public async Task<bool> PrintAccessoryLabelAsync(int accessoryId, string accessoryType, CancellationToken cancellationToken = default)
    {
        string productName, compatibleModel, slug;

        switch (accessoryType)
        {
            case "DeviceAccessory":
                var acc = await _db.DeviceAccessories
                    .Include(x => x.CompatibleDeviceModel).ThenInclude(x => x!.Brand)
                    .FirstOrDefaultAsync(x => x.Id == accessoryId, cancellationToken);
                if (acc == null) return false;
                productName = acc.Name;
                compatibleModel = acc.CompatibleDeviceModel != null
                    ? $"{acc.CompatibleDeviceModel.Brand.Name} {acc.CompatibleDeviceModel.Name}"
                    : "";
                slug = acc.StorefrontSlug ?? "";
                break;

            case "DeviceCompanion":
                var comp = await _db.DeviceCompanions
                    .FirstOrDefaultAsync(x => x.Id == accessoryId, cancellationToken);
                if (comp == null) return false;
                productName = comp.Name;
                compatibleModel = $"{comp.Brand} {comp.Model}";
                slug = comp.StorefrontSlug ?? "";
                break;

            case "UniversalAccessory":
                var uni = await _db.UniversalAccessories
                    .FirstOrDefaultAsync(x => x.Id == accessoryId, cancellationToken);
                if (uni == null) return false;
                productName = uni.Name;
                compatibleModel = "";
                slug = uni.StorefrontSlug ?? "";
                break;

            default:
                return false;
        }

        var qrUrl = $"{_baseUrl}/u/{slug}";
        var zpl = GenerateAccessoryLabel(qrUrl, productName, compatibleModel);
        return await PrintRawZplAsync(zpl, cancellationToken);
    }

    public async Task<bool> PrintDeviceInventoryLabelAsync(int deviceInventoryId, CancellationToken cancellationToken = default)
    {
        var inv = await _db.DeviceInventories
            .Include(x => x.Catalog).ThenInclude(x => x.Model).ThenInclude(x => x.Brand)
            .FirstOrDefaultAsync(x => x.Id == deviceInventoryId, cancellationToken);

        if (inv == null) return false;

        var qrUrl = $"{_baseUrl}/u/{inv.StorefrontSlug}";

        // Kalan garanti hesapla
        string? remainingWarranty = null;
        if (inv.OriginalPurchaseDate.HasValue && inv.OriginalWarrantyMonths.HasValue)
        {
            var expiryDate = inv.OriginalPurchaseDate.Value.AddMonths(inv.OriginalWarrantyMonths.Value);
            if (expiryDate > DateTime.Today)
                remainingWarranty = expiryDate.ToString("dd.MM.yyyy");
        }

        var zpl = GenerateDeviceInventoryLabel(
            qrUrl,
            $"{inv.Catalog.Model.Brand.Name} {inv.Catalog.Model.Name}",
            inv.BatteryHealth.HasValue ? $"%{inv.BatteryHealth}" : null,
            inv.Storage,
            remainingWarranty);

        return await PrintRawZplAsync(zpl, cancellationToken);
    }

    public async Task<bool> PrintRawZplAsync(string zplContent, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(_printerHost, _printerPort, cancellationToken);
            await using var stream = client.GetStream();
            var bytes = Encoding.UTF8.GetBytes(zplContent);
            await stream.WriteAsync(bytes, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ZPL print failed for host {Host}:{Port}", _printerHost, _printerPort);
            return false;
        }
    }

    private static string GenerateServiceLabel(string qrUrl, string customerName, string phone, string deviceModel, string recordNumber)
    {
        return $@"^XA
^FO20,20^BQN,2,4^FDMA,{qrUrl}^FS
^FO180,20^A0N,28,28^FD{customerName}^FS
^FO180,55^A0N,22,22^FD{phone}^FS
^FO180,85^A0N,22,22^FD{deviceModel}^FS
^FO180,115^A0N,22,22^FD{recordNumber}^FS
^FO20,200^GB560,3,3^FS
^XZ";
    }

    private static string GenerateAccessoryLabel(string qrUrl, string productName, string compatibleModel)
    {
        var compatible = string.IsNullOrEmpty(compatibleModel) ? "" : $"^FO180,55^A0N,22,22^FD{compatibleModel}^FS";
        return $@"^XA
^FO20,20^BQN,2,4^FDMA,{qrUrl}^FS
^FO180,20^A0N,28,28^FD{productName}^FS
{compatible}
^XZ";
    }

    private static string GenerateDeviceInventoryLabel(string qrUrl, string deviceModel, string? battery, string? storage, string? warrantyDate)
    {
        var batteryLine = battery != null ? $"^FO180,55^A0N,22,22^FDPil: {battery}^FS" : "";
        var storageLine = storage != null ? $"^FO180,85^A0N,22,22^FD{storage}^FS" : "";
        var warrantyLine = warrantyDate != null ? $"^FO180,115^A0N,22,22^FDGaranti: {warrantyDate}^FS" : "";

        return $@"^XA
^FO20,20^BQN,2,4^FDMA,{qrUrl}^FS
^FO180,20^A0N,28,28^FD{deviceModel}^FS
{batteryLine}
{storageLine}
{warrantyLine}
^XZ";
    }
}
