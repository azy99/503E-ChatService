using ChatService.Web.Dtos.Messages;

namespace ChatService.Web.Storage
{
    public interface IMessageStore
    {
        public Task<UserMessage> AddMessage(UserMessage message);
        public Task<UserMessage?> GetMessage(string messageID, string conversationID);
    }
}
