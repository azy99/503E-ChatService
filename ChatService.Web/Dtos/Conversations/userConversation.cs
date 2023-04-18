using ChatService.Web.Dtos.Profiles;
using System.ComponentModel.DataAnnotations;

namespace ChatService.Web.Dtos.Conversations
{
    public record UserConversation(
        [Required] string Id,
        [Required] long LastModifiedUnixTime,
        [Required] string Sender,
        [Required] string Receiver
        );
}
