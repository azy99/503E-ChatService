using ChatService.Web.Dtos.Conversations;
using ChatService.Web.Dtos.Messages;
using ChatService.Web.Dtos.Profiles;
using ChatService.Web.Exceptions;
using ChatService.Web.Services;
using ChatService.Web.Storage;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatService.Web.Tests.Service
{
    public class MessageServiceTests: IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly Mock<IConversationStore> _conversationStorageMock = new();
        private readonly Mock<IMessageStore> _messageStorageMock = new();
        private readonly Mock<IProfileStore> _profileStorageMock = new();
        private readonly MessageService _messageService;
        public MessageServiceTests() 
        {
            _messageService = new MessageService(_conversationStorageMock.Object, _messageStorageMock.Object, _profileStorageMock.Object);
        }
        [Fact]
        public async Task GetMessage()
        {
            var userMessage = new UserMessage("foo1_foo2", "1", "Faa", "foo1", 123);
            _messageStorageMock.Setup(x => x.GetMessage("1","foo1_foo2")).ReturnsAsync(userMessage);
            var result = await _messageService.GetMessage("1", "foo1_foo2");
            Assert.Equal(userMessage, result);
            _messageStorageMock.Verify(mock => mock.GetMessage("1", "foo1_foo2"), Times.Once);
        }
        [Fact]
        public async Task GetMessage_MessageNotFound()
        {
            var userMessage = new UserMessage("foo1_foo2", "1", "Faa", "foo1", 123);
            var result = await _messageService.GetMessage("1", "foo1_foo2");
            Assert.Null(result);
            _messageStorageMock.Verify(mock => mock.GetMessage("1", "foo1_foo2"), Times.Once);
        }
        [Fact]
        public async Task AddMessage_SenderDoesNotExist()
        {
            var message = new Message("1", "foo1", "foo1");
            var ConversationId = "foo1_foo2";
            var expectedResponse = new SenderDoesNotExist(message.SenderUsername);
            var functionCall = _messageService.PostMessageToConversation(ConversationId,message);
            
            var exception = await Assert.ThrowsAsync<SenderDoesNotExist>(async () => await functionCall);
            Assert.Equal(expectedResponse.Message, exception.Message);
        }
        [Fact]
        public async Task AddMessage_ConversationDoesNotExist()
        {
            var senderProfile = new Profile("foo1", "foo1", "fa", "fay");
            var message = new Message("1", senderProfile.Username, "foo1");
            var ConversationId = "foo1_foo2";
            _profileStorageMock.Setup(mock => mock.GetProfile(message.SenderUsername))
                .ReturnsAsync(senderProfile);
            var expectedResponse = new ConversationDoesNotExist(ConversationId);
            var functionCall = _messageService.PostMessageToConversation(ConversationId, message);

            var exception = await Assert.ThrowsAsync<ConversationDoesNotExist>(async () => await functionCall);
            Assert.Equal(expectedResponse.Message, exception.Message);
        }
        [Fact]
        public async Task AddMessage_NullMessage()
        {
            Message message = null;
            var ConversationId = "foo1_foo2";
            var expectedResponse = new NullMessage();
            var functionCall = _messageService.PostMessageToConversation(ConversationId, message);

            var exception = await Assert.ThrowsAsync<NullMessage>(async () => await functionCall);
            Assert.Equal(expectedResponse.Message, exception.Message);
        }
        [Theory]
        [InlineData("","foo","bar")]
        [InlineData(null, "foo", "bar")]
        [InlineData("foo", "", "bar")]
        [InlineData("foo", null, "bar")]
        [InlineData("foo", "bar", "")]
        [InlineData("foo", "bar", null)]
        public async Task AddMessage_InvalidMessageParams(string Id,string SenderUsername,string Text)
        {
            var message = new Message(Id, SenderUsername, Text);
            var ConversationId = "foo1_foo2";
            var expectedResponse = new InvalidMessageParams();
            var functionCall = _messageService.PostMessageToConversation(ConversationId, message);

            var exception = await Assert.ThrowsAsync<InvalidMessageParams>(async () => await functionCall);
            Assert.Equal(expectedResponse.Message, exception.Message);
        }

    }
}
