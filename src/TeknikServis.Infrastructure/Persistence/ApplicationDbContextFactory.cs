using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using TeknikServis.Application.Common.Interfaces;

namespace TeknikServis.Infrastructure.Persistence;

/// <summary>
/// EF Core CLI design-time factory.
/// Sadece migration oluştururken kullanılır — runtime'da etkilisi yoktur.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // appsettings.json'dan bağlantı dizesini oku
        var config = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../TeknikServis.Web"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? "Server=(localdb)\\mssqllocaldb;Database=TeknikServisDb;Trusted_Connection=True;";

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options, new NullCurrentUserService());
    }

    // Design-time için boş ICurrentUserService implementasyonu
    private class NullCurrentUserService : ICurrentUserService
    {
        public int? UserId => null;
        public string? UserName => null;
        public Guid TenantId => Guid.Empty;
        public int? BranchId => null;
        public int? RegionId => null;
        public string? Role => null;
        public bool IsAuthenticated => false;
        public bool CanViewAllBranches => false;
        public bool HasPermission(string permission) => false;
    }
}
