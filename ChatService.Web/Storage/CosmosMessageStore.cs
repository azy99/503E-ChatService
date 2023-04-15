using ChatService.Web.Dtos.Messages;
using ChatService.Web.Dtos.Profiles;
using ChatService.Web.Storage.Entities;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System.Net;

namespace ChatService.Web.Storage
{
    public class CosmosMessageStore : IMessageStore
    {
        private readonly CosmosClient _cosmosClient;

        public CosmosMessageStore(CosmosClient cosmosClient)
        {
            _cosmosClient = cosmosClient;
        }
        private Container Container => _cosmosClient.GetDatabase("profiles").GetContainer("sharedContainer");
        public async Task<UserMessage> AddMessage(UserMessage message)
        {
            var messageEntity = ToEntity(message);
            try
            {
                await Container.UpsertItemAsync(messageEntity);
            }
            catch (CosmosException e)
            {
                if (e.StatusCode == HttpStatusCode.Conflict)
                {
                    return await GetMessage(message.Id,message.ConversationId);
                }
            }
            return ToUserMessage(messageEntity);
        }
        public async Task<UserMessage?> GetMessage(string messageID,string conversationID)
        {
            try
            {
                var entity = await Container.ReadItemAsync<MessageEntity>(
                    id: messageID,
                    partitionKey: new PartitionKey(conversationID)
                );
                return ToUserMessage(entity);
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
        private static MessageEntity ToEntity(UserMessage message)
        {
            return new MessageEntity(
                partitionKey: message.ConversationId,
                Id: message.Id,
                Text: message.Text,
                SenderUsername: message.SenderUsername,
                UnixTime: message.UnixTime
            );
        }
        private static UserMessage ToUserMessage(MessageEntity messageEntity)
        {
            return new UserMessage(
                ConversationId: messageEntity.partitionKey,
                Id: messageEntity.Id,
                Text:messageEntity.Text,
                SenderUsername:messageEntity.SenderUsername,
                UnixTime: messageEntity.UnixTime
                );
        }

    }
}
