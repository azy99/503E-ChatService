using System.ComponentModel.DataAnnotations;

namespace ChatService.Web.Dtos.Profiles;

public record UploadImageRequest([Required] IFormFile File);