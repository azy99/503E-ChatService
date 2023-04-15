using ChatService.Web.Dtos.Conversations;

namespace ChatService.Web.Storage
{
    public interface IConversationStore
    {
        Task UpsertConversation(StartConversationRequest conversation);
        Task<StartConversationResponse?> GetConversation(string conversationID);
        Task<StartConversationResponse> AddConversation(StartConversationRequest request);
    }
}
