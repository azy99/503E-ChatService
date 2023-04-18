using System.ComponentModel.DataAnnotations;
using ChatService.Web.Dtos.Profiles;
namespace ChatService.Web.Dtos.Conversations;

public record Conversation(
    [Required] string Id,
    [Required] long LastModifiedUnixTime,
    [Required] Profile Recipient
    );
    