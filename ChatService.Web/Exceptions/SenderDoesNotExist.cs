namespace ChatService.Web.Exceptions
{
    public class SenderDoesNotExist: KeyNotFoundException
    {
        public SenderDoesNotExist(string senderId)
            : base($"The sender with ID {senderId} does not exist.")
        {
        }
    }
}
