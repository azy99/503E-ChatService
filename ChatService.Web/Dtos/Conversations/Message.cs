using System.ComponentModel.DataAnnotations;

namespace ChatService.Web.Dtos.Conversations
{
    public record Message(
        [Required] string Id,
        [Required] string SenderUsername,
        [Required] string Text
        );
}
