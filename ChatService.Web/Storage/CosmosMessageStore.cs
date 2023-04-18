using ChatService.Web.Dtos.Conversations;
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
        private Container Container => _cosmosClient.GetDatabase("profiles").GetContainer("messages");
        public async Task<SendMessageResponse> AddMessage(UserMessage message)
        {
            var messageEntity = ToEntity(message);
            try
            {
                await Container.CreateItemAsync(messageEntity);
            }
            catch (CosmosException e)
            {
                if (e.StatusCode == HttpStatusCode.Conflict)
                {
                    var postedMessage = await GetMessage(message.Id,message.ConversationId);
                    return ToSendMessageResponse(ToEntity(postedMessage));
                }
            }
            return ToSendMessageResponse(messageEntity);
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
        public async Task DeleteMessage(string messageID, string conversationID)
        {
            try
            {
                await Container.DeleteItemAsync<UserMessage>(
                    id: messageID,
                    partitionKey: new PartitionKey(conversationID)
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
        }
        private static MessageEntity ToEntity(UserMessage message)
        {
            return new MessageEntity(
                partitionKey: message.ConversationId,
                id: message.Id,
                Text: message.Text,
                SenderUsername: message.SenderUsername,
                UnixTime: message.UnixTime
            );
        }
        private static UserMessage ToUserMessage(MessageEntity messageEntity)
        {
            return new UserMessage(
                ConversationId: messageEntity.partitionKey,
                Id: messageEntity.id,
                Text:messageEntity.Text,
                SenderUsername:messageEntity.SenderUsername,
                UnixTime: messageEntity.UnixTime
                );
        }
        private static SendMessageResponse ToSendMessageResponse(MessageEntity messageEntity)
        {
            return new SendMessageResponse(messageEntity.UnixTime);
        }
    }
}
