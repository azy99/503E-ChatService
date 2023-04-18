using ChatService.Web.Dtos.Messages;

namespace ChatService.Web.Services
{
    public interface IMessageService
    {
        Task<UserMessage?> GetMessage(string messageId, string conversationId);
        Task<SendMessageResponse> PostMessageToConversation(string conversationId, Message message);
    }
}
