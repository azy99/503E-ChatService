namespace ChatService.Web.Exceptions
{
    public class NullMessage: ArgumentNullException
    {
        public NullMessage()
        : base("The message parameter cannot be null.")
        {
        }
    }
}
