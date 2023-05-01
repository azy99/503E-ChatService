using ChatService.Web.Dtos.Conversations;
using ChatService.Web.Dtos.Messages;

namespace ChatService.Web.Services
{
    public interface IValidationManager
    {
        public Task ValidateConversation(StartConversationRequest request);
        public Task ValidateMessage(Message message, bool isFirstMessage, string conversationId);
        public Task CheckIfSenderExists(string senderUsername);
        public  Task CheckIfReceiverExists(string receiverUsername);
        public Task CheckIfConversationExists(string conversationId);
    }
}
