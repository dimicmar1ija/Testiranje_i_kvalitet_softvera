using NUnit.Framework;
using Newtonsoft.Json;
using System.Net;
using MongoDB.Bson;

namespace NUnitTests;

[TestFixture]
public class PostApiTests
{
    private TestApiClient _api = null!;

    [SetUp]
    public void Setup()
    {
        var baseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:5000";
        _api = new TestApiClient(baseUrl);
    }

    private async Task<(string postId, string authorId)> CreatePostAsync(string? title = null, string? body = null)
    {
        var (_, userId) = await _api.EnsureAuthedAsync();

        var payload = new
        {
            id = ObjectId.GenerateNewId().ToString(),     // OBAVEZNO kod tebe
            authorId = userId,
            title = title ?? ("NUnit post " + Guid.NewGuid().ToString("N")[..6]),
            body = body ?? "Body from NUnit",
            mediaUrls = new List<string>(),
            tagsIds = new List<string>(),
            likedByUserIds = new List<string>(),
            createdAt = DateTime.UtcNow,
            updatedAt = DateTime.UtcNow
        };

        var resp = await _api.Http.PostAsync("/api/Post", TestApiClient.JsonContent(payload));
        var txt = await resp.Content.ReadAsStringAsync();
        TestContext.WriteLine(txt);
        Assert.That(resp.StatusCode, Is.AnyOf(HttpStatusCode.Created, HttpStatusCode.OK));

        dynamic created = JsonConvert.DeserializeObject<dynamic>(txt)!;
        string id = (string)(created.id ?? created.Id);
        return (id, userId);
    }

