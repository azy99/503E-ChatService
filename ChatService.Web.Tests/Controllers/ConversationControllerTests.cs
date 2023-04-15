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


        //[Fact]
        //public async Task GetConversation()
        //{
        //    var conversation = new userConversation("foo", "faa");
        //    _conversationStoreMock.Setup(m => m.GetConversation(conversation.Sender+"_"+conversation.Receiver))
        //    .ReturnsAsync(conversation);

        //    var response = await _httpClient.GetAsync($"/Conversation/{profile.Username}");
        //    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        //    var json = await response.Content.ReadAsStringAsync();
        //    Assert.Equal(profile, JsonConvert.DeserializeObject<Profile>(json));
        //}

        [Fact]
        public async Task AddConversation()
        {
            var expectedResponse = new StartConversationResponse("123", 12345678);
            Message message = new Message("1", "fel", "faa");
            var request = new StartConversationRequest(new List<string> { "foo1", "foo2" }, message);
            _conversationServiceMock.Setup(x => x.CreateConversation(It.IsAny<StartConversationRequest>()))
                            .ReturnsAsync(expectedResponse);

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.Default, "application/json");

            var response = await _httpClient.PostAsync("/Conversation", content);

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            _conversationServiceMock.Verify(mock => mock.CreateConversation(It.IsAny<StartConversationRequest>()), Times.Once);
        }



    }
}
