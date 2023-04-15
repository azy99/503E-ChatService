using System.ComponentModel.DataAnnotations;

namespace ChatService.Web.Dtos
{
    public record StartConversationResponse(
        [Required] string ConversationId,
        [Required] long CreatedUnixTime
);
}
