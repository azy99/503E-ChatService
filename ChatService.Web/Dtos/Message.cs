using System.ComponentModel.DataAnnotations;

namespace ChatService.Web.Dtos
{
    public record Message(
        [Required] string Id,
        [Required] string SenderUsername,
        [Required] string Text
        );
}
