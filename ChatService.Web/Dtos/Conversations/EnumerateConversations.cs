namespace ChatService.Web.Dtos.Conversations
{
    public record EnumerateConversations(
        string continuationToken,
        long lastSeenConversationTime,
        Conversation[] Conversations
        );
}
