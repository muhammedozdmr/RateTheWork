namespace RateTheWork.Application.Common.Interfaces;

/// <summary>
/// Dosya yükleme ve saklama işlemleri için abstraction.
/// Azure Blob Storage veya AWS S3 gibi servislerle implemente edilir.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Dosya yükler ve URL döner
    /// </summary>
    /// <param name="fileStream">Dosya stream'i</param>
    /// <param name="fileName">Dosya adı</param>
    /// <param name="contentType">MIME type</param>
    /// <param name="folder">Klasör adı (örn: "documents", "avatars")</param>
    /// <returns>Yüklenen dosyanın URL'i</returns>
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string folder);
    
    /// <summary>
    /// Dosyayı siler
    /// </summary>
    /// <param name="fileUrl">Silinecek dosyanın URL'i</param>
    Task DeleteFileAsync(string fileUrl);
    
    /// <summary>
    /// Dosyanın var olup olmadığını kontrol eder
    /// </summary>
    /// <param name="fileUrl">Kontrol edilecek dosyanın URL'i</param>
    Task<bool> FileExistsAsync(string fileUrl);
    
    /// <summary>
    /// Dosyayı indirir
    /// </summary>
    /// <param name="fileUrl">İndirilecek dosyanın URL'i</param>
    /// <returns>Dosya içeriği</returns>
    Task<byte[]> DownloadFileAsync(string fileUrl);
}
