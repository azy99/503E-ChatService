using ChatService.Web.Dtos.Conversations;
using ChatService.Web.Dtos.Messages;
using ChatService.Web.Exceptions;
using ChatService.Web.Storage;

namespace ChatService.Web.Services
{
    public class MessageService : IMessageService
    {
        private readonly IConversationStore _conversationStore;
        private readonly IMessageStore _messageStore;
        private readonly ValidationManager _validationManager;
        public MessageService(IConversationStore conversationStore, IMessageStore messageStore, IProfileStore profileStore)
        {
            _conversationStore = conversationStore;
            _messageStore = messageStore;
            _validationManager = new ValidationManager(profileStore);

        }
        public async Task<UserMessage?> GetMessage(string messageId, string conversationId)
        {
            return await _messageStore.GetMessage(messageId, conversationId);
        }

        public async Task<SendMessageResponse> PostMessageToConversation(string conversationId, Message message)
        {
            try
            {
                await _validationManager.ValidateMessage(message, false);
            }
            catch (NullMessage ex)
            {
                throw ex;
            }
            catch(InvalidMessageParams ex)
            {
                throw ex;
            }
            catch(ParticipantsInvalidParams ex)
            {
                throw ex;
            }
            catch(SenderDoesNotExist ex)
            {
                throw ex;
            }

            UserMessage userMessage = new(
                conversationId,
                message.Id,
                message.Text,
                message.SenderUsername,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                );
            var addMessageTask = _messageStore.AddMessage(userMessage);
            await addMessageTask;
            var conversation = await _conversationStore.GetConversation(conversationId);
            var updatedConversation = CreateUpdatedConversation(conversation);
            await _conversationStore.UpsertConversation(updatedConversation);
            return addMessageTask.Result;
        }
        public  UserConversation CreateUpdatedConversation(UserConversation conversation)
        {         
            return  new UserConversation(
                conversation.Id,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                conversation.Sender, 
                conversation.Receiver
                );
        }
    }
}
