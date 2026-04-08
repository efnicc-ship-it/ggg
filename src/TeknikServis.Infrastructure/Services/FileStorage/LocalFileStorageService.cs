using TeknikServis.Application.Common.Interfaces;

namespace TeknikServis.Infrastructure.Services.FileStorage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IEncryptionService _encryptionService;
    private readonly string _basePath;

    public LocalFileStorageService(IEncryptionService encryptionService, Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        _encryptionService = encryptionService;
        _basePath = configuration["FileStorage:BasePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        var safeFileName = SanitizeFileName(fileName);
        var uniqueName = $"{Guid.NewGuid():N}_{safeFileName}";
        var folderPath = Path.Combine(_basePath, folder);
        Directory.CreateDirectory(folderPath);

        var filePath = Path.Combine(folderPath, uniqueName);
        await using var file = File.Create(filePath);
        await fileStream.CopyToAsync(file, cancellationToken);

        return Path.Combine(folder, uniqueName).Replace('\\', '/');
    }

    public async Task<string> UploadEncryptedAsync(Stream fileStream, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream, cancellationToken);
        var encryptedBytes = _encryptionService.EncryptBytes(memoryStream.ToArray());

        var safeFileName = SanitizeFileName(fileName) + ".enc";
        var uniqueName = $"{Guid.NewGuid():N}_{safeFileName}";
        var folderPath = Path.Combine(_basePath, "secure", folder);
        Directory.CreateDirectory(folderPath);

        var filePath = Path.Combine(folderPath, uniqueName);
        await File.WriteAllBytesAsync(filePath, encryptedBytes, cancellationToken);

        return Path.Combine("secure", folder, uniqueName).Replace('\\', '/');
    }

    public Task DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }

    public async Task<Stream> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
    }

    public async Task<Stream> GetEncryptedFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        var encryptedBytes = await File.ReadAllBytesAsync(fullPath, cancellationToken);
        var decryptedBytes = _encryptionService.DecryptBytes(encryptedBytes);
        return new MemoryStream(decryptedBytes);
    }

    public bool Exists(string filePath)
    {
        return File.Exists(Path.Combine(_basePath, filePath));
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(Path.GetFileNameWithoutExtension(fileName)
            .Where(c => !invalid.Contains(c))
            .Take(50))
            + Path.GetExtension(fileName).ToLowerInvariant();
    }
}
