namespace ChatService.Web.Dtos.Profiles;

public record UploadFileRequest(UploadImageRequest ImageRequest, string UniqueFileId);