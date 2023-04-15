using ChatService.Web.Dtos.Conversations;
using ChatService.Web.Dtos.Messages;
using ChatService.Web.Dtos.Profiles;
using ChatService.Web.Exceptions;
using ChatService.Web.Storage;

namespace ChatService.Web.Services
{
    public class ValidationManager: IValidationManager
    {
        private readonly IConversationStore _conversationStore;
        private readonly IProfileStore _profileStore;
        public ValidationManager(IConversationStore conversationStore, IProfileStore profileStore)
        {
            _conversationStore = conversationStore;
            _profileStore = profileStore;
        }
        public void ValidateConversation(StartConversationRequest request)
        {
            if (request == null)
            {
                throw new NullStartConversationRequestException(nameof(request));
            }

            if (request.Participants.Length< 2 || request.Participants.Length > 2 || request.Participants == null)
            {
                throw new ConversationNotTwoPeople();
            }
            CheckIfSenderExists(request.Participants[0]);
            CheckIfReceiverExists(request.Participants[1]);

            ValidateMessage(request.FirstMessage,true);

        }
        public void ValidateMessage(Message message,bool IsFirstMessage)
        {
            if (message == null)
            {
                throw new NullMessage();
            }
            if (string.IsNullOrEmpty(message.Id) ||
               string.IsNullOrEmpty(message.SenderUsername) ||
               string.IsNullOrEmpty(message.Text))
            {
                throw new InvalidMessageParams();
            }
            if (!IsFirstMessage)
            {
                CheckIfSenderExists(message.SenderUsername);
            }
        }
        public void CheckIfSenderExists(string senderUsername)
        {
            var sender = _profileStore.GetProfile(senderUsername);
            if (sender == null)
            {
                throw new SenderDoesNotExist(senderUsername);
            }
        }
        public void CheckIfReceiverExists(string receiverUsername)
        {
            var recipient = _profileStore.GetProfile(receiverUsername);
            if (recipient == null)
            {
                throw new ReceiverDoesNotExist(receiverUsername);
            }
        }
    }
}
