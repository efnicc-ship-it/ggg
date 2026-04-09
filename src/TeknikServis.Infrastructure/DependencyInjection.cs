using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeknikServis.Application.Common.Interfaces;
using TeknikServis.Domain.Entities.Identity;
using TeknikServis.Infrastructure.MultiTenancy;
using TeknikServis.Infrastructure.Persistence;
using TeknikServis.Infrastructure.Services.Encryption;
using TeknikServis.Infrastructure.Services.FileStorage;
using TeknikServis.Infrastructure.Services.Label;
using TeknikServis.Infrastructure.Services.Notifications;
using TeknikServis.Infrastructure.Services.Qr;

namespace TeknikServis.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString,
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(3);
                }));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // Identity
        services.AddIdentity<AppUser, AppRole>(options =>
        {
            options.Password.RequiredLength = 10;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireDigit = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.SignIn.RequireConfirmedAccount = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Hangfire
        services.AddHangfire(config =>
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                  .UseSimpleAssemblyNameTypeSerializer()
                  .UseRecommendedSerializerSettings()
                  .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                  {
                      CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                      SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                      QueuePollInterval = TimeSpan.Zero,
                      UseRecommendedIsolationLevel = true,
                      DisableGlobalLocks = true
                  }));

        services.AddHangfireServer();

        // Services
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IEncryptionService, AesEncryptionService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IQrCodeService, QrCodeService>();
        services.AddScoped<ILabelPrintService, ZplLabelService>();

        services.AddHttpClient<ISmsService, SmsService>();
        services.AddHttpClient<IWhatsAppService, WhatsAppService>();

        // Hangfire jobs (transient — Hangfire resolves per execution)
        services.AddTransient<Jobs.AiRuleEngineJob>();
        services.AddTransient<Jobs.MediaCleanupJob>();
        services.AddTransient<Jobs.LowStockAlertJob>();
        services.AddTransient<Jobs.SmartReminderJob>();

        return services;
    }

    /// <summary>Register recurring Hangfire jobs. Call from Program.cs after app is built.</summary>
    public static void RegisterRecurringJobs()
    {
        var manager = new RecurringJobManager();

        // Kural 1: Teknisyen 24/48 saat hareketsiz — Her saat
        manager.AddOrUpdate<Jobs.AiRuleEngineJob>(
            "ai-idle-technicians",
            job => job.CheckIdleTechnicians(CancellationToken.None),
            "0 * * * *");

        // Kural 2: Parça siparişi 4 saat — Her saat
        manager.AddOrUpdate<Jobs.AiRuleEngineJob>(
            "ai-unordered-parts",
            job => job.CheckUnorderedParts(CancellationToken.None),
            "0 * * * *");

        // Kural 6: Bayi 30 gün ödeme — Pazartesi 09:00
        manager.AddOrUpdate<Jobs.AiRuleEngineJob>(
            "ai-dealer-payments",
            job => job.CheckDealerPayments(CancellationToken.None),
            "0 9 * * MON");

        // Kural 7: Cihaz 60 gün satılmadı — Her gün 09:00
        manager.AddOrUpdate<Jobs.AiRuleEngineJob>(
            "ai-unsold-devices",
            job => job.CheckUnsoldDevices(CancellationToken.None),
            "0 9 * * *");

        // Medya temizleme — Her gün gece 03:00
        manager.AddOrUpdate<Jobs.MediaCleanupJob>(
            "media-cleanup",
            job => job.CleanOldMedia(CancellationToken.None),
            "0 3 * * *");

        // Düşük stok uyarısı — Her saat
        manager.AddOrUpdate<Jobs.LowStockAlertJob>(
            "low-stock-check",
            job => job.CheckLowStock(CancellationToken.None),
            "0 * * * *");

        // Hareketsiz servis kaydı — Her saat
        manager.AddOrUpdate<Jobs.SmartReminderJob>(
            "idle-service-check",
            job => job.CheckIdleServiceRecords(CancellationToken.None),
            "0 * * * *");

        // Memnuniyet anketi — Her 2 saatte bir
        manager.AddOrUpdate<Jobs.SmartReminderJob>(
            "survey-sender",
            job => job.SendSurveys(CancellationToken.None),
            "0 */2 * * *");
    }
}
