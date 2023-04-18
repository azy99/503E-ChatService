namespace ChatService.Web.Exceptions
{
    public class NullConversation : ArgumentNullException
    {
        public NullConversation()
        : base("The conversation parameter cannot be null or empty.")
        {
        }
    }
}
