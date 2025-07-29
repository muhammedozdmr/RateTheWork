using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RateTheWork.Application.Common.Interfaces;

namespace RateTheWork.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private readonly string _baseUrl;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService
    (
        IConfiguration configuration
        , ILogger<LocalFileStorageService> logger
    )
    {
        _logger = logger;
        _basePath = configuration["Storage:LocalPath"] ??
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        _baseUrl = configuration["Storage:BaseUrl"] ?? "/uploads";

        // Ensure the base directory exists
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string folder)
    {
        try
        {
            var folderPath = Path.Combine(_basePath, folder);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var uniqueFileName = GenerateUniqueFileName(fileName);
            var filePath = Path.Combine(folderPath, uniqueFileName);

            using (var fileOutputStream = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileOutputStream);
            }

            var url = $"{_baseUrl}/{folder}/{uniqueFileName}";
            _logger.LogInformation("File uploaded successfully to local storage: {Url}", url);

            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to local storage");
            throw;
        }
    }

    public async Task DeleteFileAsync(string fileUrl)
    {
        try
        {
            var relativePath = fileUrl.Replace(_baseUrl, "").TrimStart('/');
            var filePath = Path.Combine(_basePath, relativePath);

            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
                _logger.LogInformation("File deleted successfully from local storage: {FilePath}", filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from local storage: {FileUrl}", fileUrl);
            throw;
        }
    }

    public async Task<bool> FileExistsAsync(string fileUrl)
    {
        try
        {
            var relativePath = fileUrl.Replace(_baseUrl, "").TrimStart('/');
            var filePath = Path.Combine(_basePath, relativePath);

            return await Task.FromResult(File.Exists(filePath));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence: {FileUrl}", fileUrl);
            return false;
        }
    }

    public async Task<byte[]> DownloadFileAsync(string fileUrl)
    {
        try
        {
            var relativePath = fileUrl.Replace(_baseUrl, "").TrimStart('/');
            var filePath = Path.Combine(_basePath, relativePath);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found", filePath);
            }

            return await File.ReadAllBytesAsync(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file from local storage: {FileUrl}", fileUrl);
            throw;
        }
    }

    private string GenerateUniqueFileName(string fileName)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
        var extension = Path.GetExtension(fileName);
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

        return $"{timestamp}_{uniqueId}_{nameWithoutExtension}{extension}";
    }
}