    [Test]
    public async Task Post_GetAll_Unauthorized_WithoutToken()
    {
        var resp = await _api.Http.GetAsync("/api/Post");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Post_GetAll_Ok_WithToken()
    {
        await _api.EnsureAuthedAsync();
        var resp = await _api.Http.GetAsync("/api/Post");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Post_GetAll_Ok_ReturnsJsonArray()
    {
        await _api.EnsureAuthedAsync();
        var resp = await _api.Http.GetAsync("/api/Post");
        var json = await resp.Content.ReadAsStringAsync();

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var arr = JsonConvert.DeserializeObject<List<dynamic>>(json);
        Assert.That(arr, Is.Not.Null);
    }

    [Test]
    public async Task Post_GetById_Unauthorized_WithoutToken()
    {
        var resp = await _api.Http.GetAsync("/api/Post/ffffffffffffffffffffffff");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Post_GetById_NotFound_UnknownId()
    {
        await _api.EnsureAuthedAsync();
        var resp = await _api.Http.GetAsync("/api/Post/ffffffffffffffffffffffff");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Post_GetById_Ok_AfterCreate()
    {
        var (postId, _) = await CreatePostAsync();
        var resp = await _api.Http.GetAsync($"/api/Post/{postId}");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Post_Create_Unauthorized_WithoutToken()
    {
        var payload = new
        {
            id = ObjectId.GenerateNewId().ToString(),
            authorId = "x",
            title = "t",
            body = "b",
            mediaUrls = new List<string>(),
            tagsIds = new List<string>(),
            likedByUserIds = new List<string>(),
            createdAt = DateTime.UtcNow,
            updatedAt = DateTime.UtcNow
        };

        var resp = await _api.Http.PostAsync("/api/Post", TestApiClient.JsonContent(payload));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Post_Create_Created_WithToken()
    {
        var (postId, _) = await CreatePostAsync();
        Assert.That(postId, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public async Task Post_Create_BadRequest_MissingId()
    {
        await _api.EnsureAuthedAsync();

        var payload = new
        {
            authorId = "x",
            title = "t",
            body = "b",
            mediaUrls = new List<string>(),
            tagsIds = new List<string>(),
            likedByUserIds = new List<string>(),
            createdAt = DateTime.UtcNow,
            updatedAt = DateTime.UtcNow
        };

        var resp = await _api.Http.PostAsync("/api/Post", TestApiClient.JsonContent(payload));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Post_Update_Unauthorized_WithoutToken()
    {
        var payload = new { id = ObjectId.GenerateNewId().ToString(), title = "x", body = "y" };
        var resp = await _api.Http.PutAsync("/api/Post", TestApiClient.JsonContent(payload));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Post_Update_NotFound_UnknownId()
    {
        await _api.EnsureAuthedAsync();

        var payload = new
        {
            id = ObjectId.GenerateNewId().ToString(),
            authorId = "someone",
            title = "upd",
            body = "upd",
            mediaUrls = new List<string>(),
            tagsIds = new List<string>(),
            likedByUserIds = new List<string>(),
            createdAt = DateTime.UtcNow,
            updatedAt = DateTime.UtcNow
        };

        var resp = await _api.Http.PutAsync("/api/Post", TestApiClient.JsonContent(payload));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Post_Update_Ok_AfterCreate()
    {
        var (postId, authorId) = await CreatePostAsync();
        var payload = new
        {
            id = postId,
            authorId = authorId,
            title = "UPDATED",
            body = "UPDATED BODY",
            mediaUrls = new List<string>(),
            tagsIds = new List<string>(),
            likedByUserIds = new List<string>(),
            createdAt = DateTime.UtcNow.AddMinutes(-1),
            updatedAt = DateTime.UtcNow
        };

        var resp = await _api.Http.PutAsync("/api/Post", TestApiClient.JsonContent(payload));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Post_Delete_Unauthorized_WithoutToken()
    {
        var resp = await _api.Http.DeleteAsync("/api/Post/ffffffffffffffffffffffff");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Post_Delete_NotFound_UnknownId()
    {
        await _api.EnsureAuthedAsync();
        var resp = await _api.Http.DeleteAsync("/api/Post/ffffffffffffffffffffffff");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Post_Delete_NoContent_AfterCreate()
    {
        var (postId, _) = await CreatePostAsync();
        var resp = await _api.Http.DeleteAsync($"/api/Post/{postId}");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task Post_LikePost_Unauthorized_WithoutToken()
    {
        var resp = await _api.Http.PostAsync("/api/Post/ffffffffffffffffffffffff/like?userId=abc", null);
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Post_LikePost_BadRequest_MissingUserId()
    {
        await _api.EnsureAuthedAsync();
        var (postId, _) = await CreatePostAsync();

        var resp = await _api.Http.PostAsync($"/api/Post/{postId}/like", null);
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Post_LikePost_Ok_WithUserId()
    {
        await _api.EnsureAuthedAsync();
        var (postId, _) = await CreatePostAsync();

        var resp = await _api.Http.PostAsync($"/api/Post/{postId}/like?userId=u1", null);
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Post_ToggleLike_Unauthorized_WithoutToken()
    {
        var resp = await _api.Http.PutAsync("/api/Post/ffffffffffffffffffffffff/like?userId=abc", null);
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Post_ToggleLike_NotFound_UnknownPost()
    {
        await _api.EnsureAuthedAsync();
        var resp = await _api.Http.PutAsync($"/api/Post/ffffffffffffffffffffffff/like?userId=u1", null);
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Post_ToggleLike_Ok_AddsThenRemoves()
    {
        await _api.EnsureAuthedAsync();
        var (postId, _) = await CreatePostAsync();

        var r1 = await _api.Http.PutAsync($"/api/Post/{postId}/like?userId=u1", null);
        Assert.That(r1.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var r2 = await _api.Http.PutAsync($"/api/Post/{postId}/like?userId=u1", null);
        Assert.That(r2.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Post_ByAuthor_Unauthorized_WithoutToken()
    {
        var resp = await _api.Http.GetAsync("/api/Post/by-author/someone");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

   [Test]
    public async Task Post_ByAuthor_Ok_ReturnsOk()
    {
        var (_, authorId) = await CreatePostAsync(); // kreira post i vrati authorId
        var resp = await _api.Http.GetAsync($"/api/Post/by-author/{authorId}");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Post_ByAuthor_Ok_AfterCreate_ContainsPost()
    {
        var (postId, authorId) = await CreatePostAsync();
        var resp = await _api.Http.GetAsync($"/api/Post/by-author/{authorId}");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var json = await resp.Content.ReadAsStringAsync();
        Assert.That(json, Does.Contain(postId));
    }

    [Test]
    public async Task Post_ByTag_Unauthorized_WithoutToken()
    {
        var resp = await _api.Http.GetAsync("/api/Post/by-tag/sometag");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Post_ByTag_Ok_ReturnsOk()
    {
        await _api.EnsureAuthedAsync();
        var resp = await _api.Http.GetAsync("/api/Post/by-tag/sometag");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Post_ByTag_Ok_UnknownTag_ReturnsOkMaybeEmpty()
    {
        await _api.EnsureAuthedAsync();
        var resp = await _api.Http.GetAsync("/api/Post/by-tag/ffffffffffffffffffffffff");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Post_Search_Unauthorized_WithoutToken()
    {
        var resp = await _api.Http.GetAsync("/api/Post/search");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Post_Search_Ok_NoTags_ReturnsOk()
    {
        await _api.EnsureAuthedAsync();
        var resp = await _api.Http.GetAsync("/api/Post/search");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Post_Search_Ok_WithTagsIds_ReturnsOk()
    {
        await _api.EnsureAuthedAsync();
        var resp = await _api.Http.GetAsync("/api/Post/search?tagsIds=tag1,tag2&match=any");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Post_ByAuthor_BadRequest_InvalidAuthorId()
    {
        await _api.EnsureAuthedAsync();
        var resp = await _api.Http.GetAsync("/api/Post/by-author/someone");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}