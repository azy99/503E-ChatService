using ChatService.Web.Dtos.Conversations;

namespace ChatService.Web.Services
{
    public interface IConversationService
    {
        Task <StartConversationResponse> CreateConversation(StartConversationRequest request);
        Task<UserConversation?> GetConversation(string conversationID);
        Task<EnumerateConversations> EnumerateConversations(string username,
            string? continuationToken, int? limit, long? lastSeenConversationTime);

        Task<EnumerateConversationMessages> EnumerateConversationMessages(string conversationId, string? continuationToken, int? limit,
            long? lastSeenMessageTime);
    }
}
