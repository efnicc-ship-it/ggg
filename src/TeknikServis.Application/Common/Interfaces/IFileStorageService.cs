namespace TeknikServis.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string folder, CancellationToken cancellationToken = default);
    Task<string> UploadEncryptedAsync(Stream fileStream, string fileName, string folder, CancellationToken cancellationToken = default);
    Task DeleteAsync(string filePath, CancellationToken cancellationToken = default);
    Task<Stream> GetFileAsync(string filePath, CancellationToken cancellationToken = default);
    Task<Stream> GetEncryptedFileAsync(string filePath, CancellationToken cancellationToken = default);
    bool Exists(string filePath);
}
