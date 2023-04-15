namespace ChatService.Web.Exceptions
{
    public class ReceiverDoesNotExist : KeyNotFoundException
    {
        public ReceiverDoesNotExist(string receiverId)
            : base($"The receiver with ID {receiverId} does not exist.")
        {
        }
    }
}
