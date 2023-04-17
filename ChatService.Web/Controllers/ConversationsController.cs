using ChatService.Web.Dtos.Conversations;
using ChatService.Web.Exceptions;
using ChatService.Web.Services;
using ChatService.Web.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Newtonsoft.Json;

namespace ChatService.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConversationsController : ControllerBase { 
        private readonly IConversationService _conversationService;
        public ConversationsController(IConversationService conversationService)
        {
            _conversationService = conversationService;
        }
        [HttpGet("{conversationId}")]
        public async Task<ActionResult<UserConversation>> GetConversation(string conversationId)
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
            try {
                StartConversationResponse response = await _conversationService.CreateConversation(request);
                return CreatedAtAction(nameof(GetConversation), new { conversationId = response.Id }, response);

            }
            catch(NullStartConversationRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch(ConversationNotTwoPeople ex)
            {
                return BadRequest(ex.Message);
            }
            catch (SenderDoesNotExist ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ReceiverDoesNotExist ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ParticipantsInvalidParams ex)
            {
                return BadRequest(ex.Message);
            }
            catch (NullMessage ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidMessageParams ex)
            {
                return BadRequest(ex.Message);
            }
            
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
    }

}
