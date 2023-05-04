using Azure.Core;
using ChatService.Web.Dtos.Conversations;
using ChatService.Web.Dtos.Messages;
using ChatService.Web.Dtos.Profiles;
using ChatService.Web.Exceptions;
using ChatService.Web.Storage;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;

namespace ChatService.Web.Services
{
    public class ConversationService : IConversationService
    {
        private readonly IConversationStore _conversationStore;
        private readonly IMessageStore _messageStore;
        private readonly IValidationManager _validationManager;
        public ConversationService(IConversationStore conversationStore, IMessageStore messageStore, IProfileStore profileStore,IValidationManager validationManager)
        {
            _conversationStore = conversationStore;
            _messageStore = messageStore;
            _validationManager = validationManager;
        }
        public async Task<StartConversationResponse> CreateConversation(StartConversationRequest request)
        {
            try
            {
                await _validationManager.ValidateConversation(request);
            }
            catch (SenderDoesNotExist ex)
            {
                throw ex;
            }
            catch (ReceiverDoesNotExist ex)
            {
                throw ex;
            }
            catch (NullMessage ex)
            {
                throw ex;
            }
            catch (InvalidMessageParams ex)
            {
                throw ex;
            }
            catch(ParticipantsInvalidParams ex)
            {
                throw ex;
            }
            UserMessage message = createUserMessageForConversationStart(request);
            (UserConversation, UserConversation) conversations = createConversationUserConversations(request);

            var addMessageTask = _messageStore.AddMessage(message);
            var addConversation1Task = _conversationStore.AddConversation(conversations.Item1);
            var addConversation2Task = _conversationStore.AddConversation(conversations.Item2);
            await Task.WhenAll( addMessageTask, addConversation1Task, addConversation2Task );

            return addConversation1Task.Result;
        }

        public Task<UserConversation?> GetConversation(string conversationID)
        {
            return _conversationStore.GetConversation(conversationID);
        }

        public async Task<EnumerateConversations> EnumerateConversations(string username, string? continuationToken,
            int? limit, long? lastSeenConversationTime)
        {
            try
            {
                await _validationManager.CheckIfSenderExists(username);
            }
            catch (SenderDoesNotExist ex)
            {
                throw ex;
            }
            return await _conversationStore.EnumerateConversations(username, continuationToken, limit, lastSeenConversationTime);
        }

        public async Task<EnumerateConversationMessages> EnumerateConversationMessages(string conversationId,
            string? continuationToken, int? limit, long? lastSeenMessageTime)
        {
            try
            {
                await _validationManager.CheckIfConversationExists(conversationId);
            }
            catch (ConversationDoesNotExist ex)
            {
                throw ex;
            }
            return await _conversationStore.EnumerateConversationMessages(conversationId, continuationToken, limit,
                lastSeenMessageTime);
        }
        public (UserConversation,UserConversation) createConversationUserConversations(StartConversationRequest request)
        {
            UserConversation conversation1 = new(
                Id: request.Participants[0] + "_" + request.Participants[1],
                LastModifiedUnixTime: DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Sender: request.Participants[0],
                Receiver: request.Participants[1]
                );
            UserConversation conversation2 = new(
                Id: request.Participants[1] + "_" + request.Participants[0],
                LastModifiedUnixTime: DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Sender: request.Participants[1],
                Receiver: request.Participants[0]
                );
            return (conversation1, conversation2);
        }
        public UserMessage createUserMessageForConversationStart(StartConversationRequest request)
        {
            UserMessage message = new(
                ConversationId: request.Participants[0] + "_" + request.Participants[1],
                Id: request.FirstMessage.Id,
                Text: request.FirstMessage.Text,
                SenderUsername: request.FirstMessage.SenderUsername,
                UnixTime: DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                );
            return message;
        }
    }
}
