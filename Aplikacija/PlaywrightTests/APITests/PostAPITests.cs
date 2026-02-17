using NUnit.Framework;
using System.Text.Json;


namespace PlaywrightTests.APITests;

public class PostAPITests : ApiFixtureBase
{
    [Test]
    public async Task CreatePost_Then_GetById_Works()
    {
        var postId = await CreatePostAsync(apiAuth!, authorId, Unique("title"), "body text");
        var get = await apiAuth!.GetAsync($"/api/Post/{postId}");
        Assert.That(get.Status, Is.EqualTo(200), await get.TextAsync());
    }

    [Test]
    public async Task CreatePost_WithoutToken_Returns401or403or400()
    {
        var resp = await api.PostAsync("/api/Post", new()
        {
            DataObject = new
            {
                id = GenerateObjectIdLike(),
                authorId = GenerateObjectIdLike(),
                title = "x",
                body = "y",
                mediaUrls = Array.Empty<string>(),
                tagsIds = Array.Empty<string>(),
                likedByUserIds = Array.Empty<string>(),
                createdAt = DateTime.UtcNow,
                updatedAt = DateTime.UtcNow
            }
        });

        Assert.That(resp.Status, Is.AnyOf(401, 403, 400), await resp.TextAsync());
    }

    [Test]
    public async Task DeletePost_RemovesPost()
    {
        var postId = await CreatePostAsync(apiAuth!, authorId, Unique("title"), "body");
        await DeletePostAsync(apiAuth!, postId);

        var get = await apiAuth!.GetAsync($"/api/Post/{postId}");
        Assert.That(get.Status, Is.AnyOf(404, 400), await get.TextAsync());
    }

        private static JsonElement ParseJson(string s) => JsonDocument.Parse(s).RootElement;

    [Test]
    public async Task UpdatePost_Works()
    {
        var postId = await CreatePostAsync(apiAuth!, authorId, Unique("title"), "body");

        var updatedTitle = Unique("updated");
        var updatedBody = "new body";

        var payload = new
        {
            id = postId,
            authorId = authorId,
            title = updatedTitle,
            body = updatedBody,
            mediaUrls = new string[] { },
            tagsIds = new string[] { },
            likedByUserIds = new string[] { },
            createdAt = DateTime.UtcNow,
            updatedAt = DateTime.UtcNow
        };

        var resp = await apiAuth!.PutAsync("/api/Post", new() { DataObject = payload });
        Assert.That(resp.Status, Is.EqualTo(200), await resp.TextAsync());

        var get = await apiAuth!.GetAsync($"/api/Post/{postId}");
        Assert.That(get.Status, Is.EqualTo(200), await get.TextAsync());

        var json = ParseJson(await get.TextAsync());
        Assert.That(json.GetProperty("title").GetString(), Is.EqualTo(updatedTitle));
        Assert.That(json.GetProperty("body").GetString(), Is.EqualTo(updatedBody));
    }

    [Test]
    public async Task ToggleLikePost_TwoToggles_ReturnsToOriginal()
    {
        var postId = await CreatePostAsync(apiAuth!, authorId, Unique("title"), "body");

        // 1) like
        var r1 = await apiAuth!.PutAsync($"/api/Post/{postId}/like?userId={authorId}");
        Assert.That(r1.Status, Is.EqualTo(200), await r1.TextAsync());

        var j1 = ParseJson(await r1.TextAsync());
        var liked1 = j1.GetProperty("likedByUserIds").EnumerateArray().Select(x => x.GetString()).ToList();
        Assert.That(liked1, Does.Contain(authorId));

        // 2) unlike (toggle)
        var r2 = await apiAuth!.PutAsync($"/api/Post/{postId}/like?userId={authorId}");
        Assert.That(r2.Status, Is.EqualTo(200), await r2.TextAsync());

        var j2 = ParseJson(await r2.TextAsync());
        var liked2 = j2.GetProperty("likedByUserIds").EnumerateArray().Select(x => x.GetString()).ToList();
        Assert.That(liked2, Does.Not.Contain(authorId));
    }

    [Test]
    public async Task DeletePost_Then_ThreadedComments_Empty_Cascade()
    {
        var postId = await CreatePostAsync(apiAuth!, authorId, Unique("title"), "body");
        var commentId = await CreateCommentAsync(apiAuth!, postId, authorId, "c1");

        await DeletePostAsync(apiAuth!, postId);

        // Post je obrisan
        var get = await apiAuth!.GetAsync($"/api/Post/{postId}");
        Assert.That(get.Status, Is.AnyOf(404, 400), await get.TextAsync());

        // Komentari za taj post treba da budu prazni (ili bar da nema commentId)
        var threaded = await apiAuth!.GetAsync($"/api/Comment/post/{postId}/threaded");
        Assert.That(threaded.Status, Is.EqualTo(200), await threaded.TextAsync());

        var txt = await threaded.TextAsync();
        Assert.That(txt, Does.Not.Contain(commentId));
    }

}