using ChatService.Web.Dtos.Conversations;

namespace ChatService.Web.Storage
{
    public interface IConversationStore
    {
        Task UpsertConversation(UserConversation UserConversation);
        Task<UserConversation?> GetConversation(string conversationID);
        Task<StartConversationResponse> AddConversation(UserConversation UserConversation);
        Task DeleteConversation(string username, string conversationId);
        Task<EnumerateConversationsResponse> EnumerateConversations(string username,
            string? continuationToken, int? limit, long? lastSeenConversationTime);
        Task<EnumerateConversationMessages> EnumerateConversationMessages(string conversationId,
            string? continuationToken, int? limit, long? lastSeenMessageTime);
    }
}
