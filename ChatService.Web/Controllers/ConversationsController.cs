using ChatService.Web.Dtos.Conversations;
using ChatService.Web.Dtos.Messages;
using ChatService.Web.Exceptions;
using ChatService.Web.Services;
using ChatService.Web.Storage;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConversationsController : ControllerBase { 
        private readonly IConversationService _conversationService;
        private readonly IMessageService _messageService;
        private readonly IProfileStore _profileStore;
        public ConversationsController(IConversationService conversationService, IMessageService messageService, IProfileStore profileStore)
        {
            _conversationService = conversationService;
            _messageService = messageService;
            _profileStore = profileStore;
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
            //catch(NullStartConversationRequestException ex)
            //{
            //    return BadRequest(ex.Message);
            //}
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
            //catch (NullMessage ex)
            //{
            //    return BadRequest(ex.Message);
            //}
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
            //catch (NullMessage ex)
            //{
            //    return BadRequest(ex.Message);
            //}
            catch (InvalidMessageParams ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        public async Task<ActionResult<EnumerateConversationsResponse>> EnumerateConversations(string username,
            string? continuationToken, int? limit, long? lastSeenConversationTime)
        {
            var existingProfile = await _profileStore.GetProfile(username);
            if (existingProfile == null)
            {
                return NotFound($"A User with username {username} was not found");
            }

            var response = await _conversationService.EnumerateConversations(username,continuationToken, limit, lastSeenConversationTime);

            return Ok(response);
        }

        [HttpGet("{conversationId}/messages")]
        public async Task<ActionResult<EnumerateConversationMessagesResponse>> EnumerateConversationMessages(string conversationId, string? continuationToken, int? limit,
            long? lastSeenMessageTime)
        {
            var conv = await _conversationService.GetConversation(conversationId);
            if (conv == null)
            {
                return NotFound($"A Conversation with id {conversationId} was not found");
            }

            var response = await _conversationService.EnumerateConversationMessages(conversationId, continuationToken,
                limit, lastSeenMessageTime);
            return Ok(response);

        }

    }

}
