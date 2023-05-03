using ChatService.Web.Dtos.Messages;
namespace ChatService.Web.Dtos.Conversations;

public record EnumerateConversationMessagesResponse(
    ConversationMessage[] Messages,
    String NextUri
);