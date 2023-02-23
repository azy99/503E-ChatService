using ChatService.Web.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Web.Storage;

public interface IFileStore
{
    Task<UploadImageResponse?> UploadFile(UploadImageRequest file);
    Task<BlobResponse> DownloadFile(string fileId);
    Task DeleteFile(string fileId);
}