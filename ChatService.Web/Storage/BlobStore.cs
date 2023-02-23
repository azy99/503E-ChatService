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
            BlobClient blobClient = _blobContainerClient.GetBlobClient(file.File.FileName);
            
            await using (Stream? data = file.File.OpenReadStream())
            {
                 await blobClient.UploadAsync(data);
                 return new UploadImageResponse(ImageId: file.File.FileName );
            }
        }
        catch (RequestFailedException exception)
        {
            
        }

        return null;

    }

    public async Task<Stream> DownloadFile(string fileId)
    {
        try
        {
            BlobClient blobClient = _blobContainerClient.GetBlobClient(fileId);
            if (await blobClient.ExistsAsync())
            {
                Response<BlobDownloadInfo> response = await blobClient.DownloadAsync();
                return response.Value.Content;
            }
        }
        catch (RequestFailedException exception)
        {
           
        }

        return null;
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