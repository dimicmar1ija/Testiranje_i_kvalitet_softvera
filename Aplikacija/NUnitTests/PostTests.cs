using NUnit.Framework;
using Newtonsoft.Json;
using System.Net;
using MongoDB.Bson;

namespace NUnitTests;

[TestFixture]
public class PostTests
{
    private TestApiClient _api = null!;

    [SetUp]
    public void Setup()
    {
        var baseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:5000";
        _api = new TestApiClient(baseUrl);
    }

    [Test]
    public async Task GetAll_ReturnsOk_AndHasSeedData()
    {
        var resp = await _api.Http.GetAsync("/api/Post");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var json = await resp.Content.ReadAsStringAsync();
        Assert.That(json, Is.Not.Null.And.Not.Empty);

        var arr = JsonConvert.DeserializeObject<List<dynamic>>(json);
        Assert.That(arr, Is.Not.Null);
        Assert.That(arr!.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetById_Unknown_Returns404()
    {
        var resp = await _api.Http.GetAsync("/api/Post/ffffffffffffffffffffffff");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Create_Then_GetById_Works()
    {
        var newPost = new
        {
            Id = ObjectId.GenerateNewId().ToString(),
            AuthorId = "68b6bb8fd167b59b158205bd",
            Title = "NUnit create post",
            Body = "Post created from NUnit test",
            MediaUrls = new List<string>(),
            TagsIds = new List<string>(),
            LikedByUserIds = new List<string>(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createResp = await _api.Http.PostAsync("/api/Post", TestApiClient.JsonContent(newPost));

        var body = await createResp.Content.ReadAsStringAsync();
        TestContext.WriteLine(body);

        Assert.That(createResp.StatusCode, Is.AnyOf(HttpStatusCode.Created, HttpStatusCode.OK));

        dynamic created = JsonConvert.DeserializeObject<dynamic>(body)!;
        string id = (string)(created.id ?? created.Id);

        Assert.That(id, Is.Not.Null.And.Not.Empty);

        var getResp = await _api.Http.GetAsync($"/api/Post/{id}");
        Assert.That(getResp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task ToggleLike_AddsThenRemoves()
    {
        var all = await _api.Http.GetStringAsync("/api/Post");
        var posts = JsonConvert.DeserializeObject<List<dynamic>>(all)!;
        Assert.That(posts.Count, Is.GreaterThan(0));

        string postId = (string)(posts[0].id ?? posts[0].Id);
        string userId = "68b1c138fb185dbf99a07ef7";

        var likeResp = await _api.Http.PutAsync($"/api/Post/{postId}/like?userId={userId}", null);
        Assert.That(likeResp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var unlikeResp = await _api.Http.PutAsync($"/api/Post/{postId}/like?userId={userId}", null);
        Assert.That(unlikeResp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Search_NoTags_ReturnsAll()
    {
        var resp = await _api.Http.GetAsync("/api/Post/search");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}
