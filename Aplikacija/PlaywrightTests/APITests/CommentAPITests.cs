using NUnit.Framework;
using System.Text.Json;


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

    private static JsonElement ParseJson(string s) => JsonDocument.Parse(s).RootElement;

    [Test]
    public async Task ReplyComment_Then_GetThreaded_ContainsReply()
    {
        var postId = await CreatePostAsync(apiAuth!, authorId, Unique("title"), "body");

        var parentId = await CreateCommentAsync(apiAuth!, postId, authorId, "parent");
        var replyId  = await CreateCommentAsync(apiAuth!, postId, authorId, "reply", parentId);

        var resp = await apiAuth!.GetAsync($"/api/Comment/post/{postId}/threaded");
        Assert.That(resp.Status, Is.EqualTo(200), await resp.TextAsync());

        var json = ParseJson(await resp.TextAsync());

        Assert.That(json.ToString(), Does.Contain(replyId));
        Assert.That(json.ToString(), Does.Contain(parentId));
    }

    [Test]
    public async Task DeleteParentComment_DeletesAllReplies_TreeCascade()
    {
        var postId = await CreatePostAsync(apiAuth!, authorId, Unique("title"), "body");

        var parentId = await CreateCommentAsync(apiAuth!, postId, authorId, "parent");
        var replyId  = await CreateCommentAsync(apiAuth!, postId, authorId, "reply", parentId);

        //brise tree
        await DeleteCommentAsync(apiAuth!, parentId);

        var threaded = await apiAuth!.GetAsync($"/api/Comment/post/{postId}/threaded");
        Assert.That(threaded.Status, Is.EqualTo(200), await threaded.TextAsync());

        var text = await threaded.TextAsync();
        Assert.That(text, Does.Not.Contain(parentId));
        Assert.That(text, Does.Not.Contain(replyId));
    }

    [Test]
    public async Task Dislike_Then_Undislike_Works()
    {
        var postId = await CreatePostAsync(apiAuth!, authorId, Unique("title"), "body");
        var commentId = await CreateCommentAsync(apiAuth!, postId, authorId, "to react");

        //dislike
        var d = await apiAuth!.PostAsync($"/api/Comment/{commentId}/dislike/{authorId}");
        Assert.That(d.Status, Is.EqualTo(200), await d.TextAsync());

        //undislike
        var ud = await apiAuth!.PostAsync($"/api/Comment/{commentId}/undislike/{authorId}");
        Assert.That(ud.Status, Is.EqualTo(200), await ud.TextAsync());
    }

    [Test]
    public async Task Like_Then_Unlike_Works()
    {
        var postId = await CreatePostAsync(apiAuth!, authorId, Unique("title"), "body");
        var commentId = await CreateCommentAsync(apiAuth!, postId, authorId, "to react");

        var l = await apiAuth!.PostAsync($"/api/Comment/{commentId}/like/{authorId}");
        Assert.That(l.Status, Is.EqualTo(200), await l.TextAsync());

        var ul = await apiAuth!.PostAsync($"/api/Comment/{commentId}/unlike/{authorId}");
        Assert.That(ul.Status, Is.EqualTo(200), await ul.TextAsync());
    }
}
