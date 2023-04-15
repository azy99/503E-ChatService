using ChatService.Web.Configuration;
using ChatService.Web.Dtos;
using ChatService.Web.Exceptions;
using ChatService.Web.Storage.Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChatService.Web.Storage
{
    public class CosmosConversationStore : IConversationStore
    {
        private readonly CosmosClient _cosmosClient;
        public CosmosConversationStore(IOptions<CosmosSettings> options)
        {
            _cosmosClient = new CosmosClient(options.Value.ConnectionString);
        }

        private Container Container => _cosmosClient.GetDatabase("profiles").GetContainer("sharedContainer");

        public async Task<StartConversationResponse> AddConversation(StartConversationRequest request)
        {
            var entity = ToEntity(request);
            try
            {
                ValidateConversation(request);
                await Container.CreateItemAsync(entity);
            }
            catch (CosmosException e)
            {
                if (e.StatusCode == HttpStatusCode.Conflict)
                {
                    //Should you return this or call get and return what you get?
                    return await GetConversation(entity.Id);
                }

                throw;
            }
            return ToStartConversationResponse(entity);
        }

        public async Task<StartConversationResponse?> GetConversation(string conversationID)
        {
            try
            {
                var entity = await Container.ReadItemAsync<ConversationEntity>(
                    id: conversationID,
                    partitionKey: new PartitionKey(conversationID)
                );
                return ToStartConversationResponse(entity);
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

        public Task UpsertConversation(StartConversationRequest conversation)
        {
            throw new NotImplementedException();
        }

        private static ConversationEntity ToEntity(StartConversationRequest request)   
        {
            return new ConversationEntity(
                partitionKey: request.Participants[0],
                Id: request.Participants[0] + "_" + request.Participants[1],
                LastModifiedUnixTime: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                ReceiverUsername: request.Participants[1]
            );
        }
        private static StartConversationResponse ToStartConversationResponse(ConversationEntity entity)
        {
            return new StartConversationResponse(
                ConversationId: entity.Id,
                CreatedUnixTime: entity.LastModifiedUnixTime
            );
        }
    }
}
