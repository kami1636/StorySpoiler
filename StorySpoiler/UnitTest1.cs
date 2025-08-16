using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using StorySpoiler.Models;
using System.Net;
using System.Text.Json;

namespace StorySpoiler
{
    [TestFixture]

    public class Tests
    {
        private RestClient client;
        private static string CreateStoryId;
        private static string baseUrl = "https://d3s5nxhwblsjbi.cloudfront.net";
        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJwtToken("kami16", "kam123");
            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(options);
        }
        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseUrl);

            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { username, password });

            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            return json.GetProperty("accessToken").GetString();

            
        }

        [Test,Order(1)]
        public void CreateNewStpry_ShouldReturnOK()
        {
            var Story = new
            {
                Title = "Story 1",
                Description = "This is a new story.",
                Url = ""
            };
            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(Story);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            CreateStoryId = json.GetProperty("storyId").GetString();

            Assert.That(response.Content, Does.Contain(CreateStoryId));

            var createResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(createResponse.Msg, Is.EqualTo("Successfully created!"));

        }
        [Test, Order(2)]
        public void EditStory_ShouldReturnOK()
        {
            var EditStory = new StoryDTO
            {
                Title = "Edited Story",
                Description = "This is an edited story.",
                Url = ""
            };
            var request = new RestRequest($"/api/Story/Edit/{CreateStoryId}", Method.Put);
            request.AddJsonBody(EditStory);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(editResponse.Msg, Is.EqualTo("Successfully edited"));
        }
        [Test,Order(3)]
        public void GetAllStories_ShouldReturnOK()
        {
          var request = new RestRequest("/api/Story/All", Method.Get);
          var response = client.Execute(request);

          Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

          var stories = JsonSerializer.Deserialize<List<StoryDTO>>(response.Content);
          Assert.That(stories, Is.Not.Empty);

        }
        [Test, Order(4)]
        public void DeleteStory_ShouldReturnOK() 
        { 
            var request = new RestRequest($"/api/Story/Delete/{CreateStoryId}", Method.Delete);
            var response = client.Execute(request);
            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var deleteResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(deleteResponse.Msg, Is.EqualTo("Deleted successfully!"));
        }
        [Test, Order(5)]
        public void CreateStory_WithoutReqiredFields_ShouldReturnBadRequest()
        {
            var Story = new
            {
                Title = "",
                Description = "",
                Url = ""
            };
            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(Story);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        }
        [Test,Order(6)]
        public void EditStory_NonExistingStory_ShouldReturnNotFound()
        {
            string fakeStoryId = "123456789";
            var EditStory = new StoryDTO
            {
                Title = "Non-Existing Story",
                Description = "Non-Exsisting Story descripton",
                Url = ""
            };
            var request = new RestRequest($"/api/Story/Edit/{fakeStoryId}", Method.Put);
            request.AddJsonBody(EditStory);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

            var notFoundResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(notFoundResponse.Msg, Is.EqualTo("No spoilers..."));
        }
        [Test, Order(7)]
        public void DeteleteStory_NonExsistingStory_ShouldReturnBadRequest()
        {
            string fakeStoryId = "123456789";
            var request = new RestRequest($"/api/Story/Delete/{fakeStoryId}", Method.Delete);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var FakeDeleteResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(FakeDeleteResponse.Msg, Is.EqualTo("Unable to delete this story spoiler!"));
        }
        [OneTimeTearDown]
        public void Cleanup()
        {
            client?.Dispose();
        }
    }
}