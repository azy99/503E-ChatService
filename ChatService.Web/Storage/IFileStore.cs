using ChatService.Web.Dtos.Profile;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Web.Storage;

public interface IFileStore
{
    Task UploadFile(UploadFileRequest fileRequest);
    Task<BlobResponse> DownloadFile(string fileId);
    Task DeleteFile(string fileId);
}