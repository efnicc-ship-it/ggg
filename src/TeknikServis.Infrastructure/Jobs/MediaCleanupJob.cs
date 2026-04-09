using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TeknikServis.Application.Common.Interfaces;

namespace TeknikServis.Infrastructure.Jobs;

/// <summary>
/// Cihaz resim/videolarını 2 yıl sonra siler.
/// Kimlik fotoğrafları ve sözleşmeler silinmez.
/// </summary>
public class MediaCleanupJob
{
    private readonly IApplicationDbContext _db;
    private readonly IFileStorageService _storage;
    private readonly ILogger<MediaCleanupJob> _logger;

    public MediaCleanupJob(IApplicationDbContext db, IFileStorageService storage, ILogger<MediaCleanupJob> logger)
    {
        _db = db;
        _storage = storage;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 1)]
    public async Task CleanOldMedia(CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddYears(-2);
        _logger.LogInformation("Medya temizleme başlatıldı. Kesim tarihi: {Cutoff}", cutoff);

        var oldMedia = await _db.ServiceMedias
            .Where(m => m.CreatedAt < cutoff && !m.IsDeleted)
            .ToListAsync(ct);

        int deleted = 0;
        foreach (var media in oldMedia)
        {
            try
            {
                if (!string.IsNullOrEmpty(media.FilePath))
                    await _storage.DeleteAsync(media.FilePath);

                media.IsDeleted = true;
                media.DeletedAt = DateTime.UtcNow;
                deleted++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Medya silinemedi: {FilePath}", media.FilePath);
            }
        }

        if (deleted > 0)
            await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Medya temizleme tamamlandı. Silinen: {Count}", deleted);
    }
}
