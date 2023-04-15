namespace ChatService.Web.Exceptions
{
    public class InvalidMessageParams: ArgumentException
    {
        public InvalidMessageParams()
        : base("The message parameters cannot be null or empty .")
        {
        }
    }
}
