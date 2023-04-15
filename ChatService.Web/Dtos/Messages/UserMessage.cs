using System.ComponentModel.DataAnnotations;

namespace ChatService.Web.Dtos.Messages
{
    public record UserMessage(
        [Required] string Id,
        [Required] string Text,
        [Required] string SenderUsername,
        [Required] long UnixTime
        );
}
