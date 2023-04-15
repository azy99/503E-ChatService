namespace ChatService.Web.Dtos.Profile.Profile;

public record BlobResponse(
    string ImageId,
    string? ContentType,
    Stream? Content
);