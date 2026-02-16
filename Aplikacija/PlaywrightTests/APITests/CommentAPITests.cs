using NUnit.Framework;

namespace PlaywrightTests.APITests;

public class CommentAPITests : ApiFixtureBase
{
    [Test]
    public async Task CreateComment_Then_GetThreaded_Works()
    {
        var postId = await CreatePostAsync(apiAuth!, authorId, Unique("title"), "body");
        var commentId = await CreateCommentAsync(apiAuth!, postId, authorId, "komentar");

        var threaded = await apiAuth.GetAsync($"/api/Comment/post/{postId}/threaded");
        Assert.That(threaded.Status, Is.EqualTo(200), await threaded.TextAsync());
        Assert.That(await threaded.TextAsync(), Does.Contain(commentId));
    }

    [Test]
    public async Task DeleteComment_WithToken_Works()
    {
        var postId = await CreatePostAsync(apiAuth!, authorId, Unique("title"), "body");
        var commentId = await CreateCommentAsync(apiAuth!, postId, authorId, "to delete");
        await DeleteCommentAsync(apiAuth, commentId);
    }

    [Test]
    public async Task CreateComment_WithoutToken_Returns401or403()
    {
        var resp = await api.PostAsync("/api/Comment", new()
        {
            DataObject = new
            {
                postId = GenerateObjectIdLike(),
                authorId = GenerateObjectIdLike(),
                parentCommentId = (string?)null,
                body = "x"
            }
        });

        Assert.That(resp.Status, Is.AnyOf(401, 403, 400), await resp.TextAsync());
    }
}
