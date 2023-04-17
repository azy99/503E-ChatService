using ChatService.Web.Dtos.Conversations;
using ChatService.Web.Dtos.Messages;

namespace ChatService.Web.Services
{
    public interface IConversationService
    {
        Task <StartConversationResponse> CreateConversation(StartConversationRequest request);
        Task<UserConversation?> GetConversation(string conversationID);

    }
}
