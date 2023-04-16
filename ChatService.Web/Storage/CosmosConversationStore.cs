using ChatService.Web.Configuration;
using ChatService.Web.Dtos.Conversations;
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

        public async Task<StartConversationResponse> AddConversation(UserConversation UserConversation)
        {
            var ConversationEntity = ToEntity(UserConversation);
            try
            {
                await Container.CreateItemAsync(ConversationEntity);
            }
            catch (CosmosException e)
            {
                if (e.StatusCode == HttpStatusCode.Conflict)
                {
                    //Should you return this or call get and return what you get?
                    var existingConversation =  await GetConversation(ConversationEntity.Id);
                    return FromUserConversationToStartConversationResponse(existingConversation);
                }

                throw;
            }
            return ToStartConversationResponse(ConversationEntity);
        }

        public async Task<UserConversation?> GetConversation(string conversationID)
        {
            try
            {
                var entity = await Container.ReadItemAsync<ConversationEntity>(
                    id: conversationID,
                    partitionKey: new PartitionKey(conversationID)
                );
                return ToUserConversation(entity);
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
        //TODO Implement   Update Conversation to change modified time
        public Task UpsertConversation(UserConversation UserConversation)
        {
            throw new NotImplementedException();
        }

        private static ConversationEntity ToEntity(UserConversation UserConversation)   
        {
            return new ConversationEntity(
                partitionKey: UserConversation.Sender,
                Id: UserConversation.Id,
                LastModifiedUnixTime: UserConversation.LastModifiedUnixTime,
                ReceiverUsername: UserConversation.Receiver
            );
        }
        private static UserConversation ToUserConversation(ConversationEntity entity)
        {
            return new UserConversation(
                Id: entity.Id,
                LastModifiedUnixTime: entity.LastModifiedUnixTime,
                Sender: entity.partitionKey,
                Receiver: entity.ReceiverUsername
                                                                                       );
        }
        private static StartConversationResponse ToStartConversationResponse(ConversationEntity entity)
        {
            return new StartConversationResponse(
                Id: entity.Id,
                CreatedUnixTime: entity.LastModifiedUnixTime
            );
        }
        private StartConversationResponse FromUserConversationToStartConversationResponse(UserConversation UserConversation)
        {
            return new StartConversationResponse(
                Id: UserConversation.Id,
                CreatedUnixTime: UserConversation.LastModifiedUnixTime
                );
        }
    }
}
