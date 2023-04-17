using ChatService.Web.Storage;
using Moq;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Net.Http;
using ChatService.Web.Services;
using Azure.Core;
using ChatService.Web.Controllers;
using System.Collections.Specialized;
using ChatService.Web.Dtos.Conversations;
using ChatService.Web.Dtos.Messages;
using ChatService.Web.Dtos.Profiles;
using ChatService.Web.Exceptions;

namespace ChatService.Web.Tests.Controllers
{
    public class ConversationControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly Mock<IConversationService> _conversationServiceMock = new();
        private readonly HttpClient _httpClient;

        public ConversationControllerTests(WebApplicationFactory<Program> factory)
        {
            _httpClient = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services => { services.AddSingleton(_conversationServiceMock.Object); });
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
            StartConversationRequest request = null;
            _conversationServiceMock.Setup(x => x.CreateConversation(request))
                            .ThrowsAsync(expectedResponse);

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.Default, "application/json");
            var response = await _httpClient.PostAsync("/Conversations", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        [Fact]
        public async Task AddConversation_SenderDoesNotExist()
        {
            var expectedResponse = new SenderDoesNotExist("foo");
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
        public async Task AddConversation_ReceiverDoesNotExist()
        {
            var expectedResponse = new SenderDoesNotExist("foo");
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
        public async Task AddConversation_ParticipantsInvalidParams()
        {
            var expectedResponse = new ParticipantsInvalidParams();
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
        public async Task AddConversation_NullMessage()
        {
            var expectedResponse = new NullMessage();
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
        public async Task AddConversation_InvalidMessageParams()
        {
            var expectedResponse = new InvalidMessageParams();
            var message = new Message("1", "fel", "faa");
            StartConversationRequest request = null;
            _conversationServiceMock.Setup(x => x.CreateConversation(request))
                            .ThrowsAsync(expectedResponse);

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.Default, "application/json");
            var response = await _httpClient.PostAsync("/Conversations", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
