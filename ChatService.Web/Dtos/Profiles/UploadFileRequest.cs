namespace ChatService.Web.Dtos.Profile;

public record UploadFileRequest(UploadImageRequest ImageRequest, string UniqueFileId);