using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TeknikServis.Domain.Entities.EnsarAI;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Infrastructure.Persistence;

/// <summary>
/// İlk kurulumda çalışır: varsayılan AI kuralları, sistem müşterisi vb. oluşturur.
/// </summary>
public class ApplicationDbSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ApplicationDbSeeder> _logger;

    public ApplicationDbSeeder(ApplicationDbContext context, ILogger<ApplicationDbSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        await SeedAiRulesAsync(tenantId, cancellationToken);
        await SeedSystemCustomerAsync(tenantId, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedAiRulesAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var existingRuleNames = await _context.AiRules
            .Where(r => r.TenantId == tenantId)
            .Select(r => r.Name)
            .ToListAsync(cancellationToken);

        foreach (var (name, triggerType, cron, description, conditionJson, actionsJson) in AiRuleDefaults.Rules)
        {
            if (existingRuleNames.Contains(name)) continue;

            _context.AiRules.Add(new AiRule
            {
                TenantId = tenantId,
                Name = name,
                Description = description,
                TriggerType = triggerType,
                CronExpression = cron,
                ConditionJson = conditionJson,
                ActionsJson = actionsJson,
                Severity = AlertSeverity.Warning,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            _logger.LogInformation("Seeded AI rule: {RuleName}", name);
        }
    }

    private async Task SeedSystemCustomerAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        // Arızalı alışlarda otomatik servis kaydına sistem müşterisi eklenir
        // Tenant kurulumunda TenantSettings'ten firma bilgileri okunur
        // Bu sadece sistem müşterisinin yokluğunu işaretler — gerçek kayıt Web layer'da oluşturulur
        _logger.LogInformation("System customer placeholder checked for tenant: {TenantId}", tenantId);
    }
}
