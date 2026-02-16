using NUnit.Framework;
using Newtonsoft.Json;
using System.Net;
using MongoDB.Bson;

namespace NUnitTests;

[TestFixture]
public class CommentApiTests
{
    private TestApiClient _api = null!;

    [SetUp]
    public void Setup()
    {
        var baseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:5000";
        _api = new TestApiClient(baseUrl);
    }

    private async Task<(string postId, string authorId)> CreatePostAsync()
    {
        var (_, userId) = await _api.EnsureAuthedAsync();

        var payload = new
        {
            id = ObjectId.GenerateNewId().ToString(),
            authorId = userId,
            title = "Post for comment " + Guid.NewGuid().ToString("N")[..6],
            body = "Body",
            mediaUrls = new List<string>(),
            tagsIds = new List<string>(),
            likedByUserIds = new List<string>(),
            createdAt = DateTime.UtcNow,
            updatedAt = DateTime.UtcNow
        };

        var resp = await _api.Http.PostAsync("/api/Post", TestApiClient.JsonContent(payload));
        var txt = await resp.Content.ReadAsStringAsync();
        Assert.That(resp.StatusCode, Is.AnyOf(HttpStatusCode.Created, HttpStatusCode.OK));

        dynamic created = JsonConvert.DeserializeObject<dynamic>(txt)!;
        string id = (string)(created.id ?? created.Id);
        return (id, userId);
    }

    private async Task<(string commentId, string postId, string ownerId)> CreateCommentAsync(string? body = null)
    {
        var (postId, ownerId) = await CreatePostAsync();

        var dto = new
        {
            postId = postId,
            parentCommentId = (string?)null,
            body = body ?? ("Comment " + Guid.NewGuid().ToString("N")[..6])
        };

        var resp = await _api.Http.PostAsync("/api/Comment", TestApiClient.JsonContent(dto));
        var txt = await resp.Content.ReadAsStringAsync();
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        dynamic created = JsonConvert.DeserializeObject<dynamic>(txt)!;
        string cid = (string)(created.id ?? created.Id);
        return (cid, postId, ownerId);
    }

