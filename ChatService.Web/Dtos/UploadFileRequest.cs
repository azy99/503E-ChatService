namespace ChatService.Web.Dtos;

public record UploadFileRequest(UploadImageRequest ImageRequest, String UniqueFileId);