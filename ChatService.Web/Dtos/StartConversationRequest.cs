using System.ComponentModel.DataAnnotations;

namespace ChatService.Web.Dtos
{
    public record StartConversationRequest(
        [Required] List <string> Participants,
        [Required] Message FirstMessage
        );
    
}
