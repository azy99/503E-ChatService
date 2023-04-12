using ChatService.Web.Dtos;
using ChatService.Web.Services;
using ChatService.Web.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;

namespace ChatService.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConversationController : ControllerBase { 
        private readonly IConversationService _conversationService;
        public ConversationController(IConversationService conversationService)
        {
            _conversationService = conversationService;
        }
        [HttpGet("conversationId")]
        public async Task<ActionResult<userConversation>> GetConversation(string conversationId)
        {
            var conversation = await _conversationService.GetConversation(conversationId);
            if (conversation == null)
            {
                return NotFound($"A User with conversationID {conversationId} was not found");
            }
            return Ok(conversation);
        }

        [HttpPost]
        public async Task<ActionResult<StartConversationResponse>> AddConversation(StartConversationRequest request)
        {
            //TODO ADD VALIDATION FOR INCORRECT INPUT
            StartConversationResponse response = await _conversationService.CreateConversation(request);

            return CreatedAtAction(nameof(GetConversation), new { conversationId = response.conversationId }, response);
        }

        //[HttpPut("conversationId")]{
        //public async Task<ActionResult<userConversation>> UpdateProfile(string username, PutProfileRequest request)
        //{
        //    var existingProfile = await _profileService.GetProfile(username);
        //    if (existingProfile == null)
        //    {
        //        return NotFound($"A User with username {username} was not found");
        //    }

        //    var profile = new Profile(username, request.firstName, request.lastName);
        //    await _profileService.UpdateProfile(profile);

        //    _logger.LogInformation("Updated Profile for {Username}", profile.Username);

        //    return Ok(profile);
        //}
    }

}
