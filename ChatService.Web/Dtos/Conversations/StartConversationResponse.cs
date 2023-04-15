using System.ComponentModel.DataAnnotations;

namespace ChatService.Web.Dtos.Conversations
{
    public record StartConversationResponse(
        [Required] string ConversationId,
        [Required] long CreatedUnixTime
    );
}