    [Test]
    public async Task Comment_Threaded_Unauthorized_WithoutToken()
    {
        var resp = await _api.Http.GetAsync("/api/Comment/post/ffffffffffffffffffffffff/threaded");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Comment_Threaded_Ok_EmptyOrNot()
    {
        await _api.EnsureAuthedAsync();
        var resp = await _api.Http.GetAsync("/api/Comment/post/ffffffffffffffffffffffff/threaded");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Comment_Threaded_Ok_AfterCreate_ContainsBody()
    {
        var (_, postId, _) = await CreateCommentAsync("Threaded body");
        var resp = await _api.Http.GetAsync($"/api/Comment/post/{postId}/threaded");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var json = await resp.Content.ReadAsStringAsync();
        Assert.That(json, Does.Contain("Threaded body"));
    }

    [Test]
    public async Task Comment_Create_Unauthorized_WithoutToken()
    {
        var dto = new { postId = "x", parentCommentId = (string?)null, body = "b" };
        var resp = await _api.Http.PostAsync("/api/Comment", TestApiClient.JsonContent(dto));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Comment_Create_Ok_WithToken()
    {
        var (cid, _, _) = await CreateCommentAsync("Create works");
        Assert.That(cid, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public async Task Comment_Create_BadRequest_MissingPostId()
    {
        await _api.EnsureAuthedAsync();
        var dto = new { body = "no postId" };
        var resp = await _api.Http.PostAsync("/api/Comment", TestApiClient.JsonContent(dto));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Comment_Delete_Unauthorized_WithoutToken()
    {
        var resp = await _api.Http.DeleteAsync("/api/Comment/ffffffffffffffffffffffff");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Comment_Delete_NotFound_UnknownId()
    {
        await _api.EnsureAuthedAsync();
        var resp = await _api.Http.DeleteAsync("/api/Comment/ffffffffffffffffffffffff");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Comment_Delete_NoContent_OwnerDeletes()
    {
        var (cid, _, _) = await CreateCommentAsync("To delete");
        var resp = await _api.Http.DeleteAsync($"/api/Comment/{cid}");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task Comment_Update_Unauthorized_WithoutToken()
    {
        var dto = new { body = "upd" };
        var resp = await _api.Http.PutAsync("/api/Comment/ffffffffffffffffffffffff", TestApiClient.JsonContent(dto));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Comment_Update_NotFound_UnknownId()
    {
        await _api.EnsureAuthedAsync();
        var dto = new { body = "upd" };
        var resp = await _api.Http.PutAsync("/api/Comment/ffffffffffffffffffffffff", TestApiClient.JsonContent(dto));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Comment_Update_NoContent_OwnerUpdates()
    {
        var (cid, _, _) = await CreateCommentAsync("Original");
        var dto = new { body = "UPDATED" };

        var resp = await _api.Http.PutAsync($"/api/Comment/{cid}", TestApiClient.JsonContent(dto));

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task Comment_Like_Unauthorized_WithoutToken()
    {
        var resp = await _api.Http.PostAsync("/api/Comment/ffffffffffffffffffffffff/like/u1", null);
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Comment_Like_Ok_FirstTime()
    {
        await _api.EnsureAuthedAsync();
        var (cid, _, _) = await CreateCommentAsync();

        var resp = await _api.Http.PostAsync($"/api/Comment/{cid}/like/u1", null);
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Comment_Like_BadRequest_SecondTime()
    {
        await _api.EnsureAuthedAsync();
        var (cid, _, _) = await CreateCommentAsync();

        var r1 = await _api.Http.PostAsync($"/api/Comment/{cid}/like/u1", null);
        Assert.That(r1.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var r2 = await _api.Http.PostAsync($"/api/Comment/{cid}/like/u1", null);
        Assert.That(r2.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Comment_Dislike_Unauthorized_WithoutToken()
    {
        var resp = await _api.Http.PostAsync("/api/Comment/ffffffffffffffffffffffff/dislike/u1", null);
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Comment_Dislike_Ok_FirstTime()
    {
        await _api.EnsureAuthedAsync();
        var (cid, _, _) = await CreateCommentAsync();

        var resp = await _api.Http.PostAsync($"/api/Comment/{cid}/dislike/u1", null);
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Comment_Dislike_BadRequest_SecondTime()
    {
        await _api.EnsureAuthedAsync();
        var (cid, _, _) = await CreateCommentAsync();

        var r1 = await _api.Http.PostAsync($"/api/Comment/{cid}/dislike/u1", null);
        Assert.That(r1.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var r2 = await _api.Http.PostAsync($"/api/Comment/{cid}/dislike/u1", null);
        Assert.That(r2.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Comment_Unlike_Unauthorized_WithoutToken()
    {
        var resp = await _api.Http.PostAsync("/api/Comment/ffffffffffffffffffffffff/unlike/u1", null);
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Comment_Unlike_BadRequest_WhenNotLikedYet()
    {
        await _api.EnsureAuthedAsync();
        var (cid, _, _) = await CreateCommentAsync();

        var resp = await _api.Http.PostAsync($"/api/Comment/{cid}/unlike/u1", null);
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Comment_Unlike_Ok_AfterLike()
    {
        await _api.EnsureAuthedAsync();
        var (cid, _, _) = await CreateCommentAsync();

        var like = await _api.Http.PostAsync($"/api/Comment/{cid}/like/u1", null);
        Assert.That(like.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var unlike = await _api.Http.PostAsync($"/api/Comment/{cid}/unlike/u1", null);
        Assert.That(unlike.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Comment_Undislike_Unauthorized_WithoutToken()
    {
        var resp = await _api.Http.PostAsync("/api/Comment/ffffffffffffffffffffffff/undislike/u1", null);
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Comment_Undislike_BadRequest_WhenNotDislikedYet()
    {
        await _api.EnsureAuthedAsync();
        var (cid, _, _) = await CreateCommentAsync();

        var resp = await _api.Http.PostAsync($"/api/Comment/{cid}/undislike/u1", null);
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Comment_Undislike_Ok_AfterDislike()
    {
        await _api.EnsureAuthedAsync();
        var (cid, _, _) = await CreateCommentAsync();

        var dislike = await _api.Http.PostAsync($"/api/Comment/{cid}/dislike/u1", null);
        Assert.That(dislike.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var undislike = await _api.Http.PostAsync($"/api/Comment/{cid}/undislike/u1", null);
        Assert.That(undislike.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}