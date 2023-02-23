using System.Net;
using System.Net.Mime;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ChatService.Web.Dtos;

namespace ChatService.Web.Storage;

public class BlobStore: IFileStore
{
    private readonly BlobContainerClient _blobContainerClient;

    public BlobStore(BlobContainerClient blobContainerClient)
    {
        _blobContainerClient = blobContainerClient;
    }

    public async Task<UploadImageResponse?> UploadFile(UploadImageRequest file)
    {
        if (file.File == null)
        {
            throw new ArgumentException($"Invalid file {file}", nameof(file));
        }

        try
        {
            String uniqueFileId = $"{Guid.NewGuid()}";
            BlobClient blobClient = _blobContainerClient.GetBlobClient(uniqueFileId);
            
            await using (Stream? data = file.File.OpenReadStream())
            {
                 await blobClient.UploadAsync(data);
                 return new UploadImageResponse(ImageId: uniqueFileId );
            }
        }
        catch (RequestFailedException exception)
        {
            
        }

        return null;

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
            throw exception;
        }
            
       
        

    }
    
}