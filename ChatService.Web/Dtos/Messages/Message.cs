using System.ComponentModel.DataAnnotations;

namespace ChatService.Web.Dtos.Messages
{
    public record Message(
        [Required] string Id,
        [Required] string SenderUsername,
        [Required] string Text
        );
}
