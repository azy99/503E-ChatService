using ChatService.Web.Dtos;

namespace ChatService.Web.Exceptions
{
    public class NullStartConversationRequestException: ArgumentNullException
    {
        public NullStartConversationRequestException()
            : base("The start of a conversation cannot be null.")
        {
        }
    }
    
}
