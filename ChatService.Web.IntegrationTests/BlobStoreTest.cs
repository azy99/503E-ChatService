using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ChatService.Web.Dtos;
using ChatService.Web.Storage;
using Microsoft.AspNetCore.Http;

namespace ChatService.Web.IntegrationTests;

public class BlobStoreTest : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly IFileStore _store;
    
    private UploadFileRequest _fileRequest;
    private BlobResponse _blobResponse;

    public Task InitializeAsync()
    {
        byte[] imageContent = File.ReadAllBytes("../../../test.jpg");
        MemoryStream memoryStream = new MemoryStream(imageContent);
        var fileName = "test.jpg";
        var contentType = "image/jpeg";
        var formFile = new FormFile(memoryStream, 0, imageContent.Length, null, fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
        
        _fileRequest = new(
            UniqueFileId: Guid.NewGuid().ToString(),
            ImageRequest: new UploadImageRequest(formFile)
        );
        
        _blobResponse =
            new BlobResponse(ImageId: _fileRequest.UniqueFileId, ContentType: "image/jpeg", Content: memoryStream);
        
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _store.DeleteFile(_fileRequest.UniqueFileId);
    }

    public BlobStoreTest(WebApplicationFactory<Program> factory)
    {
        _store = factory.Services.GetRequiredService<IFileStore>();
    }
    
    [Fact]
    public async Task UploadImage()
    {
        await _store.UploadFile(_fileRequest);
        var result = await _store.DownloadFile(_fileRequest.UniqueFileId);
        Assert.Equal(_blobResponse.ImageId, result.ImageId);
        Assert.Equal(_blobResponse.ContentType, result.ContentType);
    }
    
    [Fact]
    public async Task DownloadImage_NotFound()
    {
        var result = await _store.DownloadFile("abc");
        Assert.Null(result.Content);
        Assert.Null(result.ContentType);
        Assert.Equal("abc", result.ImageId);
    }

    [Theory]
    [InlineData(null,"abc")]
    [InlineData(null,null)]
    public async Task UploadImage_InvalidArgs(UploadImageRequest imageRequest, String uniqueFileId)
    {
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await _store.UploadFile(new UploadFileRequest(imageRequest, uniqueFileId));
        });
    }



}