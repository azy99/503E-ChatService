using ChatService.Web.Dtos.Conversations;
using ChatService.Web.Dtos.Messages;
using ChatService.Web.Dtos.Profiles;

namespace ChatService.Web.Services
{
    public interface IValidationManager
    {
        public void ValidateConversation(StartConversationRequest request);
        public void ValidateMessage(Message message, bool IsFirstMessage);
        public void CheckIfSenderExists(string senderUsername);
        public void CheckIfReceiverExists(string receiverUsername);
    }
}
