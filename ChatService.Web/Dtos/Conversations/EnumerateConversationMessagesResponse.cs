using ChatService.Web.Dtos.Messages;
namespace ChatService.Web.Dtos.Conversations;

public record EnumerateConversationMessagesResponse(
    List<ConversationMessage> Messages,
    String NextUri
);