using Azure.Core;
using ChatService.Web.Dtos.Conversations;
using ChatService.Web.Dtos.Messages;
using ChatService.Web.Dtos.Profiles;
using ChatService.Web.Exceptions;
using ChatService.Web.Services;
using ChatService.Web.Storage;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ChatService.Web.Tests.Service
{
    public class ConversationServiceTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly Mock<IConversationStore> _conversationStorageMock = new();
        private readonly Mock<IMessageStore> _messageStorageMock = new();
        private readonly Mock<IProfileStore> _profileStorageMock = new();
        private readonly ConversationService _conversationService;
        private readonly ValidationManager _validationManager;
        public ConversationServiceTests()
        {
            _validationManager = new ValidationManager(_profileStorageMock.Object, _conversationStorageMock.Object);
            _conversationService = new ConversationService(_conversationStorageMock.Object, _messageStorageMock.Object, _profileStorageMock.Object, _validationManager);
        }
        [Fact]
        public async Task GetConversation()
        {
            var UserConversation = new UserConversation("foo1_foo2", DateTimeOffset.UtcNow.ToUnixTimeSeconds(), "foo1", "foo2");
            _conversationStorageMock.Setup(x => x.GetConversation("foo1_foo2")).ReturnsAsync(UserConversation);

            var result = await _conversationService.GetConversation("foo1_foo2");
            Assert.Equal(UserConversation, result);
            _conversationStorageMock.Verify(mock => mock.GetConversation("foo1_foo2"), Times.Once);
        }
        [Fact]
        public async Task GetConversation_NotFound()
        {
            var message = new Message("1", "fel", "faa");
            var UserConversationRequest = new UserConversation("foo1_foo2", DateTimeOffset.UtcNow.ToUnixTimeSeconds(), "foo1", "foo2");
            var result = await _conversationService.GetConversation("foo1_foo2");

            Assert.Null(result);
            _conversationStorageMock.Verify(mock => mock.GetConversation("foo1_foo2"), Times.Once);
            _conversationStorageMock.Verify(mock => mock.AddConversation(UserConversationRequest), Times.Never);
        }
        [Fact]
        public async Task AddConversation()
        {
            var message = new Message("1", "fel", "faa");
            var request = new StartConversationRequest(new string[] { "foo1", "foo2" }, message);
            var UserConversationRequest = new UserConversation("foo1_foo2", DateTimeOffset.UtcNow.ToUnixTimeSeconds(), "foo1", "foo2");
            var expectedResponse = new StartConversationResponse("foo1_foo2", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            _conversationStorageMock.Setup(x => x.AddConversation(UserConversationRequest)).ReturnsAsync(expectedResponse);
            _profileStorageMock.Setup(x => x.GetProfile("foo1")).ReturnsAsync(new Profile("foo1", "foo1", "a", "1"));
            _profileStorageMock.Setup(x => x.GetProfile("foo2")).ReturnsAsync(new Profile("foo2", "foo2", "a", "1"));
            var result = await _conversationService.CreateConversation(request);

            Assert.Equal(expectedResponse, result);
            _conversationStorageMock.Verify(mock => mock.AddConversation(UserConversationRequest), Times.Once);
        }
        [Fact]
        public async Task AddConversation_NullConversation()
        {
            var message = new Message("1", "fel", "faa");
            StartConversationRequest request = null;
            var expectedResponse = new NullStartConversationRequestException();
            var functionCall = _conversationService.CreateConversation(request);

            var exception = await Assert.ThrowsAsync<NullStartConversationRequestException>(async () => await functionCall);
            Assert.Equal(expectedResponse.Message, exception.Message);
        }
        [Theory]
        [InlineData(null)]
        [InlineData("foo")]
        [InlineData("foo","foo1","foo2")]
        public async Task AddConversation_ConversationNotTwoPeople(params string[] Participants)
        {
            var message = new Message("1", "fel", "faa");
            var request = new StartConversationRequest(Participants, message);
            var expectedResponse = new ConversationNotTwoPeople();

            var functionCall = _conversationService.CreateConversation(request);
            var exception = await Assert.ThrowsAsync<ConversationNotTwoPeople>(async () => await functionCall);

            Assert.Equal(expectedResponse.Message, exception.Message);
        }
        [Fact]
        public async Task AddConversation_SenderNotFound()
        {
            var message = new Message("1", "fel", "faa");
            var request = new StartConversationRequest(new string[] { "foo1", "foo2" }, message);
            var expectedResponse = new SenderDoesNotExist("foo1");
            var functionCall = _conversationService.CreateConversation(request);

            var exception = await Assert.ThrowsAsync<SenderDoesNotExist>(async () => await functionCall);
            Assert.Equal(expectedResponse.Message, exception.Message);
        }
        [Fact]
        public async Task AddConversation_ReceiverNotFound()
        {
            var message = new Message("1", "fel", "faa");
            var request = new StartConversationRequest(new string[] { "foo1", "foo2" }, message);
            var expectedResponse = new ReceiverDoesNotExist("foo2");
            _profileStorageMock.Setup(x => x.GetProfile("foo1")).ReturnsAsync(new Profile("foo1", "foo1", "a", "1"));
            var functionCall = _conversationService.CreateConversation(request);

            var exception = await Assert.ThrowsAsync<ReceiverDoesNotExist>(async () => await functionCall);
            Assert.Equal(expectedResponse.Message, exception.Message);
        }
        [Theory]
        [InlineData("","foo")]
        [InlineData(null, "foo")]
        [InlineData("foo", "")]
        [InlineData("foo", null)]
        [InlineData("", "")]
        [InlineData(null, null)]
        public async Task AddConversation_ParticipantsSenderOrReceiversIsNullOrEmpty(string Sender,string Receiver)
        {
            var message = new Message("1", "fel", "faa");
            var request = new StartConversationRequest(new string[]{ Sender,Receiver }, message);
            var expectedResponse = new ParticipantsInvalidParams();
            var profile = new Profile(Receiver, "foo", "a", "1");
            _profileStorageMock.Setup(x => x.GetProfile(Sender)).ReturnsAsync(profile);
            _profileStorageMock.Setup(x => x.GetProfile(Receiver)).ReturnsAsync(profile);
            var functionCall = _conversationService.CreateConversation(request);
            var exception = await Assert.ThrowsAsync<ParticipantsInvalidParams>(async () => await functionCall);
            Assert.Equal(expectedResponse.Message, exception.Message);
        }
        [Fact]
        public async Task AddConversation_NullMessage()
        {
            var message = new Message("1", "fel", "faa");
            var request = new StartConversationRequest(new string[] { "foo1", "foo2" }, null);
            var expectedResponse = new NullMessage();
            _profileStorageMock.Setup(x => x.GetProfile("foo1")).ReturnsAsync(new Profile("foo1", "foo1", "a", "1"));
            _profileStorageMock.Setup(x => x.GetProfile("foo2")).ReturnsAsync(new Profile("foo2", "foo1", "a", "1"));
            var functionCall = _conversationService.CreateConversation(request);

            var exception = await Assert.ThrowsAsync<NullMessage>(async () => await functionCall);
            Assert.Equal(expectedResponse.Message, exception.Message);
        }
        [Theory]
        [InlineData(null,"foo","foo")]
        [InlineData("", "foo", "foo")]
        [InlineData("foo", null, "foo")]
        [InlineData("foo", "foo", null)]
        [InlineData("foo", "foo", "")]
        [InlineData(null, null, "foo")]
        [InlineData("foo", null, null)]
        [InlineData(null, "foo", null)]
        [InlineData("", "", "foo")]
        [InlineData("foo", "", "")]
        [InlineData("", "foo", "")]
        [InlineData("", null, "foo")]
        [InlineData("foo", null, "")]
        [InlineData(null, "foo", "")]
        [InlineData(null, "" ,"foo")]
        [InlineData("foo", "", null)]
        [InlineData("", "foo", null)]
        [InlineData("", "", "")]
        [InlineData(null, null, null)]
        public async Task AddConversation_InvalidMessageArgs( string messageId, string MessageSenderUsername, string Text)
        {
            var message = new Message(messageId, MessageSenderUsername, Text);
            var request = new StartConversationRequest(new string[] { "foo", "foo" }, message);
            var expectedResponse = new InvalidMessageParams();
            _profileStorageMock.Setup(x => x.GetProfile("foo")).ReturnsAsync(new Profile("foo", "foo", "a", "1"));
            var functionCall = _conversationService.CreateConversation(request);

            var exception = await Assert.ThrowsAsync<InvalidMessageParams>(async () => await functionCall);
            Assert.Equal(expectedResponse.Message, exception.Message);
        }
        [Fact]
        public async Task EnumerateConversations()
        {
            var senderProfile = new Profile("foofoo", "foo", "bar", "1");
            var recipientProfile = new Profile("foobar", "bar", "foo", "2");
            var conversation = new Conversation(senderProfile.Username + "_" + recipientProfile.Username, 1, recipientProfile);
            var limit = 10;
            var continuationToken = "x";
            var lastSeenConversationTime = 1;
            var conversations = new Conversation[] { conversation };
            var enumerateConversations = new EnumerateConversations(continuationToken, lastSeenConversationTime, conversations);
            _profileStorageMock.Setup(p => p.GetProfile(senderProfile.Username)).ReturnsAsync(senderProfile);
            _conversationStorageMock.Setup(x => x.EnumerateConversations(senderProfile.Username,continuationToken, limit, lastSeenConversationTime))
                .ReturnsAsync(enumerateConversations);
            var result = await _conversationService.EnumerateConversations(senderProfile.Username,continuationToken, limit, lastSeenConversationTime);
            Assert.Equal(enumerateConversations, result);
        }
        [Fact]
        public async Task EnumerateConversations_ProfileDoesNotExist()
        {
            var senderProfile = new Profile("foofoo", "foo", "bar", "1");
            var expectedResponse = new SenderDoesNotExist(senderProfile.Username);
            var limit = 10;
            var continuationToken = "x";
            var lastSeenConversationTime = 1;
            var functionCall = _conversationService.EnumerateConversations(senderProfile.Username, continuationToken, limit, lastSeenConversationTime);
            var result = await Assert.ThrowsAsync<SenderDoesNotExist>(async () => await functionCall);
            Assert.Equal(expectedResponse.Message, result.Message);
        }
        [Fact]
        public async Task EnumerateConversationMessages()
        {
            var conversation = new UserConversation("foo_bar", 2, "foo", "bar");
            var message = new ConversationMessage("Hi", "foo", 1);
            var limit = 10;
            var continuationToken = "x";
            var lastSeenMessageTime = 1;
            var conversationMessages = new ConversationMessage[]{message};

            var enumerateConversationMessages = new EnumerateConversationMessages(continuationToken, lastSeenMessageTime,conversationMessages);
            _conversationStorageMock.Setup(x => x.GetConversation(conversation.Id)).ReturnsAsync(conversation);
            _conversationStorageMock.Setup(x => x.EnumerateConversationMessages(conversation.Id,continuationToken,limit,lastSeenMessageTime))
                .ReturnsAsync(enumerateConversationMessages);
            var result = await _conversationService.EnumerateConversationMessages(conversation.Id, continuationToken, limit, lastSeenMessageTime);
            Assert.Equal(enumerateConversationMessages, result);
        }

        [Fact]
        public async Task EnumerateConversationMessages_ConversationDoesNotExist()
        {
            var conversation = new UserConversation("foo_bar", 2, "foo", "bar");
            var expectedResponse = new ConversationDoesNotExist(conversation.Id);
            UserConversation notFound = null;
            var result = await _conversationService.GetConversation("foo1_foo2");
            Assert.Null(result);

            var functionCall = _conversationService.EnumerateConversationMessages(conversation.Id, null, null, 1);
            var exception = await Assert.ThrowsAsync<ConversationDoesNotExist>(async () => await functionCall);
            Assert.Equal(expectedResponse.Message, exception.Message);
        }

    }
}
