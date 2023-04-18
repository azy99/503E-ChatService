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

            var response = await _httpClient.GetAsync($"/Conversations/{conversation.Sender + "_" + conversation.Receiver}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(conversation, JsonConvert.DeserializeObject<UserConversation>(json));
        }

        [Fact]
        public async Task GetConversation_NotFound()
        {
            _conversationServiceMock.Setup(m => m.GetConversation("foobar"))
                .ReturnsAsync((UserConversation?)null);

            var response = await _httpClient.GetAsync($"/Conversations/foobar");
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

            var response = await _httpClient.PostAsync("/Conversations", content);

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
            var response = await _httpClient.PostAsync("/Conversations", content);

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
            var response = await _httpClient.PostAsync("/Conversations", content);
            
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
            var response = await _httpClient.PostAsync("/Conversations", content);

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
            var response = await _httpClient.PostAsync("/Conversations", content);

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
            var response = await _httpClient.PostAsync("/Conversations", content);

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
            var response = await _httpClient.PostAsync("/Conversations", content);

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
            var response = await _httpClient.PostAsync("/Conversations", content);

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
            var response = await _httpClient.GetAsync("/Conversations/foo1_foo2/1");
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
            var response = await _httpClient.GetAsync("/Conversations/foo1_foo2/1");
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

            var response = await _httpClient.PostAsync("/Conversations/foo1_foo2/messages", content);

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
            var response = await _httpClient.PostAsync($"/Conversations/{conversationId}/messages", content);

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
            var response = await _httpClient.PostAsync($"/Conversations/{conversationId}/messages", content);

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
            var response = await _httpClient.PostAsync($"/Conversations/{conversationId}/messages", content);

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
            var response = await _httpClient.PostAsync($"/Conversations/{conversationId}/messages", content);

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
            var response = await _httpClient.PostAsync($"/Conversations/{conversationId}/messages", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task EnumerateConversations()
        {
            var recipient = new Profile("foobar", "Foo", "Bar","5b0fa492-3271-4131-bb6b-519c263d6c7b");
            var conversation = new Conversation(Id: "abcdef", LastModifiedUnixTime: 2, Recipient: recipient);
            var expectedResponse = new EnumerateConversationsResponse(Conversations:new List<Conversation>{conversation}, NextUri: "/api/conversations/?username=foobar&");
            _profileStoreMock.Setup(m => m.GetProfile("foo")).ReturnsAsync(recipient);
            _conversationServiceMock.Setup(m => m.EnumerateConversations("foo",null,null,null)).ReturnsAsync(expectedResponse);
            var response =
                await _httpClient.GetAsync($"/Conversations?username=foo");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task EnumerateConversations_UserNotFound()
        {
            var recipient = new Profile("foobar", "Foo", "Bar","5b0fa492-3271-4131-bb6b-519c263d6c7b");
            var conversation = new Conversation(Id: "abcdef", LastModifiedUnixTime: 2, Recipient: recipient);
            var expectedResponse = new EnumerateConversationsResponse(Conversations:new List<Conversation>{conversation}, NextUri: "/api/conversations/?username=foobar&");
            _conversationServiceMock.Setup(m => m.EnumerateConversations("foo",null,null,null)).ReturnsAsync(expectedResponse);
            var response = await _httpClient.GetAsync($"/Conversations?username=foo");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task EnumerateConversationMessages()
        {
            var message = new ConversationMessage(Text: "hello", SenderUsername: "foo", UnixTime: 1);
            var expectedResponse = new EnumerateConversationMessagesResponse(
                Messages: new List<ConversationMessage> { message }, NextUri: "/api/conversations/foo_bar/messages?");
            var conversation = new UserConversation("foo_bar", 123,"foo","bar");
            _conversationServiceMock.Setup(m => m.GetConversation("foo_bar")).ReturnsAsync(conversation);
            _conversationServiceMock.Setup(m => m.EnumerateConversationMessages("foo_bar", null, null, null))
                .ReturnsAsync(expectedResponse);
            var response = await _httpClient.GetAsync($"/Conversations/foo_bar/messages");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task EnumerateConversationMessages_ConversationNotFound()
        {
            var message = new ConversationMessage(Text: "hello", SenderUsername: "foo", UnixTime: 1);
            var expectedResponse = new EnumerateConversationMessagesResponse(
                Messages: new List<ConversationMessage> { message }, NextUri: "/api/conversations/foo_bar/messages?");
            _conversationServiceMock.Setup(m => m.EnumerateConversationMessages("foo_bar", null, null, null))
                .ReturnsAsync(expectedResponse);
            var response = await _httpClient.GetAsync($"/Conversations/foo_bar/messages");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
