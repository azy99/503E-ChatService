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
        private readonly IValidationManager _validationManager;
        public MessageService(IConversationStore conversationStore, IMessageStore messageStore, IProfileStore profileStore, IValidationManager validationManager)
        {
            _conversationStore = conversationStore;
            _messageStore = messageStore;
            _validationManager = validationManager;
        }
        public async Task<UserMessage?> GetMessage(string messageId, string conversationId)
        {
            return await _messageStore.GetMessage(messageId, conversationId);
        }

        public async Task<SendMessageResponse> PostMessageToConversation(string conversationId, Message message)
        {
            try
            {
                await _validationManager.ValidateMessage(message, false,conversationId);
            }
            catch (NullMessage ex)
            {
                throw ex;
            }
            catch(InvalidMessageParams ex)
            {
                throw ex;
            }
            catch(SenderDoesNotExist ex)
            {
                throw ex;
            }
            catch(ConversationDoesNotExist ex)
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
            var response = await _messageStore.AddMessage(userMessage);
            var conversation = await _conversationStore.GetConversation(conversationId);
            var updatedConversation = CreateUpdatedConversation(conversation);
            await _conversationStore.UpsertConversation(updatedConversation);
            return response;
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
