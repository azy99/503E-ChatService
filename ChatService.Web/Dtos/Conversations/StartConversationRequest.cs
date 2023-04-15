using System.ComponentModel.DataAnnotations;
using ChatService.Web.Dtos.Messages;

namespace ChatService.Web.Dtos.Conversations
{
    public record StartConversationRequest(
        [Required] List<string> Participants,
        [Required] Message FirstMessage
        );

}
