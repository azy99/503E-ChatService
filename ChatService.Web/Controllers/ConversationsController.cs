using ChatService.Web.Dtos;
using ChatService.Web.Exceptions;
using ChatService.Web.Services;
using ChatService.Web.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;

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
           // ValidateConversation(request); Validation done on storage layer?

            StartConversationResponse response = await _conversationService.CreateConversation(request);

            return CreatedAtAction(nameof(GetConversation), new { conversationId = response.ConversationId }, response);
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

        //public void ValidateConversation(StartConversationRequest request)
        //{
        //    if(request == null)
        //    {
        //        throw new NullStartConversationRequestException(nameof(request));
        //    }

        //    var participantsCount = request.Participants.Count;
        //    if(request.Participants.Count < 2 || request.Participants.Count>2) {
        //        throw new ConversationNotTwoPeople();
        //    }

        //    ValidateMessage(request.FirstMessage);
        //}

        //public void ValidateMessage(Message message)
        //{
        //    //If ID null or empty
        //    //If SenderUsername null or empty
        //    //If text null or empty
        //    //But required validation in DTO should deal with that

        //    //Only left to check if sender exists
        //    if(message == null)
        //    {
        //        throw new NullMessage();
        //    }
            
        //}
    }

}
