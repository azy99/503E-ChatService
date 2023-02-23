using ChatService.Web.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Web.Storage;

public interface IFileStore
{
    Task UploadFile(UploadFileRequest fileRequest);
    Task<BlobResponse> DownloadFile(string fileId);
    Task DeleteFile(string fileId);
}