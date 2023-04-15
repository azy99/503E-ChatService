using System.ComponentModel.DataAnnotations;

namespace ChatService.Web.Dtos.Conversations
{
    public record StartConversationRequest(
        [Required] List<string> Participants,
        [Required] Message FirstMessage
        );

}
