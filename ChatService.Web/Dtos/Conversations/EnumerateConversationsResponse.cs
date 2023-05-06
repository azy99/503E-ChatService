namespace ChatService.Web.Dtos.Conversations;

public record EnumerateConversationsResponse(
    Conversation[] Conversations,
    string NextUri
    );