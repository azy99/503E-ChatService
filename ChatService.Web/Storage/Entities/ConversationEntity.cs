namespace ChatService.Web.Storage.Entities
{
    public record ConversationEntity(
        string partitionKey,        //Username
        string Id,                  // ConversationId: SenderUsername_ReceiverUsername  split based on _ to retrieve sender, receiver
        long LastModifiedUnixTime,
        string ReceiverUsername     //TODO INSTEAD OF ONLY RECEIVERUSERNAME, MIGHT AS WELL GET THE PROFILE AND SAVE ITS
        );
    
}
