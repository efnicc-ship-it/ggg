using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TeknikServis.Infrastructure.Persistence.Configurations;

/// <summary>
/// Tüm decimal alanlar için global precision 18,4 — SQL truncation uyarısını giderir
/// </summary>
public static class ModelBuilderExtensions
{
    public static void ApplyGlobalDecimalPrecision(this ModelBuilder modelBuilder)
    {
        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            if (property.GetColumnType() == null)
            {
                property.SetColumnType("decimal(18,4)");
            }
        }
    }
}
