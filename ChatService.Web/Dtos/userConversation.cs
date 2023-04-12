using System.ComponentModel.DataAnnotations;

namespace ChatService.Web.Dtos
{
    public record userConversation(
        [Required] string Sender,
        [Required] string Receiver,
        [Required] Message FirstMessage
        );

}
