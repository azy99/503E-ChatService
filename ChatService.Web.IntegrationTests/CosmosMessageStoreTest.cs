using ChatService.Web.Dtos.Conversations;
using ChatService.Web.Dtos.Messages;
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
    public class CosmosMessageStoreTest : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
    {
        private readonly IMessageStore _messageStore;
        string _guid;
        private readonly UserMessage _message;

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }
        public async Task DisposeAsync()
        {
            await _messageStore.DeleteMessage(_message.Id, _message.ConversationId);
        }
        public CosmosMessageStoreTest(WebApplicationFactory<Program> factory)
        {
            _guid = Guid.NewGuid().ToString();
            _messageStore = factory.Services.GetRequiredService<IMessageStore>();
            _message = new(
            ConversationId: _guid + "_" + Guid.NewGuid().ToString(),
            Id: Guid.NewGuid().ToString(),
            Text: "Foo",
            SenderUsername: _guid,
            UnixTime: 1
            ) ;
        }
        [Fact]
        public async Task AddNewMessage() 
        {
            var response = await _messageStore.AddMessage(_message);
            Assert.Equal(_message, await _messageStore.GetMessage(_message.Id,_message.ConversationId));
            Assert.Equal(response, new SendMessageResponse(1));
        }
        [Fact]
        public async Task AddExistingMessage()
        {
            await _messageStore.AddMessage(_message);
            var response = await _messageStore.AddMessage(_message);
            Assert.Equal(_message, await _messageStore.GetMessage(_message.Id, _message.ConversationId));
            Assert.Equal(response, new SendMessageResponse(1));
        }
        [Fact]
        public async Task GetNonExistingMessage()
        {
            Assert.Null(await _messageStore.GetMessage(_message.Id, _message.ConversationId));
        }
        [Fact]
        public async Task DeleteMessageNotFound()
        {
            await _messageStore.DeleteMessage(_message.Id, _message.ConversationId);
        }
    }
}
