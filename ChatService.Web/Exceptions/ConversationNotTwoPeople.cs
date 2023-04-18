namespace ChatService.Web.Exceptions
{
    public class ConversationNotTwoPeople: ArgumentException
    {
        public ConversationNotTwoPeople()
            : base( "The number of Participants cannot be more or less than 2 .")
        {
        }
    }
}
