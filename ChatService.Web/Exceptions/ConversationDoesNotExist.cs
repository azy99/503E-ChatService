namespace ChatService.Web.Exceptions
{
    public class ConversationDoesNotExist:KeyNotFoundException
    {
        public ConversationDoesNotExist(string conversationId)
            : base($"The conversation with following Id:{conversationId} does not exist.")
        {
        }
    }
}
