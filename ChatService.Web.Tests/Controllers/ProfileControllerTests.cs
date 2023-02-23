using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using ChatService.Web.Dtos;
using ChatService.Web.Storage;

namespace ChatService.Web.Tests.Controllers;

public class ProfileControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly Mock<IProfileStore> _profileStoreMock = new();
    private readonly HttpClient _httpClient;
    
    public ProfileControllerTests(WebApplicationFactory<Program> factory)
    {
        _httpClient = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services => { services.AddSingleton(_profileStoreMock.Object); });
        }).CreateClient();
    }
    
    [Fact]
    public async Task GetProfile()
    {
        var profile = new Profile("foobar", "Foo", "Bar","5b0fa492-3271-4131-bb6b-519c263d6c7b");
        _profileStoreMock.Setup(m => m.GetProfile(profile.Username))
            .ReturnsAsync(profile);

        var response = await _httpClient.GetAsync($"/Profile/{profile.Username}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Equal(profile, JsonConvert.DeserializeObject<Profile>(json));
    }
    
    [Fact]
    public async Task GetProfile_NotFound()
    {
        _profileStoreMock.Setup(m => m.GetProfile("foobar"))
            .ReturnsAsync((Profile?)null);

        var response = await _httpClient.GetAsync($"/Profile/foobar");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task AddProfile()
    {
        var profile = new Profile("foobar", "Foo", "Bar","5b0fa492-3271-4131-bb6b-519c263d6c7b");
        var response = await _httpClient.PostAsync("/Profile",
            new StringContent(JsonConvert.SerializeObject(profile), Encoding.Default, "application/json"));
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("http://localhost/Profile/foobar", response.Headers.GetValues("Location").First());
        
        _profileStoreMock.Verify(mock => mock.UpsertProfile(profile), Times.Once);
    }
    
    [Fact]
    public async Task AddProfile_Conflict()
    {
        var profile = new Profile("foobar", "Foo", "Bar","5b0fa492-3271-4131-bb6b-519c263d6c7b" );
        _profileStoreMock.Setup(m => m.GetProfile(profile.Username))
            .ReturnsAsync(profile);

        var response = await _httpClient.PostAsync("/Profile",
            new StringContent(JsonConvert.SerializeObject(profile), Encoding.Default, "application/json"));
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        
        _profileStoreMock.Verify(m => m.UpsertProfile(profile), Times.Never);
    }
    
    [Theory]
    [InlineData(null, "Foo", "Bar","5b0fa492-3271-4131-bb6b-519c263d6c7b")]
    [InlineData("", "Foo", "Bar", "5b0fa492-3271-4131-bb6b-519c263d6c7b")]
    [InlineData(" ", "Foo", "Bar","5b0fa492-3271-4131-bb6b-519c263d6c7b")]
    [InlineData("foobar", null, "Bar","5b0fa492-3271-4131-bb6b-519c263d6c7b")]
    [InlineData("foobar", "", "Bar","5b0fa492-3271-4131-bb6b-519c263d6c7b")]
    [InlineData("foobar", "   ", "Bar","5b0fa492-3271-4131-bb6b-519c263d6c7b")]
    [InlineData("foobar", "Foo", "","5b0fa492-3271-4131-bb6b-519c263d6c7b")]
    [InlineData("foobar", "Foo", null,"5b0fa492-3271-4131-bb6b-519c263d6c7b")]
    [InlineData("foobar", "Foo", " ","5b0fa492-3271-4131-bb6b-519c263d6c7b")]
    [InlineData("foobar", "Foo", "Bar","")]
    [InlineData("foobar", "Foo", "Bar"," ")]
    [InlineData("foobar", "Foo", "Bar",null)]
    public async Task AddProfile_InvalidArgs(string username, string firstName, string lastName, string profilePictureId)
    {
        var profile = new Profile(username, firstName, lastName, profilePictureId);
        var response = await _httpClient.PostAsync("/Profile",
            new StringContent(JsonConvert.SerializeObject(profile), Encoding.Default, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        _profileStoreMock.Verify(mock => mock.UpsertProfile(profile), Times.Never);
    }
    
}