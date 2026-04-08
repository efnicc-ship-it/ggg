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

        return services;
    }
}
