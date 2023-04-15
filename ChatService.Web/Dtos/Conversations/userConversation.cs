using System.ComponentModel.DataAnnotations;

namespace ChatService.Web.Dtos.Conversations
{
    public record userConversation(
        [Required] string Sender,
        [Required] string Receiver
        );

}
