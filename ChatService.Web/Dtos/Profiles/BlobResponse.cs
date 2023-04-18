namespace ChatService.Web.Dtos.Profiles;

public record BlobResponse(
    string ImageId,
    string? ContentType,
    Stream? Content
);