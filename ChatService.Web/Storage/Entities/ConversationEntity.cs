﻿namespace ChatService.Web.Storage.Entities
{
    public record ConversationEntity(
        string partitionKey,        //Username
        string id,                  // ConversationId: SenderUsername_ReceiverUsername  split based on _ to retrieve sender, receiver
        long LastModifiedUnixTime,
        string ReceiverUsername
        );
    
}
