namespace ChatService.Web.Storage.Entities
{
    public record MessageEntity(
        string partitionKey,    //ConversationId
        string Id,              //messageId
        string Text,
        string SenderUsername,  
        string UnixTime
        );
}
