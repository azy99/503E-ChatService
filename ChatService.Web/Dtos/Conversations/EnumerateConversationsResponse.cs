namespace ChatService.Web.Dtos.Conversations;

public record EnumerateConversationsResponse(
    List<Conversation> Conversations,
    String NextUri
    );