using System.ComponentModel.DataAnnotations;

namespace ChatService.Web.Dtos.Profile;

public record UploadImageRequest([Required] IFormFile File);