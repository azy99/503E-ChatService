using ChatService.Web.Dtos;

namespace ChatService.Web.Services
{
    public interface IConversationService
    {
        Task <StartConversationResponse> CreateConversation(StartConversationRequest request);
        Task<userConversation?> GetConversation(string conversationID);
        //Task UpdateConversation(userConversation conversation);
    }
}
