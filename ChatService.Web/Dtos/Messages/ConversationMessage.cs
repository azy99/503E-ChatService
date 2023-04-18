using System.ComponentModel.DataAnnotations;
namespace ChatService.Web.Dtos.Messages;

public record ConversationMessage(
    [Required] string Text,
    [Required] string SenderUsername,
    [Required] long UnixTime
    );