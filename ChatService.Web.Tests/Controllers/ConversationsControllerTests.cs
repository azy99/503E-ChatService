using ChatService.Web.Storage;
using Moq;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.TestHost;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using ChatService.Web.Services;
using ChatService.Web.Dtos.Conversations;
using ChatService.Web.Dtos.Messages;
using ChatService.Web.Dtos.Profiles;
using ChatService.Web.Exceptions;
using System.Net.Http;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Message = ChatService.Web.Dtos.Messages.Message;

namespace ChatService.Web.Tests.Controllers
{
    public class ConversationsControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly Mock<IMessageService> _messageServiceMock = new();
        private readonly Mock<IConversationService> _conversationServiceMock = new();
        private readonly Mock<IProfileStore> _profileStoreMock = new();
        private readonly HttpClient _httpClient;

        public ConversationsControllerTests(WebApplicationFactory<Program> factory)
        {
            _httpClient = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services => { services.AddSingleton(_conversationServiceMock.Object); });
                builder.ConfigureTestServices(services => { services.AddSingleton(_messageServiceMock.Object); });
                builder.ConfigureTestServices(services => { services.AddSingleton(_profileStoreMock.Object); });
            }).CreateClient();
        }
        [Fact]
        public async Task GetConversation()
        {
            var conversation = new UserConversation("foo", 123,"fa","faa");
            _conversationServiceMock.Setup(m => m.GetConversation(conversation.Sender + "_" + conversation.Receiver))
            .ReturnsAsync(conversation);

            var response = await _httpClient.GetAsync($"api/Conversations/{conversation.Sender + "_" + conversation.Receiver}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(conversation, JsonConvert.DeserializeObject<UserConversation>(json));
        }

        [Fact]
        public async Task GetConversation_NotFound()
        {
            _conversationServiceMock.Setup(m => m.GetConversation("foobar"))
                .ReturnsAsync((UserConversation?)null);

            var response = await _httpClient.GetAsync($"api/Conversations/foobar");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AddConversation()
        {
            var expectedResponse = new StartConversationResponse("123", 12345678910);
            var message = new Message("1", "fel", "faa");
            var request = new StartConversationRequest(new string[] {"foo1", "foo2" }, message);
            _conversationServiceMock.Setup(x => x.CreateConversation(It.IsAny<StartConversationRequest>()))
                            .ReturnsAsync(expectedResponse);

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.Default, "application/json");

            var response = await _httpClient.PostAsync("api/Conversations", content);

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            _conversationServiceMock.Verify(mock => mock.CreateConversation(It.IsAny<StartConversationRequest>()), Times.Once);
        }
        [Fact]
        public async Task AddConversation_NullStartConversationRequest()
        {
            var expectedResponse = new NullStartConversationRequestException();
            var message = new Message("1", "fel", "faa");
            StartConversationRequest request = null;
            _conversationServiceMock.Setup(x => x.CreateConversation(request))
                            .ThrowsAsync(expectedResponse);
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.Default, "application/json");
            var response = await _httpClient.PostAsync("api/Conversations", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        [Fact]
        public async Task AddConversation_ConversationNotTwoPeople()
        {
            var expectedResponse = new ConversationNotTwoPeople();
            var message = new Message("1", "fel", "faa");
            StartConversationRequest request = new StartConversationRequest(new string[] { "foo" },message);
            _conversationServiceMock.Setup(x => x.CreateConversation(It.IsAny<StartConversationRequest>()))
                            .ThrowsAsync(expectedResponse);

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.Default, "application/json");
            var response = await _httpClient.PostAsync("api/Conversations", content);
            
            _conversationServiceMock.Verify(mock => mock.CreateConversation(It.IsAny<StartConversationRequest>()), Times.Once);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains(expectedResponse.Message, responseContent);
        }
        [Fact]
        public async Task AddConversation_SenderDoesNotExist()
        {
            var expectedResponse = new SenderDoesNotExist("foo");
            var message = new Message("1", "foo", "faa");
            StartConversationRequest request = new StartConversationRequest(new string[] { "foo","fa" }, message);
            _conversationServiceMock.Setup(x => x.CreateConversation(It.IsAny<StartConversationRequest>()))
                            .ThrowsAsync(expectedResponse);

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.Default, "application/json");
            var response = await _httpClient.PostAsync("api/Conversations", content);

            _conversationServiceMock.Verify(mock => mock.CreateConversation(It.IsAny<StartConversationRequest>()), Times.Once);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains(expectedResponse.Message, responseContent);
        }
        [Fact]
        public async Task AddConversation_ReceiverDoesNotExist()
        {
            var expectedResponse = new ReceiverDoesNotExist("fa");
            var message = new Message("1", "foo", "faa");
            StartConversationRequest request = new StartConversationRequest(new string[] { "foo", "fa" }, message);
            _conversationServiceMock.Setup(x => x.CreateConversation(It.IsAny<StartConversationRequest>()))
                            .ThrowsAsync(expectedResponse);

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.Default, "application/json");
            var response = await _httpClient.PostAsync("api/Conversations", content);

            _conversationServiceMock.Verify(mock => mock.CreateConversation(It.IsAny<StartConversationRequest>()), Times.Once);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains(expectedResponse.Message, responseContent);
        }
        [Fact]
        public async Task AddConversation_ParticipantsInvalidParams()
        {
            var expectedResponse = new ParticipantsInvalidParams();
            var message = new Message("1", "foo", "faa");
            StartConversationRequest request = new StartConversationRequest(new string[] { "", "fa" }, message);
            _conversationServiceMock.Setup(x => x.CreateConversation(It.IsAny<StartConversationRequest>()))
                            .ThrowsAsync(expectedResponse);

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.Default, "application/json");
            var response = await _httpClient.PostAsync("api/Conversations", content);

            _conversationServiceMock.Verify(mock => mock.CreateConversation(It.IsAny<StartConversationRequest>()), Times.Once);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains(expectedResponse.Message, responseContent);
        }
        [Fact]
        public async Task AddConversation_NullMessage()
        {
            var expectedResponse = new NullMessage();
            Message message = null;
            StartConversationRequest request = new StartConversationRequest(new string[] { "foo", "fa" }, message);
            _conversationServiceMock.Setup(x => x.CreateConversation(It.IsAny<StartConversationRequest>()))
                            .ThrowsAsync(expectedResponse);

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.Default, "application/json");
            var response = await _httpClient.PostAsync("api/Conversations", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        [Fact]
        public async Task AddConversation_InvalidMessageParams()
        {
            var expectedResponse = new InvalidMessageParams();
            var message = new Message("1", "fel", "faa");
            StartConversationRequest request = new StartConversationRequest(new string[] { "1"},message);
            _conversationServiceMock.Setup(x => x.CreateConversation(It.IsAny<StartConversationRequest>()))
                            .ThrowsAsync(expectedResponse);

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.Default, "application/json");
            var response = await _httpClient.PostAsync("api/Conversations", content);

            _conversationServiceMock.Verify(mock => mock.CreateConversation(It.IsAny<StartConversationRequest>()), Times.Once);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains(expectedResponse.Message, responseContent);
        }
        [Fact]
        public async Task GetMessage()
        {
            var expectedResponse = new UserMessage("foo1_foo2", "1", "fel", "faa", 12345678910);
            _messageServiceMock.Setup(x => x.GetMessage("1", "foo1_foo2"))
                            .ReturnsAsync(expectedResponse);
            var response = await _httpClient.GetAsync("api/Conversations/foo1_foo2/1");
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _messageServiceMock.Verify(mock => mock.GetMessage("1", "foo1_foo2"), Times.Once);
        }
        [Fact]
        public async Task GetMessage_NotFound()
        {
            UserMessage expectedResponse = null;
            _messageServiceMock.Setup(x => x.GetMessage("1", "foo1_foo2"))
                            .ReturnsAsync(expectedResponse);
            var response = await _httpClient.GetAsync("api/Conversations/foo1_foo2/1");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            _messageServiceMock.Verify(mock => mock.GetMessage("1", "foo1_foo2"), Times.Once);
        }
        [Fact]
        public async Task AddMessage()
        {
            var expectedResponse = new SendMessageResponse(12345678910);
            var message = new Message("1", "fel", "faa");
            _messageServiceMock.Setup(x => x.PostMessageToConversation("foo1_foo2", message))
                            .ReturnsAsync(expectedResponse);
            var json = JsonConvert.SerializeObject(message);
            var content = new StringContent(json, Encoding.Default, "application/json");

            var response = await _httpClient.PostAsync("api/Conversations/foo1_foo2/messages", content);

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            _messageServiceMock.Verify(mock => mock.PostMessageToConversation("foo1_foo2", message), Times.Once);
        }
        [Fact]
        public async Task AddMessage_ConversationDoesNotExist()
        {
            var conversationId = "foo1_foo2";
            var expectedResponse = new ConversationDoesNotExist(conversationId);
            var message = new Message("1", "fel", "faa");
            _messageServiceMock.Setup(x => x.PostMessageToConversation(conversationId, message))
                            .ThrowsAsync(expectedResponse);
            var json = JsonConvert.SerializeObject(message);
            var content = new StringContent(json, Encoding.Default, "application/json");
            var response = await _httpClient.PostAsync($"api/Conversations/{conversationId}/messages", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            _messageServiceMock.Verify(mock => mock.PostMessageToConversation(conversationId, message), Times.Once);
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains(expectedResponse.Message, responseContent);
        }
        [Fact]
        public async Task AddMessage_SenderDoesNotExist()
        {
            var conversationId = "foo1_foo2";
            var expectedResponse = new SenderDoesNotExist("fo");
            var message = new Message("1", "fo", "faa");
            _messageServiceMock.Setup(x => x.PostMessageToConversation(conversationId, message))
                            .ThrowsAsync(expectedResponse);
            var json = JsonConvert.SerializeObject(message);
            var content = new StringContent(json, Encoding.Default, "application/json");
            var response = await _httpClient.PostAsync($"api/Conversations/{conversationId}/messages", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            _messageServiceMock.Verify(mock => mock.PostMessageToConversation(conversationId, message), Times.Once);
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains(expectedResponse.Message, responseContent);
        }
        [Fact]
        public async Task AddMessage_NullMessage()
        {
            var conversationId = "foo1_foo2";
            var expectedResponse = new NullMessage();
            Message message = null;
            _messageServiceMock.Setup(x => x.PostMessageToConversation(conversationId, message))
                            .ThrowsAsync(expectedResponse);
            var json = JsonConvert.SerializeObject(message);
            var content = new StringContent(json, Encoding.Default, "application/json");
            var response = await _httpClient.PostAsync($"api/Conversations/{conversationId}/messages", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        [Fact]
        public async Task AddMessage_InvalidMessageParams()
        {
            var conversationId = "foo1_foo2";
            var expectedResponse = new InvalidMessageParams();
            Message message = new Message("1", "1", "1");
            _messageServiceMock.Setup(x => x.PostMessageToConversation(conversationId, message))
                            .ThrowsAsync(expectedResponse);
            var json = JsonConvert.SerializeObject(message);
            var content = new StringContent(json, Encoding.Default, "application/json");
            var response = await _httpClient.PostAsync($"api/Conversations/{conversationId}/messages", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            _messageServiceMock.Verify(mock => mock.PostMessageToConversation(conversationId, message), Times.Once);
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains(expectedResponse.Message, responseContent);
        }
        [Fact]
        public async Task AddMessage_NullConversation()
        {
            var conversationId = " ";
            var expectedResponse = new NullConversation();
            Message message = new Message("1", "1", "1");
            _messageServiceMock.Setup(x => x.PostMessageToConversation(conversationId, message))
                            .ThrowsAsync(expectedResponse);
            var json = JsonConvert.SerializeObject(message);
            var content = new StringContent(json, Encoding.Default, "application/json");
            var response = await _httpClient.PostAsync($"api/Conversations/{conversationId}/messages", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        [Fact]
        public async Task EnumerateConversations()
        {
            var senderProfile = new Profile("foofoo", "foo", "bar", "1");
            var recipientProfile = new Profile("foobar", "bar", "foo", "2");
            var conversation = new Conversation(senderProfile.Username+"_"+recipientProfile.Username, 1, recipientProfile);
            var enumerateResponse = new EnumerateConversations(
                continuationToken: "next",
                lastSeenConversationTime: conversation.LastModifiedUnixTime,
                Conversations: new Conversation[] { conversation }
                );

            var limit = 10;
            var lastSeenConversationTime = 1;
            var continuationToken = "toBeContinued";
            var conversations = new Conversation[] { conversation };

            _conversationServiceMock.Setup(m => m.EnumerateConversations(senderProfile.Username, continuationToken, limit, lastSeenConversationTime))
                .ReturnsAsync(enumerateResponse);
            var response = await _httpClient.GetAsync($"api/Conversations?username={senderProfile.Username}&limit={limit}&lastSeenConversationTime={lastSeenConversationTime}&continuationToken={continuationToken}");
            var responseContent = await response.Content.ReadAsStringAsync();
            var actualResponse = JsonConvert.DeserializeObject<EnumerateConversationMessagesResponse>(responseContent);
            var encodedToken = WebUtility.UrlEncode(enumerateResponse.continuationToken);
            var expectedNextUri = $"/api/conversations?username={senderProfile.Username}&limit={limit}&lastSeenConversationTime={enumerateResponse.lastSeenConversationTime}&continuationToken={encodedToken}";
            response.EnsureSuccessStatusCode();
            Assert.Equal(expectedNextUri, actualResponse.NextUri);
        }
        [Fact]
        public async Task EnumerateConversation_ProfileNotFound()
        {
            var senderProfile = new Profile("foo", "foo", "bar", "1");
            var expectedResponse = new SenderDoesNotExist(senderProfile.Username);
            _conversationServiceMock.Setup(m => m.EnumerateConversations("foo", null, null, null))
                .ThrowsAsync(expectedResponse);
            var response = await _httpClient.GetAsync($"api/Conversations?username={senderProfile.Username}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        [Fact]
        public async Task EnumerateConversationMessagesNextUriGeneration()
        {
            var message = new ConversationMessage(Text: "hello", SenderUsername: "foo", UnixTime: 123);
            var enumerateResponse = new EnumerateConversationMessages(
                continuationToken: "next",
                lastSeenMessageTime: message.UnixTime,
                ConversationMessages: new ConversationMessage[] { message }
                );

            var conversation = new UserConversation("foo_bar", 123,"foo","bar");
            var limit = 10;
            var lastSeenMessageTime = 123;
            var continuationToken = "toBeContinued";
            _conversationServiceMock.Setup(m => m.EnumerateConversationMessages(conversation.Id, continuationToken, limit, lastSeenMessageTime))
                .ReturnsAsync(enumerateResponse);
            var response = await _httpClient.GetAsync($"api/Conversations/{conversation.Id}/messages?&limit={limit}&lastSeenMessageTime={lastSeenMessageTime}&continuationToken={continuationToken}");
            var responseContent = await response.Content.ReadAsStringAsync();
            var actualResponse = JsonConvert.DeserializeObject<EnumerateConversationMessagesResponse>(responseContent);
            var encodedToken = WebUtility.UrlEncode(enumerateResponse.continuationToken);
            var expectedNextUri = $"/api/conversations/{conversation.Id}/messages?&limit={limit}&lastSeenMessageTime={enumerateResponse.lastSeenMessageTime}&continuationToken={encodedToken}";
            
            response.EnsureSuccessStatusCode();
            Assert.Equal(expectedNextUri, actualResponse.NextUri);
        }

        [Fact]
        public async Task EnumerateConversationMessages_ConversationNotFound()
        {
            var message = new ConversationMessage(Text: "hello", SenderUsername: "foo", UnixTime: 1);
            var conversationId = "foo_bar";
            var expectedResponse = new ConversationDoesNotExist(conversationId);
            _conversationServiceMock.Setup(m => m.EnumerateConversationMessages("foo_bar", null, null, null))
                .ThrowsAsync(expectedResponse);
            var response = await _httpClient.GetAsync($"api/Conversations/{conversationId}/messages");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
