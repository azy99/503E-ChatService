using ChatService.Web.Dtos;
using ChatService.Web.Storage;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class ImagesController : ControllerBase
{
    private readonly IFileStore _fileStore;
    
    public ImagesController(IFileStore fileStore)
    {
        _fileStore = fileStore;
    }

    [HttpGet("{imageId}")]
    public async Task<IActionResult> DownloadImage(string imageId)
    {
        BlobResponse response = await _fileStore.DownloadFile(imageId);
        if (response.Content == null)
        {
            return NotFound($"An image with {imageId} was not found");
        }
        return File(response.Content, response.ContentType);
    }
    
    [HttpPost]
    public async Task<ActionResult<UploadImageResponse>> UploadImage([FromForm] UploadImageRequest request)
    {
       var response = await _fileStore.UploadFile(request);

       return CreatedAtAction(nameof(DownloadImage), new {imageId=response.ImageId}, response);

    }

    [HttpDelete("{imageId}")]
    public async Task<ActionResult> DeleteImage(string imageId)
    {
        await _fileStore.DeleteFile(imageId);
        return NoContent();
    }
    
    
    
}