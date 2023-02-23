using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ChatService.Web.Dtos;
using ChatService.Web.Storage;

namespace ChatService.Web.IntegrationTests;

public class CosmosProfileStoreTest : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly IProfileStore _store;

    private readonly Profile _profile = new(
        Username: Guid.NewGuid().ToString(),
        FirstName: "Foo",
        LastName: "Bar",
        ProfilePictureId: "abcdef"
    );
    
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _store.DeleteProfile(_profile.Username);
    }

    public CosmosProfileStoreTest(WebApplicationFactory<Program> factory)
    {
        _store = factory.Services.GetRequiredService<IProfileStore>();
    }
    
    [Fact]
    public async Task AddNewProfile()
    {
        await _store.UpsertProfile(_profile);
        Assert.Equal(_profile, await _store.GetProfile(_profile.Username));
    }

    [Fact]
    public async Task GetNonExistingProfile()
    {
        Assert.Equal(null, await _store.GetProfile(_profile.Username));
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
    public async Task AddNewProfile_InvalidArgs(string username, string firstName, string lastName, string profilePictureId)
    {
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await _store.UpsertProfile(new Profile(username, firstName, lastName, profilePictureId)); 
        });
    }
}