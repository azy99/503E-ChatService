using ChatService.Web.Dtos.Conversations;
using ChatService.Web.Dtos.Messages;
using ChatService.Web.Dtos.Profiles;
using ChatService.Web.Exceptions;
using ChatService.Web.Storage;

namespace ChatService.Web.Services
{
    public class ValidationManager
    {
        private readonly IProfileStore _profileStore;
        private readonly IConversationStore _conversationStore;
        public ValidationManager( IProfileStore profileStore, IConversationStore conversationStore)
        {
            _profileStore = profileStore;
            _conversationStore = conversationStore;
        }
        public async Task ValidateConversation(StartConversationRequest request)
        {
            if (request == null)
            {
                throw new NullStartConversationRequestException();
            }

            if (request.Participants == null || request.Participants.Length< 2 || request.Participants.Length > 2 )
            {
                throw new ConversationNotTwoPeople();
            }
            try
            {
                await CheckIfSenderExists(request.Participants[0]);
                await CheckIfReceiverExists(request.Participants[1]);

                await ValidateMessage(request.FirstMessage, true, request.Participants[0] +"_"+ request.Participants[1]);
            }catch (SenderDoesNotExist ex)
            {
                throw ex;
            }
            catch(ReceiverDoesNotExist ex)
            {
                throw ex;
            }
            catch (ParticipantsInvalidParams ex)
            {
                throw ex;
            }
            catch (NullMessage ex)
            {
                throw ex;
            }
            catch (InvalidMessageParams ex)
            {
                throw ex;
            }
            

        }
        public async Task ValidateMessage(Message message, bool isFirstMessage,string conversationId)
        {
            if (message == null)
            {
                throw new NullMessage();
            }
            if (string.IsNullOrEmpty(message.Id) ||
               string.IsNullOrEmpty(message.SenderUsername) ||
               string.IsNullOrEmpty(message.Text))
            {
                throw new InvalidMessageParams();
            }
            if (!isFirstMessage)
            {
                await CheckIfSenderExists(message.SenderUsername);
                await CheckIfConversationExists(conversationId);
            }
            
        }
        public async Task CheckIfSenderExists(string senderUsername)
        {
            if (string.IsNullOrEmpty(senderUsername))
            {
                throw new ParticipantsInvalidParams();
            }
            var sender = await _profileStore.GetProfile(senderUsername);
            if (sender == null)
            {
                throw new SenderDoesNotExist(senderUsername);
            }
        }
        public async Task CheckIfReceiverExists(string receiverUsername)
        {
            if (string.IsNullOrEmpty(receiverUsername))
            {
                throw new ParticipantsInvalidParams();
            }
            var recipient = await _profileStore.GetProfile(receiverUsername);
            if (recipient == null)
            {
                throw new ReceiverDoesNotExist(receiverUsername);
            }
        }
        public async Task CheckIfConversationExists(string conversationId)
        {
            if (string.IsNullOrEmpty(conversationId))
            {
                throw new NullConversation();
            }
            var conversation = await _conversationStore.GetConversation(conversationId);
            if (conversation == null)
            {
                throw new ConversationDoesNotExist(conversationId);
            }
        }
    }
}
