using Microsoft.Playwright;
using NUnit.Framework;

namespace PlaywrightTests.E2ETests;

public class CommentPageTests : UiFixtureBase
{
    [Test]
    public async Task AddComment_OnHomePost_ShowsComment()
    {
        var (ctx, page, postCard, _) = await ArrangeHomePostWithCommentsAsync();

        var text = Unique("Komentar");
        await AddCommentInPostCardAsync(postCard, text);

        await Assertions.Expect(postCard).ToContainTextAsync(text, new() { Timeout = 20000 });

        await ctx.DisposeAsync();
    }

    [Test]
    public async Task EditComment_OnHomePost_ShowsUpdatedText()
    {
        var (ctx, page, postCard, _) = await ArrangeHomePostWithCommentsAsync();

        var original = Unique("Komentar");
        await AddCommentInPostCardAsync(postCard, original);

        var commentBlock = postCard.Locator("div.border-l-2")
            .Filter(new() { HasTextString = original })
            .First;

        await Assertions.Expect(commentBlock).ToBeVisibleAsync(new() { Timeout = 20000 });

        await commentBlock.GetByRole(AriaRole.Button, new() { Name = "Izmeni" }).ClickAsync();

        var editBox = postCard.Locator("textarea[placeholder='Napiši komentar...']").First;
        await Assertions.Expect(editBox).ToBeVisibleAsync(new() { Timeout = 20000 });

        var updated = Unique("Komentar_IZMENJEN");
        await editBox.FillAsync(updated);

        var editBlock = editBox.Locator("xpath=ancestor::div[contains(@class,'border-l-2')][1]");
        var saveBtn = editBlock.GetByRole(AriaRole.Button, new() { Name = "Sačuvaj" });

        await Assertions.Expect(saveBtn).ToBeVisibleAsync(new() { Timeout = 20000 });
        await saveBtn.ClickAsync();

        await Assertions.Expect(postCard).ToContainTextAsync(updated, new() { Timeout = 20000 });

        await ctx.DisposeAsync();
    }

    [Test]
    public async Task DeleteComment_OnHomePost_RemovesIt()
    {
        var (ctx, page, postCard, _) = await ArrangeHomePostWithCommentsAsync();

        var text = Unique("Komentar");
        await AddCommentInPostCardAsync(postCard, text);

        var commentTextLoc = postCard.GetByText(text, new() { Exact = true });
        var commentBlock = commentTextLoc.Locator("xpath=ancestor::div[contains(@class,'border-l-2')][1]");
        await Assertions.Expect(commentBlock).ToBeVisibleAsync(new() { Timeout = 20000 });

        await commentBlock.GetByRole(AriaRole.Button, new() { Name = "Obriši" }).ClickAsync();

        await Assertions.Expect(commentTextLoc).ToHaveCountAsync(0, new() { Timeout = 20000 });

        await ctx.DisposeAsync();
    }

    [Test]
    public async Task LikeComment_OnHomePost_IncrementsLikeCount()
    {
        var (ctx, page, postCard, _) = await ArrangeHomePostWithCommentsAsync();

        var text = Unique("Komentar");
        await AddCommentInPostCardAsync(postCard, text);

        var commentBlock = postCard.GetByText(text, new() { Exact = true })
            .Locator("xpath=ancestor::div[contains(@class,'border-l-2')][1]");

        await Assertions.Expect(commentBlock).ToBeVisibleAsync(new() { Timeout = 20000 });

        var actionsRow = commentBlock.Locator("div.flex.gap-3.mt-2.items-center");
        await Assertions.Expect(actionsRow).ToBeVisibleAsync(new() { Timeout = 20000 });

        var likeBtn = actionsRow.Locator("button").Nth(0);

        var beforeText = await likeBtn.InnerTextAsync();
        var before = ExtractLastInt(beforeText);

        await likeBtn.ClickAsync();

        await Assertions.Expect(likeBtn)
                .Not.ToHaveTextAsync(beforeText, new() { Timeout = 10000 });

        var afterText = await likeBtn.InnerTextAsync();
        var after = ExtractLastInt(afterText);

        Assert.That(after, Is.GreaterThan(before),
            $"Like count nije porastao. Pre={before}, Posle={after}");

        await ctx.DisposeAsync();
    }

    [Test]
    public async Task DislikeComment_OnHomePost_TogglesStateAndCount()
    {
        var (ctx, page, postCard, _) = await ArrangeHomePostWithCommentsAsync();

        var text = Unique("Komentar");
        await AddCommentInPostCardAsync(postCard, text);

        var commentBlock = postCard.GetByText(text, new() { Exact = true })
            .Locator("xpath=ancestor::div[contains(@class,'border-l-2')][1]");

        await Assertions.Expect(commentBlock).ToBeVisibleAsync(new() { Timeout = 20000 });

        var actionsRow = commentBlock.Locator("div.flex.gap-3.mt-2.items-center");
        await Assertions.Expect(actionsRow).ToBeVisibleAsync(new() { Timeout = 20000 });

        var dislikeBtn = actionsRow.Locator("button").Nth(1);

        var beforeText = await dislikeBtn.InnerTextAsync();
        var before = ExtractLastInt(beforeText);

        await dislikeBtn.ClickAsync();

        await Assertions.Expect(dislikeBtn)
            .ToHaveClassAsync(new System.Text.RegularExpressions.Regex(".*bg-red-600.*"),
                new() { Timeout = 20000 });

        var afterText = await dislikeBtn.InnerTextAsync();
        var after = ExtractLastInt(afterText);

        Assert.That(after, Is.GreaterThanOrEqualTo(before),
            $"Dislike count nije porastao/ostao isti. Pre={before}, Posle={after}. PreText='{beforeText}', PosleText='{afterText}'");

        await dislikeBtn.ClickAsync();

        await Assertions.Expect(dislikeBtn)
            .Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex(".*bg-red-600.*"),
                new() { Timeout = 20000 });

