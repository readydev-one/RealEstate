// RealEstate.Infrastructure/Services/StorageService.cs
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using RealEstate.Application.Interfaces;

namespace RealEstate.Infrastructure.Services;

public class StorageService : IStorageService
{
    private readonly StorageClient _storageClient;
    private readonly string _bucketName;

    public StorageService(IConfiguration configuration)
    {
        _storageClient = StorageClient.Create();
        _bucketName = configuration["GoogleCloud:StorageBucket"] ?? 
            throw new InvalidOperationException("Storage bucket not configured");
    }

    public async Task<string> UploadFileAsync(string fileName, byte[] fileData, string contentType)
    {
        var objectName = $"documents/{Guid.NewGuid()}/{fileName}";
        
        using var stream = new MemoryStream(fileData);
        await _storageClient.UploadObjectAsync(
            _bucketName,
            objectName,
            contentType,
            stream);

        return $"gs://{_bucketName}/{objectName}";
    }

    public async Task<byte[]> DownloadFileAsync(string fileUrl)
    {
        var objectName = ExtractObjectNameFromUrl(fileUrl);
        
        using var stream = new MemoryStream();
        await _storageClient.DownloadObjectAsync(_bucketName, objectName, stream);
        
        return stream.ToArray();
    }

    public async Task DeleteFileAsync(string fileUrl)
    {
        var objectName = ExtractObjectNameFromUrl(fileUrl);
        await _storageClient.DeleteObjectAsync(_bucketName, objectName);
    }

    public async Task<bool> FileExistsAsync(string fileUrl)
    {
        try
        {
            var objectName = ExtractObjectNameFromUrl(fileUrl);
            await _storageClient.GetObjectAsync(_bucketName, objectName);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string ExtractObjectNameFromUrl(string fileUrl)
    {
        var uri = new Uri(fileUrl);
        return uri.AbsolutePath.TrimStart('/');
    }
}