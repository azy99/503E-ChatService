using ChatService.Web.Dtos.Conversations;

namespace ChatService.Web.Services
{
    public interface IConversationService
    {
        Task <StartConversationResponse> CreateConversation(StartConversationRequest request);
        Task<UserConversation?> GetConversation(string conversationID);
        //Task UpdateConversation(userConversation conversation);
    }
}
