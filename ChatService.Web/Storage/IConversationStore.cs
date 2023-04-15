using ChatService.Web.Dtos.Conversations;

namespace ChatService.Web.Storage
{
    public interface IConversationStore
    {
        Task UpsertConversation(UserConversation UserConversation);
        Task<StartConversationResponse?> GetConversation(string conversationID);
        Task<StartConversationResponse> AddConversation(UserConversation UserConversation);
    }
}
