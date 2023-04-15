﻿using Azure.Core;
using ChatService.Web.Dtos.Conversations;
using ChatService.Web.Dtos.Messages;
using ChatService.Web.Dtos.Profiles;
using ChatService.Web.Exceptions;
using ChatService.Web.Storage;

namespace ChatService.Web.Services
{
    //TODO Different DTOs between layers: controller -> service -> storage to add logic in service layer
    public class ConversationService : IConversationService
    {
        private readonly IConversationStore _conversationStore;
        private readonly IMessageStore _messageStore;
        private readonly IValidationManager _validationManager;
        public ConversationService(IConversationStore conversationStore, IMessageStore messageStore, IValidationManager validationManager)
        {
            _conversationStore = conversationStore;
            _messageStore = messageStore;
            _validationManager = validationManager;
        }
        public async Task<StartConversationResponse> CreateConversation(StartConversationRequest request)
        {
            _validationManager.ValidateConversation(request);

            UserMessage message = new (
                request.FirstMessage.Id,
                request.FirstMessage.Text,
                request.FirstMessage.SenderUsername,
                UnixTime : DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                );

            //CAN GET RECIPIENT IN GET BY SPLITTING ID
            UserConversation conversation1 = new (
                Id : request.Participants[0] + "_" + request.Participants[1],
                LastModifiedUnixTime : DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                );
            UserConversation conversation2 = new(
                Id: request.Participants[1] + "_" + request.Participants[2],
                LastModifiedUnixTime: DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                );

            var addMessageTask = _messageStore.AddMessage(message);
            var addConversation1Task = _conversationStore.AddConversation(conversation1);
            var addConversation2Task = _conversationStore.AddConversation(conversation2);

            await Task.WhenAll( addMessageTask, addConversation1Task, addConversation2Task );

            return addConversation1Task.Result;
        }

        public Task<UserConversation?> GetConversation(string conversationID)
        {
            throw new NotImplementedException();
        }
    }
}
