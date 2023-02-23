using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ChatService.Web.Dtos;
using ChatService.Web.Storage;

namespace ChatService.Web.Tests.Controllers;

public class ImagesControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly Mock<IFileStore> _fileStoreMock = new();
    private readonly HttpClient _httpClient;
    public ImagesControllerTests(WebApplicationFactory<Program> factory)
    {
        _httpClient = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services => { services.AddSingleton(_fileStoreMock.Object); });
        }).CreateClient();
    }

    [Fact]
    public async Task DownloadImage()
    {
        byte[] imageContent = File.ReadAllBytes("../../../test.jpg");
        MemoryStream stream = new MemoryStream(imageContent);
        _fileStoreMock.Setup(m => m.DownloadFile("abcdef")).ReturnsAsync(new BlobResponse(ImageId: "abcdef", ContentType: "image/jpeg", Content: stream));
        var response = await _httpClient.GetAsync($"/Images/abcdef");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task DownloadImage_NotFound()
    {
        _fileStoreMock.Setup(m => m.DownloadFile("abcdef")).ReturnsAsync(new BlobResponse(ImageId: "abcdef", ContentType: null , Content: null));
        var response = await _httpClient.GetAsync($"/Images/abcdef");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    
    
    [Fact]
    public async Task UploadImage()
    {
        byte[] imageContent = File.ReadAllBytes("../../../test.jpg");
        MemoryStream stream = new MemoryStream(imageContent);
        HttpContent fileStreamContent = new StreamContent(stream);
        fileStreamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = "file",
            FileName = "test"
        };
        using var formData = new MultipartFormDataContent();
        formData.Add(fileStreamContent);
        var response = await _httpClient.PostAsync("Images", formData);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
    
    
}