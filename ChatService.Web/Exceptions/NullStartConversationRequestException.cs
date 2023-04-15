using ChatService.Web.Dtos;

namespace ChatService.Web.Exceptions
{
    public class NullStartConversationRequestException: ArgumentNullException
    {
        public NullStartConversationRequestException(string paramName)
            : base(paramName, "The input parameter cannot be null.")
        {
        }
    }
    
}