        await ctx.DisposeAsync();
    }

[Test]
    public async Task ReplyComment_OnHomePost_ShowsNestedReply()
    {
        var (ctx, page, postCard, _) = await ArrangeHomePostWithCommentsAsync();

        var parentText = Unique("PARENT");
        await AddCommentInPostCardAsync(postCard, parentText);

        var parentBlock = postCard.GetByText(parentText, new() { Exact = true })
            .Locator("xpath=ancestor::div[contains(@class,'border-l-2')][1]");
        await Assertions.Expect(parentBlock).ToBeVisibleAsync(new() { Timeout = 20000 });

        await parentBlock.GetByRole(AriaRole.Button, new() { Name = "Odgovori" }).ClickAsync();

        var replyBox = parentBlock.Locator("textarea[placeholder^='Odgovori na komentar']").First;
        await Assertions.Expect(replyBox).ToBeVisibleAsync(new() { Timeout = 20000 });

        var replyText = Unique("REPLY");
        await replyBox.FillAsync(replyText);

        var replyForm = replyBox.Locator("xpath=ancestor::form[1]");
        var submit = replyForm.Locator("button[type='submit']").First;
        await Assertions.Expect(submit).ToBeVisibleAsync(new() { Timeout = 20000 });

        await submit.ClickAsync();

        // verify da se reply pojavio 
        await Assertions.Expect(postCard).ToContainTextAsync(replyText, new() { Timeout = 20000 });

        await ctx.DisposeAsync();
    }

    //helpers
    private async Task<(IBrowserContext ctx, IPage page, ILocator postCard, string postTitle)>
        ArrangeHomePostWithCommentsAsync()
    {
        var (ctx, page) = await NewAuthedPageAsync();

        await page.GotoAsync($"{UiBaseUrl}/home", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        await page.Locator("button", new() { HasTextString = "Kreiraj" }).First.ClickAsync();

        var postTitle = Unique("UI_POST");
        await page.GetByPlaceholder("Naslov").FillAsync(postTitle);
        await page.GetByPlaceholder("Tekst (opciono)").FillAsync("UI body");

        var createPostRespTask = page.WaitForResponseAsync(r =>
            r.Url.Contains("/api/Post") && r.Request.Method == "POST",
            new() { Timeout = 15000 });

        await page.Locator("button", new() { HasTextString = "Objavi" }).First.ClickAsync();

        var resp = await createPostRespTask;
        Assert.That(resp.Status, Is.InRange(200, 299), $"UI kreiranje posta nije uspelo. Body={await resp.TextAsync()}");

        // /home  (GET /api/Post)
        await page.GotoAsync($"{UiBaseUrl}/home", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        await page.WaitForResponseAsync(r =>
            r.Url.Contains("/api/Post") && r.Request.Method == "GET",
            new() { Timeout = 15000 });

        //reload
        if (await page.GetByText("Ne mogu da učitam postove").CountAsync() > 0)
        {
            await page.ReloadAsync(new() { WaitUntil = WaitUntilState.DOMContentLoaded });
            await page.WaitForResponseAsync(r =>
                r.Url.Contains("/api/Post") && r.Request.Method == "GET",
                new() { Timeout = 15000 });
        }

        var postCard = page.Locator("h2", new() { HasTextString = postTitle })
            .First
            .Locator("xpath=ancestor::div[contains(@class,'rounded-3xl')][1]");

        await Assertions.Expect(postCard).ToBeVisibleAsync(new() { Timeout = 15000 });

        return (ctx, page, postCard, postTitle);
    }

    private async Task AddCommentInPostCardAsync(ILocator postCard, string text)
    {
        var toggle = postCard.Locator("button", new() { HasTextString = "Komentari" }).First;
        await Assertions.Expect(toggle).ToBeVisibleAsync(new() { Timeout = 15000 });
        await toggle.ClickAsync();

        var threadHeader = postCard.Locator("h3", new() { HasTextString = "Komentari" });
        await Assertions.Expect(threadHeader).ToBeVisibleAsync(new() { Timeout = 15000 });

        var commentBox = postCard.Locator("textarea").First;
        await Assertions.Expect(commentBox).ToBeVisibleAsync(new() { Timeout = 15000 });

        await commentBox.FillAsync(text);

        var form = commentBox.Locator("xpath=ancestor::form[1]");
        await Assertions.Expect(form).ToBeVisibleAsync(new() { Timeout = 15000 });

        var submit = form.Locator("button[type='submit']").First;

        // fallback 
        if (await submit.CountAsync() == 0)
            submit = form.Locator("button", new() { HasTextString = "Objavi" }).First;

        if (await submit.CountAsync() == 0)
        {
            await postCard.ScreenshotAsync(new() { Path = "debug-no-submit-comment.png" });
            Assert.Fail("Ne nalazim submit dugme za komentar (nema button[type='submit'] / Objavi ).");
        }

        await submit.ClickAsync();

        await Assertions.Expect(postCard).ToContainTextAsync(text, new() { Timeout = 15000 });
    }

    private static int ExtractLastInt(string s)
    {
        var m = System.Text.RegularExpressions.Regex.Match(s ?? "", @"(\d+)\s*$");
        return m.Success ? int.Parse(m.Groups[1].Value) : 0;
    }
}