using ChatService.Web.Configuration;
using ChatService.Web.Dtos.Conversations;
using ChatService.Web.Storage.Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using System.Net;
using ChatService.Web.Dtos.Messages;
using ChatService.Web.Dtos.Profiles;

namespace ChatService.Web.Storage
{
    public class CosmosConversationStore : IConversationStore
    {
        private readonly CosmosClient _cosmosClient;
        private readonly IProfileStore _profileStore;
        public CosmosConversationStore(IOptions<CosmosSettings> options, IProfileStore profileStore)
        {
            _cosmosClient = new CosmosClient(options.Value.ConnectionString);
            _profileStore = profileStore;
        }

        private Container ConversationsContainer => _cosmosClient.GetDatabase("profiles").GetContainer("conversations");
        private Container MessagesContainer => _cosmosClient.GetDatabase("profiles").GetContainer("messages");

        public async Task<StartConversationResponse> AddConversation(UserConversation UserConversation)
        {
            var ConversationEntity = ToEntity(UserConversation);
            try
            {
                await ConversationsContainer.CreateItemAsync(ConversationEntity);
            }
            catch (CosmosException e)
            {
                if (e.StatusCode == HttpStatusCode.Conflict)
                {
                    var existingConversation =  await GetConversation(ConversationEntity.id);
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
                var entity = await ConversationsContainer.ReadItemAsync<ConversationEntity>(
                    id: conversationID,
                    partitionKey: new PartitionKey(conversationID.Split("_")[0])
                );
                return ToUserConversation(entity);
            }
            catch (CosmosException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw e;
            }
        }

        public async Task<EnumerateConversationsResponse> EnumerateConversations(string username,
            string? continuationToken, int? limit, long? lastSeenConversationTime)
        {
            var query = lastSeenConversationTime != null && string.IsNullOrEmpty(continuationToken)
                ? new QueryDefinition(
                    $"SELECT * FROM conversations WHERE conversations.LastModifiedUnixTime > {lastSeenConversationTime} ORDER BY conversations.LastModifiedUnixTime DESC")
                : new QueryDefinition($"SELECT * FROM conversations ORDER BY conversations.LastModifiedUnixTime DESC");
            var getConversations = ConversationsContainer.GetItemQueryIterator<ConversationEntity>(
                query,
                continuationToken: continuationToken,
                requestOptions:new QueryRequestOptions()
                {
                    MaxItemCount = limit != null ? limit: -1,
                    PartitionKey = new PartitionKey(username),
                    ConsistencyLevel = ConsistencyLevel.Session
                }
            );
            
            var conversationEntities = await getConversations.ReadNextAsync();
            var conversations = new List<Conversation>();
            foreach (var conv in conversationEntities)
            {
                var profile = await _profileStore.GetProfile(conv.ReceiverUsername);
                conversations.Add(ToConversation(conv, profile));
            }
            var nextContinuationToken = conversationEntities.ContinuationToken;
            var nextUri = $"/api/conversations/?username={username}&";
            
            if (limit != null && limit != 0)
            {
                nextUri += $"limit={limit}&";
            }
    
            if (conversations.Count > 0)
            {
                var lastSeen = conversations[0].LastModifiedUnixTime;
                nextUri += $"lastSeenConversationTime={lastSeen}&continuationToken={nextContinuationToken}";
            }
            
            return new EnumerateConversationsResponse(conversations, nextUri);
        }

        public async Task<EnumerateConversationMessages> EnumerateConversationMessages(string conversationId,
            string? continuationToken, int? limit, long? lastSeenMessageTime)
        {
            var query = lastSeenMessageTime != null && string.IsNullOrEmpty(continuationToken)
                ? new QueryDefinition(
                    $"SELECT * FROM messages WHERE messages.UnixTime > {lastSeenMessageTime} ORDER BY messages.UnixTime DESC")
                : new QueryDefinition($"SELECT * FROM messages ORDER BY messages.UnixTime DESC");
            var getMessages = MessagesContainer.GetItemQueryIterator<MessageEntity>(
                query,
                continuationToken: continuationToken,
                requestOptions:new QueryRequestOptions()
                {
                    MaxItemCount = limit != null ? limit: -1,
                    PartitionKey = new PartitionKey(conversationId),
                    ConsistencyLevel = ConsistencyLevel.Session
                }
            );
            var messageEntities = await getMessages.ReadNextAsync();
            var messages = new ConversationMessage[] { };
            foreach (var m in messageEntities)
            {
                messages = messages.Append(ToConversationMessage(m)).ToArray();
            }
            var nextContinuationToken = messageEntities.ContinuationToken;
            lastSeenMessageTime = messages.Length > 0 ? messages.Last().UnixTime : 0;
            return new EnumerateConversationMessages(nextContinuationToken, (long)lastSeenMessageTime, messages);
        }
        public async Task UpsertConversation(UserConversation UserConversation)
        {
            try
            {
                await ConversationsContainer.UpsertItemAsync<ConversationEntity>(ToEntity(UserConversation));
            }
            catch(CosmosException e)
            {
                throw e;
            }
        }
        public async Task DeleteConversation(string username, string conversationId)
        {
            try
            {
                await ConversationsContainer.DeleteItemAsync<UserConversation>(
                    id: username,
                    partitionKey: new PartitionKey(conversationId)
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

        private static ConversationEntity ToEntity(UserConversation UserConversation)   
        {
            return new ConversationEntity(
                partitionKey: UserConversation.Sender,
                id: UserConversation.Id,
                LastModifiedUnixTime: UserConversation.LastModifiedUnixTime,
                ReceiverUsername: UserConversation.Receiver
            );
        }
        private static UserConversation ToUserConversation(ConversationEntity entity)
        {
            return new UserConversation(
                Id: entity.id,
                LastModifiedUnixTime: entity.LastModifiedUnixTime,
                Sender: entity.partitionKey,
                Receiver: entity.ReceiverUsername
                                                                                       );
        }
        private static StartConversationResponse ToStartConversationResponse(ConversationEntity entity)
        {
            return new StartConversationResponse(
                Id: entity.id,
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

        private static Conversation ToConversation(ConversationEntity entity, Profile profile)
        {
            return new Conversation(
                Id: entity.id,
                LastModifiedUnixTime: entity.LastModifiedUnixTime,
                Recipient: profile
            );
        }

        private static ConversationMessage ToConversationMessage(MessageEntity entity)
        {
            return new ConversationMessage(
                Text: entity.Text,
                SenderUsername: entity.SenderUsername,
                UnixTime: entity.UnixTime
            );
        }
    }
}
