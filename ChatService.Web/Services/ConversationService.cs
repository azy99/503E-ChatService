using ChatService.Web.Dtos.Conversations;
using ChatService.Web.Exceptions;
using ChatService.Web.Storage;

namespace ChatService.Web.Services
{
    //TODO Different DTOs between layers: controller -> service -> storage to add logic in service layer
    public class ConversationService : IConversationService
    {
        private readonly IConversationStore _conversationStore;
        public ConversationService(IConversationStore conversationStore)
        {
            _conversationStore = conversationStore;
        }

        public Task<StartConversationResponse> CreateConversation(StartConversationRequest request)
            //TODO CALL THE AddConversation on both sides
            //TODO Add Validations here
            //TODO Add toEntitie in service
        {
            //create current time, add it to request and ccall _conversationStore.UpdateConversation(request)

            return _conversationStore.AddConversation(request);
        }

        public Task<userConversation?> GetConversation(string conversationID)
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
    }
}
