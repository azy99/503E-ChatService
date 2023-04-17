using Azure.Core;
using ChatService.Web.Dtos.Conversations;
using ChatService.Web.Dtos.Messages;
using ChatService.Web.Dtos.Profiles;
using ChatService.Web.Exceptions;
using ChatService.Web.Storage;

namespace ChatService.Web.Services
{
    public class ConversationService : IConversationService
    {
        private readonly IConversationStore _conversationStore;
        private readonly IMessageStore _messageStore;
        private readonly ValidationManager _validationManager;
        public ConversationService(IConversationStore conversationStore, IMessageStore messageStore, IProfileStore profileStore)
        {
            _conversationStore = conversationStore;
            _messageStore = messageStore;
            _validationManager = new ValidationManager(profileStore, conversationStore);
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

            UserMessage message = new (
                ConversationId : request.Participants[0] + "_" + request.Participants[1],
                Id: request.FirstMessage.Id,
                Text: request.FirstMessage.Text,
                SenderUsername: request.FirstMessage.SenderUsername,
                UnixTime : DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                );
            UserConversation conversation1 = new (
                Id : request.Participants[0] + "_" + request.Participants[1],
                LastModifiedUnixTime : DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Sender: request.Participants[0],
                Receiver: request.Participants[1]
                );
            UserConversation conversation2 = new(
                Id: request.Participants[1] + "_" + request.Participants[0],
                LastModifiedUnixTime: DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Sender: request.Participants[1],
                Receiver: request.Participants[0]
                );

            var addMessageTask = _messageStore.AddMessage(message);
            var addConversation1Task = _conversationStore.AddConversation(conversation1);
            var addConversation2Task = _conversationStore.AddConversation(conversation2);

            await Task.WhenAll( addMessageTask, addConversation1Task, addConversation2Task );

            return addConversation1Task.Result;
        }

        public Task<UserConversation?> GetConversation(string conversationID)
        {
            return _conversationStore.GetConversation(conversationID);
        }
    }
}
