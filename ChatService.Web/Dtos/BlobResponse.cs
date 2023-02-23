namespace ChatService.Web.Dtos;

public record BlobResponse(
    string ImageId,
    string? ContentType,
    Stream? Content
);