using ChatService.Web.Dtos.Conversations;

namespace ChatService.Web.Storage
{
    public interface IConversationStore
    {
        Task UpsertConversation(UserConversation UserConversation);
        Task<UserConversation?> GetConversation(string conversationID);
        Task<StartConversationResponse> AddConversation(UserConversation UserConversation);
    }
}
