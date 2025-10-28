// RealEstate.Application/Interfaces/IStorageService.cs
namespace RealEstate.Application.Interfaces;

public interface IStorageService
{
    Task<string> UploadFileAsync(string fileName, byte[] fileData, string contentType);
    Task<byte[]> DownloadFileAsync(string fileUrl);
    Task DeleteFileAsync(string fileUrl);
    Task<bool> FileExistsAsync(string fileUrl);
}
