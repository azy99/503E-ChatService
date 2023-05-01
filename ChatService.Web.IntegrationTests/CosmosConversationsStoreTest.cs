using ChatService.Web.Dtos.Conversations;
using ChatService.Web.Dtos.Profiles;
using ChatService.Web.Exceptions;
using ChatService.Web.Storage;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatService.Web.IntegrationTests
{
    public class CosmosConversationsStoreTest : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
    {
        private readonly IConversationStore _conversationStore;
        string _guid;
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
            _guid = Guid.NewGuid().ToString();
            _conversationStore = factory.Services.GetRequiredService<IConversationStore>();
            _conversation = new(
            Id: _guid + "_" + Guid.NewGuid().ToString(),
            LastModifiedUnixTime: 1,
            Sender: _guid,
            Receiver: "Bar"
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
    }
}
