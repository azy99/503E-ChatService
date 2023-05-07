using System.ComponentModel.DataAnnotations;

namespace ChatService.Web.Dtos.Profiles;

public record Profile(
    [Required] string Username,
    [Required] string FirstName,
    [Required] string LastName,
    //[Required] string ProfilePictureId         Commented out and changed to default null because of functional tests requirements
    string ProfilePictureId = null
    );