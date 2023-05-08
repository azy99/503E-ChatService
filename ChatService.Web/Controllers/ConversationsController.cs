using ChatService.Web.Dtos.Conversations;
using ChatService.Web.Dtos.Messages;
using ChatService.Web.Exceptions;
using ChatService.Web.Services;
using ChatService.Web.Storage;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using System.Net;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;

namespace ChatService.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class conversationsController : ControllerBase { 
        private readonly IConversationService _conversationService;
        private readonly IMessageService _messageService;
        private readonly ILogger _logger;
        private readonly TelemetryClient _telemetryClient;
        public conversationsController(IConversationService conversationService, IMessageService messageService, IProfileStore profileStore, ILogger<conversationsController> logger, TelemetryClient telemetryClient)
        {
            _conversationService = conversationService;
            _messageService = messageService;
            _logger = logger;
            _telemetryClient = telemetryClient;
        }
        [HttpGet("{conversationId}")]
        public async Task<ActionResult<UserConversation>> GetConversation(string conversationId)
        {
            var conversation = await _conversationService.GetConversation(conversationId);
            _telemetryClient.TrackEvent("GotConversation");
            if (conversation == null)
            {
                _logger.LogWarning("A Conversation with conversationID {conversationId} was not found", conversationId);
                return NotFound($"A Conversation with conversationID {conversationId} was not found");
            }
            return Ok(conversation);
        }

        [HttpPost]
        public async Task<ActionResult<StartConversationResponse>> AddConversation(StartConversationRequest request)
        {
            try
            {
                StartConversationResponse response = await _conversationService.CreateConversation(request);
                _telemetryClient.TrackEvent("ConversationAdded");
                return CreatedAtAction(nameof(GetConversation), new { conversationId = response.Id }, response);

            }
            //catch(NullStartConversationRequestException ex)
            //{
            //    return BadRequest(ex.Message);
            //}
            catch (ConversationNotTwoPeople ex)
            {
                return BadRequest(ex.Message);
            }
            catch (SenderDoesNotExist ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ReceiverDoesNotExist ex)
            {
                var participants = request.Participants.ToString();
                _logger.LogWarning("A Recipient from the list of participants: {participants} was not found", participants);
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
                _logger.LogWarning("A Conversation with conversationID {conversationId} was not found", conversationId);
                return NotFound($"A Conversation with conversationID {conversationId} was not found");
            }
            return Ok(conversation);
        }
        [HttpPost("{conversationId}/messages")]
        public async Task<ActionResult<SendMessageResponse>> AddMessageToConversation(string conversationId, Message message)
        {
            try
            {
                SendMessageResponse response = await _messageService.PostMessageToConversation(conversationId, message);
                _telemetryClient.TrackEvent("MessageAdded");
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
            catch (CosmosException ex)
            {
                if (ex.StatusCode == HttpStatusCode.Conflict)
                {
                    return Conflict(ex.Message);
                }
                throw ex;
            }
        }

        [HttpGet]
        public async Task<ActionResult<EnumerateConversationsResponse>> EnumerateConversations(string username,
            string? continuationToken, int? limit, long? lastSeenConversationTime)
        {
            try
            {
                var response = await _conversationService.EnumerateConversations(username, continuationToken, limit, lastSeenConversationTime);
                _telemetryClient.TrackEvent("EnumeratedConversations");
                string nextUri;
                if (string.IsNullOrWhiteSpace(response.continuationToken))
                {
                    nextUri = "";
                }
                else
                {
                    nextUri = $"/api/conversations?username={username}&";
                    if (limit != null && limit != 0)
                    {
                        nextUri += $"limit={limit}&";
                    }
                    if (response.lastSeenConversationTime != null && response.lastSeenConversationTime != 0)
                    {
                        nextUri += $"lastSeenConversationTime={response.lastSeenConversationTime}&";
                    }
                    if (!string.IsNullOrEmpty(response.continuationToken))
                    {
                        var continuationTokenEncoded = WebUtility.UrlEncode(response.continuationToken);
                        nextUri += $"continuationToken={continuationTokenEncoded}";
                    }
                }
                return Ok(new EnumerateConversationsResponse(response.Conversations, nextUri));
            }
            catch(SenderDoesNotExist ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{conversationId}/messages")]
        public async Task<ActionResult<EnumerateConversationMessagesResponse>> EnumerateConversationMessages(string conversationId, string? continuationToken, int? limit,
            long? lastSeenMessageTime)
        {
            try
            {
                var response = await _conversationService.EnumerateConversationMessages(conversationId, continuationToken,
                    limit, lastSeenMessageTime);
                _telemetryClient.TrackEvent("EnumeratedMessages");
                string nextUri;
                if (string.IsNullOrWhiteSpace(response.continuationToken))
                {
                    nextUri = "";
                }
                else
                {
                    nextUri = $"/api/conversations/{conversationId}/messages?&";
                    if (limit != null && limit != 0)
                    {
                        nextUri += $"limit={limit}&";
                    }
                    if (response.lastSeenMessageTime != null && response.lastSeenMessageTime != 0)
                    {
                        nextUri += $"lastSeenMessageTime={lastSeenMessageTime}&";
                    }
                    if (!string.IsNullOrEmpty(response.continuationToken))
                    {
                        var continuationTokenEncoded = WebUtility.UrlEncode(response.continuationToken);
                        nextUri += $"continuationToken={continuationTokenEncoded}";
                    }
                }
                return Ok(new EnumerateConversationMessagesResponse(response.ConversationMessages, nextUri));
            }
            catch (ConversationDoesNotExist ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
