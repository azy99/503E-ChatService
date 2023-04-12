using System.ComponentModel.DataAnnotations;
using ChatService.Web.ValidationAttributes;

namespace ChatService.Web.Dtos
{
    public record StartConversationRequest(
        [Required] [MinimumCountAttribute(2)] List <string> Participants,
        [Required] Message FirstMessage
        );
    
}
