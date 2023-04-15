using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ChatService.Web.Dtos.Profile;

namespace ChatService.Web.Storage;

public class BlobStore: IFileStore
{
    private readonly BlobContainerClient _blobContainerClient;

    public BlobStore(BlobContainerClient blobContainerClient)
    {
        _blobContainerClient = blobContainerClient;
    }

    public async Task UploadFile(UploadFileRequest request)
    {
        if (request.ImageRequest == null || request.ImageRequest.File == null || request.UniqueFileId == null)
        {
            throw new ArgumentException($"Invalid file {request.ImageRequest}", nameof(request.ImageRequest));
        }
        
        BlobClient blobClient = _blobContainerClient.GetBlobClient(request.UniqueFileId);
        await using (Stream? data = request.ImageRequest.File.OpenReadStream())
        {
             await blobClient.UploadAsync(data);
        }
        
    }

    public async Task<BlobResponse> DownloadFile(string fileId)
    {
        try
        {
            BlobClient blobClient = _blobContainerClient.GetBlobClient(fileId);
            if (await blobClient.ExistsAsync())
            {
                Response<BlobDownloadInfo> response = await blobClient.DownloadAsync();
                return new BlobResponse(ImageId: fileId, ContentType: "image/jpeg",
                    Content: response.Value.Content);
            }
        }
        catch (RequestFailedException exception)
        {
            if (exception.Status == 404)
            {
                return new BlobResponse(ImageId: fileId, ContentType: null, Content: null);
            }

            throw;
        }

        return new BlobResponse(ImageId: fileId, ContentType: null, Content: null);
    }

    public async Task DeleteFile(string fileId)
    {
        BlobClient client = _blobContainerClient.GetBlobClient(fileId);
        try
        {
            await client.DeleteAsync();
        }
        catch (RequestFailedException exception)
        {
            if (exception.Status == 404)
            {
                return;
            }

            throw;
        }
        

    }
    
}