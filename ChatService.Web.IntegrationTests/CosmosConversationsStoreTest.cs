using ChatService.Web.Dtos.Conversations;
using ChatService.Web.Dtos.Messages;
using ChatService.Web.Dtos.Profiles;
using ChatService.Web.Exceptions;
using ChatService.Web.Storage;
using ChatService.Web.Storage.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ChatService.Web.IntegrationTests
{
    public class CosmosConversationsStoreTest : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
    {
        private readonly IConversationStore _conversationStore;
        private readonly IMessageStore _messageStore;
        string _guidSender, _guidReceiver;
        private readonly UserConversation _conversation;

        public async Task DisposeAsync()
        {
            await _conversationStore.DeleteConversation(_conversation.Sender,_conversation.Id);
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }
        public CosmosConversationsStoreTest(WebApplicationFactory<Program> factory)
        {
            _guidSender = Guid.NewGuid().ToString();
            _guidReceiver = Guid.NewGuid().ToString();
            _conversationStore = factory.Services.GetRequiredService<IConversationStore>();
            _messageStore = factory.Services.GetRequiredService<IMessageStore>();
            _conversation = new(
            Id: _guidSender + "_" + _guidReceiver,
            LastModifiedUnixTime: 1,
            Sender: _guidSender,
            Receiver: _guidReceiver
            );
        }
        [Fact]
        public async Task AddNewConversation()
        {
            var response = await _conversationStore.AddConversation(_conversation);
            Assert.Equal(_conversation, await _conversationStore.GetConversation(_conversation.Id));
            Assert.Equal(response, new StartConversationResponse( _conversation.Id,1 ));
        }
        [Fact]
        public async Task AddNewConversationAlreadyExists()
        {
            var response = await _conversationStore.AddConversation(_conversation);
            response = await _conversationStore.AddConversation(_conversation);
            Assert.Equal(_conversation, await _conversationStore.GetConversation(_conversation.Id));
            Assert.Equal(response, new StartConversationResponse(_conversation.Id, 1));
        }
        [Fact]
        public async Task GetNonExistingConversation()
        {
            Assert.Null(await _conversationStore.GetConversation(_conversation.Id));
        }

        [Fact]
        public async Task UpdateConversation()
        {
            await _conversationStore.AddConversation(_conversation);
            var updatedConversation = new UserConversation(
                Id: _conversation.Id,
                LastModifiedUnixTime: 2,
                Sender: _conversation.Sender,
                Receiver: _conversation.Receiver
                );
            await _conversationStore.UpsertConversation(updatedConversation);
            Assert.Equal(updatedConversation, await _conversationStore.GetConversation(_conversation.Id));
        }
        [Fact]
        public async Task DeleteConversationNotFound()
        {
                await _conversationStore.DeleteConversation(_conversation.Sender, _conversation.Id);
        }

        [Fact]
        public async Task EnumerateConversationMessages()
        {
            UserConversation userConversation = new UserConversation (
                Id: _conversation.Id,
                LastModifiedUnixTime: 2,
                Sender: _conversation.Sender,
                Receiver: _conversation.Receiver
                );
            var conversation = await _conversationStore.AddConversation(userConversation);
            UserMessage firstMessage = new UserMessage(userConversation.Id, "1", "Hello", userConversation.Sender, 12346);
            UserMessage secondMessage = new UserMessage(userConversation.Id, "2", "How are you?", userConversation.Sender, 12347);
            UserMessage thirdMessage = new UserMessage(userConversation.Id, "3", "I am fine", userConversation.Receiver, 12348);
            UserMessage fourthMessage = new UserMessage(userConversation.Id, "4", "How about you", userConversation.Receiver, 12349);
            await _messageStore.AddMessage(firstMessage);
            await _messageStore.AddMessage(secondMessage);
            await _messageStore.AddMessage(thirdMessage);
            await _messageStore.AddMessage(fourthMessage);
            var limit = 2;
            ConversationMessage firstConversationMessage = new ConversationMessage(firstMessage.Text, firstMessage.SenderUsername, firstMessage.UnixTime);
            ConversationMessage secondConversationMessage = new ConversationMessage(secondMessage.Text, secondMessage.SenderUsername, secondMessage.UnixTime);
            ConversationMessage thirdConversationMessage = new ConversationMessage(thirdMessage.Text, thirdMessage.SenderUsername, thirdMessage.UnixTime);
            ConversationMessage fourthConversationMessage = new ConversationMessage(fourthMessage.Text, fourthMessage.SenderUsername, fourthMessage.UnixTime);
            var expectedMessages = new ConversationMessage[]{ fourthConversationMessage,thirdConversationMessage };

            var response = await _conversationStore.EnumerateConversationMessages(userConversation.Id, null, limit, null);
            Assert.Equal(response.ConversationMessages, expectedMessages);
            Assert.Equal(response.lastSeenMessageTime, thirdConversationMessage.UnixTime);
            expectedMessages = new ConversationMessage[] { secondConversationMessage, firstConversationMessage };
            response = await _conversationStore.EnumerateConversationMessages(userConversation.Id, response.continuationToken, limit, response.lastSeenMessageTime);
            Assert.Equal(response.ConversationMessages, expectedMessages);
            Assert.Equal(response.lastSeenMessageTime, firstConversationMessage.UnixTime);
        }
    }
}
