using System.Net;
using ChatService.Web.Dtos;
using ChatService.Web.Storage.Entities;
using Microsoft.Azure.Cosmos;

namespace ChatService.Web.Storage;

public class CosmosProfileStore : IProfileStore
{
    private readonly CosmosClient _cosmosClient;

    public CosmosProfileStore(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    private Container Container => _cosmosClient.GetDatabase("profiles").GetContainer("sharedContainer");

    public async Task UpsertProfile(Profile profile)
    {
        if (profile == null ||
            string.IsNullOrWhiteSpace(profile.Username) ||
            string.IsNullOrWhiteSpace(profile.FirstName) ||
            string.IsNullOrWhiteSpace(profile.LastName) ||
            string.IsNullOrEmpty(profile.ProfilePictureId)
           )
        {
            throw new ArgumentException($"Invalid profile {profile}", nameof(profile));
        }

        await Container.UpsertItemAsync(ToEntity(profile));
    }
    
    public async Task<Profile?> GetProfile(string username)
    {
        try
        {
            var entity = await Container.ReadItemAsync<ProfileEntity>(
                id: username,
                partitionKey: new PartitionKey(username),
                new ItemRequestOptions
                {
                    ConsistencyLevel = ConsistencyLevel.Session
                }
            );
            return ToProfile(entity);
        }
        catch (CosmosException e)
        {
            if (e.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            throw;
        }
    }
    
    public async Task DeleteProfile(string username)
    {
        try
        {
            await Container.DeleteItemAsync<Profile>(
                id: username,
                partitionKey: new PartitionKey(username)
            );
        }
        catch (CosmosException e)
        {
            if (e.StatusCode == HttpStatusCode.NotFound)
            {
                return;
            }

            throw;
        }
        //TODO delete associated image
    }
    
    private static ProfileEntity ToEntity(Profile profile)
    {
        return new ProfileEntity(
            partitionKey: profile.Username,
            id: profile.Username,
            profile.FirstName,
            profile.LastName,
            profile.ProfilePictureId
        );
    }
    
    private static Profile ToProfile(ProfileEntity entity)
    {
        return new Profile(
            Username: entity.id,
            entity.firstName,
            entity.lastName,
            entity.profilePictureId
        );
    }
}