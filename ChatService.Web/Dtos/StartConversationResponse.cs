using System.ComponentModel.DataAnnotations;

namespace ChatService.Web.Dtos
{
    public record StartConversationResponse(
        [Required] string conversationId,
        [Required] long CreatedUnixTime
);
}
