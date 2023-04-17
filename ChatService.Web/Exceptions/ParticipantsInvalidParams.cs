namespace ChatService.Web.Exceptions
{
    public class ParticipantsInvalidParams:ArgumentNullException
    {
        public ParticipantsInvalidParams()
            : base("Both receiver and sender values cannot be null or empty.")
        {
        }
    }
}
