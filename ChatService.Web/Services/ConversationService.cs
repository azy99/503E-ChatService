using Azure.Core;
using ChatService.Web.Dtos.Conversations;
using ChatService.Web.Dtos.Messages;
using ChatService.Web.Dtos.Profiles;
using ChatService.Web.Exceptions;
using ChatService.Web.Storage;

namespace ChatService.Web.Services
{
    //TODO Different DTOs between layers: controller -> service -> storage to add logic in service layer
    public class ConversationService : IConversationService
    {
        private readonly IConversationStore _conversationStore;
        private readonly IProfileStore _profileStore;
        public ConversationService(IConversationStore conversationStore, IProfileStore profileStore)
        {
            _conversationStore = conversationStore;
            _profileStore = profileStore;
        }
        public Task<StartConversationResponse> CreateConversation(StartConversationRequest request)
            //TODO CALL THE AddConversation on both sides
        {
            //create current time, add it to request and ccall _conversationStore.UpdateConversation(request)
            ValidateConversation(request);
            ValidateMessage(request.FirstMessage);



            return _conversationStore.AddConversation(request);
        }

        public Task<UserConversation?> GetConversation(string conversationID)
        {
            throw new NotImplementedException();
        }


        public void ValidateConversation(StartConversationRequest request)
        {
            if (request == null)
            {
                throw new NullStartConversationRequestException(nameof(request));
            }

            if (request.Participants.Count < 2 || request.Participants.Count > 2 || request.Participants == null)
            {
                throw new ConversationNotTwoPeople();
            }
            CheckIfSenderExists(request.Participants[0]);
            CheckIfReceiverExists(request.Participants[1]);

            ValidateMessage(request.FirstMessage);
        }
        public void ValidateMessage(Message message)
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
