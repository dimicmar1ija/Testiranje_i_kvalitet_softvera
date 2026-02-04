using NUnit.Framework;
using Newtonsoft.Json;
using System.Net;

namespace NUnitTests;

[TestFixture]
public class CommentTests
{
    private TestApiClient _api = null!;

    [SetUp]
    public void Setup()
    {
        var baseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:5000";
        _api = new TestApiClient(baseUrl);
    }

    [Test]
    public async Task GetThreadedComments_ReturnsOk()
    {
        var all = await _api.Http.GetStringAsync("/api/Post");
        var posts = JsonConvert.DeserializeObject<List<dynamic>>(all)!;

        Assert.That(posts, Is.Not.Null);
        Assert.That(posts.Count, Is.GreaterThan(0));

        string postId = (string)(posts[0].id ?? posts[0].Id);

        var resp = await _api.Http.GetAsync($"/api/Comment/post/{postId}/threaded");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task CreateComment_Works()
    {
        var all = await _api.Http.GetStringAsync("/api/Post");
        var posts = JsonConvert.DeserializeObject<List<dynamic>>(all)!;
        Assert.That(posts.Count, Is.GreaterThan(0));

        string postId = (string)(posts[0].id ?? posts[0].Id);

        var dto = new
        {
            PostId = postId,
            AuthorId = "68b6bb8fd167b59b158205bd",
            ParentCommentId = (string?)null,
            Body = "NUnit comment"
        };

        var resp = await _api.Http.PostAsync("/api/Comment", TestApiClient.JsonContent(dto));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var json = await resp.Content.ReadAsStringAsync();
        Assert.That(json, Does.Contain("NUnit comment"));
    }

    [Test]
    public async Task DeleteComment_RequiresAuth_Unauthorized()
    {
        var resp = await _api.Http.DeleteAsync("/api/Comment/ffffffffffffffffffffffff");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task DeleteComment_AsOwner_Works()
    {
        var uname = "nunit_" + Guid.NewGuid().ToString("N")[..8];
        var email = uname + "@test.com";
        var pass = "Test123!";

        var (token, userId) = await _api.RegisterAndLoginAsync(uname, email, pass);
        _api.SetBearer(token);

        var all = await _api.Http.GetStringAsync("/api/Post");
        var posts = JsonConvert.DeserializeObject<List<dynamic>>(all)!;
        Assert.That(posts.Count, Is.GreaterThan(0));

        string postId = (string)(posts[0].id ?? posts[0].Id);

        var createDto = new
        {
            PostId = postId,
            AuthorId = userId,
            ParentCommentId = (string?)null,
            Body = "Comment to delete"
        };

        var createResp = await _api.Http.PostAsync("/api/Comment", TestApiClient.JsonContent(createDto));
        Assert.That(createResp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var createdJson = await createResp.Content.ReadAsStringAsync();
        dynamic created = JsonConvert.DeserializeObject<dynamic>(createdJson)!;
        string commentId = (string)(created.id ?? created.Id);

        var del = await _api.Http.DeleteAsync($"/api/Comment/{commentId}");
        Assert.That(del.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }
}
