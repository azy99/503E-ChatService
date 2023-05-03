using ChatService.Web.Dtos.Messages;

namespace ChatService.Web.Dtos.Conversations
{
    public record EnumerateConversationMessages(
        string continuationToken,
        long lastSeenMessageTime,
        ConversationMessage[] ConversationMessages
        );
}
