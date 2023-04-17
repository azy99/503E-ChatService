using ChatService.Web.Dtos.Conversations;
using ChatService.Web.Dtos.Messages;
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
        private readonly IMessageService _messageService;
        public ConversationsController(IConversationService conversationService, IMessageService messageService)
        {
            _conversationService = conversationService;
            _messageService = messageService;
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
        [HttpGet("{conversationId}/{messageId}")]
        public async Task<ActionResult<UserConversation>> GetMessage(string conversationId, string messageId)
        {
            var conversation = await _messageService.GetMessage(messageId, conversationId);
            if (conversation == null)
            {
                return NotFound($"A User with conversationID {conversationId} was not found");
            }
            return Ok(conversation);
        }
        [HttpPost("{conversationId}/messages")]
        public async Task<ActionResult<SendMessageResponse>> AddMessageToConversation(string conversationId, Message message)
        {
            try
            {
                SendMessageResponse response = await _messageService.PostMessageToConversation(conversationId, message);
                return CreatedAtAction(nameof(GetMessage), new { conversationId, messageId = message.Id }, response);
            }
            catch (ConversationDoesNotExist ex)
            {
                return BadRequest(ex.Message);
            }
            catch (SenderDoesNotExist ex)
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
    }

}
