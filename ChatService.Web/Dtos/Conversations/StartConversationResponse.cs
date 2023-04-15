using System.ComponentModel.DataAnnotations;

namespace ChatService.Web.Dtos.Conversations
{
    public record StartConversationResponse(
        [Required] string Id,
        [Required] long CreatedUnixTime
    );
}
