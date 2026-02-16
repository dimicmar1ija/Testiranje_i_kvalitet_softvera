using NUnit.Framework;

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
}