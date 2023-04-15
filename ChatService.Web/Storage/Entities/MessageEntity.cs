using System.ComponentModel.DataAnnotations;

namespace ChatService.Web.Storage.Entities
{
    public record MessageEntity(
        [Required] string partitionKey,    //ConversationId
        [Required] string Id,              //messageId
        [Required] string Text,
        [Required] string SenderUsername,
        [Required] long UnixTime
        );
}
