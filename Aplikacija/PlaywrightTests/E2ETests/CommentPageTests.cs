using Microsoft.Playwright;
using NUnit.Framework;

namespace PlaywrightTests.E2ETests;

public class CommentPageTests : BaseTest
{
    [Test]
    public async Task AddComment_OnHomePost_ShowsComment()
    {
        await using var api = await CreateApiContextAsync(this.Playwright);

        var u = Unique("pw_user");
        var p = "Pass123!";

        await RegisterAsync(api, u, p);
        var token = await LoginAndGetTokenAsync(api, u, p);

        var page = await OpenHomeAsJwtAsync(token);

        // Kreiraj post preko UI i uzmi ID + TITLE
        var (postId, postTitle) = await UiCreatePostAsync(page);

        // Uveri se da smo na /home
        await page.GotoAsync($"{UiBaseUrl}/home", new() { WaitUntil = WaitUntilState.NetworkIdle });

        // 1) Nađi karticu posta po naslovu (u PostView title je <h2>)
        var postCard = page.Locator("h2", new() { HasTextString = postTitle })
                           .Locator("xpath=ancestor::div[contains(@class,'rounded-3xl')][1]");

        await Assertions.Expect(postCard).ToBeVisibleAsync(new() { Timeout = 20000 });

        // 2) Klikni dugme "Komentari" (u PostView je baš taj tekst)
        var toggle = postCard.Locator("button", new() { HasTextString = "Komentari" }).First;
        await Assertions.Expect(toggle).ToBeVisibleAsync(new() { Timeout = 20000 });
        await toggle.ClickAsync();

        // 3) Sačekaj da se pojavi CommentThread (ima <h3>Komentari</h3>)
        var threadHeader = postCard.Locator("h3", new() { HasTextString = "Komentari" });
        await Assertions.Expect(threadHeader).ToBeVisibleAsync(new() { Timeout = 20000 });

        // 4) Polje iz CommentForm: može biti textarea ili input
        //    Uzimamo prvo koje postoji UNUTAR thread-a
        var commentBox = postCard.Locator("h3:has-text('Komentari')").Locator("xpath=..")
                                 .Locator("textarea, input[type='text'], input:not([type])")
                                 .First;

        await Assertions.Expect(commentBox).ToBeVisibleAsync(new() { Timeout = 20000 });

        var text = Unique("Komentar");
        await commentBox.FillAsync(text);

        // 5) Submit dugme: najbolje type='submit' u CommentForm
        var submit = postCard.Locator("button[type='submit']").First;

        if (await submit.CountAsync() == 0)
            submit = postCard.Locator("button", new() { HasTextString = "Objavi" }).First;

        if (await submit.CountAsync() == 0)
            submit = postCard.Locator("button", new() { HasTextString = "Pošalji" }).First;

        if (await submit.CountAsync() == 0)
        {
            await postCard.ScreenshotAsync(new() { Path = "debug-no-submit-comment.png" });
            Assert.Fail("Ne nalazim submit dugme za komentar (nema button[type='submit'] / Objavi / Pošalji).");
        }

        await submit.ClickAsync();

        // 6) Komentar se pojavljuje posle refresh() u CommentThread
        await Assertions.Expect(postCard).ToContainTextAsync(text, new() { Timeout = 20000 });
    }

    [Test]
    public async Task EditComment_OnHomePost_ShowsUpdatedText()
    {
        var (page, postCard, _, _) = await ArrangeHomePostWithCommentsAsync();

        var original = await AddCommentInPostCardAsync(postCard);

        // 1) Nađi blok komentara dok još postoji originalni tekst
        var commentBlock = postCard.Locator("div.border-l-2")
            .Filter(new() { HasTextString = original })
            .First;

        await Assertions.Expect(commentBlock).ToBeVisibleAsync(new() { Timeout = 20000 });

        // 2) Klik Izmeni
        await commentBlock.GetByRole(AriaRole.Button, new() { Name = "Izmeni" }).ClickAsync();

        // 3) U edit modu: textarea placeholder "Napiši komentar..."
        //    (lociraj najbliži comment blok koji SAD sadrži taj textarea)
        var editBox = postCard.Locator("textarea[placeholder='Napiši komentar...']").First;
        await Assertions.Expect(editBox).ToBeVisibleAsync(new() { Timeout = 20000 });

        var updated = Unique("Komentar_IZMENJEN");
        await editBox.FillAsync(updated);

        // 4) Klik Sačuvaj - uzmi dugme u istom comment bloku (border-l-2 parent)
        var editBlock = editBox.Locator("xpath=ancestor::div[contains(@class,'border-l-2')][1]");
        var saveBtn = editBlock.GetByRole(AriaRole.Button, new() { Name = "Sačuvaj" });

        await Assertions.Expect(saveBtn).ToBeVisibleAsync(new() { Timeout = 20000 });
        await saveBtn.ClickAsync();

        // 5) Čekaj da se pojavi novi tekst (refresh u CommentThread)
        await Assertions.Expect(postCard).ToContainTextAsync(updated, new() { Timeout = 20000 });
    }

    [Test]
    public async Task DeleteComment_OnHomePost_RemovesIt()
    {
        var (page, postCard, _, _) = await ArrangeHomePostWithCommentsAsync();

        var text = await AddCommentInPostCardAsync(postCard);

        var commentTextLoc = postCard.GetByText(text, new() { Exact = true });
        var commentBlock = commentTextLoc.Locator("xpath=ancestor::div[contains(@class,'border-l-2')][1]");
        await Assertions.Expect(commentBlock).ToBeVisibleAsync(new() { Timeout = 20000 });

        // klik Obriši
        await commentBlock.GetByRole(AriaRole.Button, new() { Name = "Obriši" }).ClickAsync();

        // refresh() se dešava u CommentThread, pa čekamo da nestane tekst
        await Assertions.Expect(commentTextLoc).ToHaveCountAsync(0, new() { Timeout = 20000 });
    }

    [Test]
    public async Task LikeComment_OnHomePost_IncrementsLikeCount()
    {
        var (page, postCard, _, _) = await ArrangeHomePostWithCommentsAsync();

        var text = await AddCommentInPostCardAsync(postCard);

        var commentBlock = postCard.GetByText(text, new() { Exact = true })
            .Locator("xpath=ancestor::div[contains(@class,'border-l-2')][1]");

        await Assertions.Expect(commentBlock).ToBeVisibleAsync(new() { Timeout = 20000 });

        // Like dugme je prvo u actions row-u
        var actionsRow = commentBlock.Locator("div.flex.gap-3.mt-2.items-center");
        await Assertions.Expect(actionsRow).ToBeVisibleAsync(new() { Timeout = 20000 });

        var likeBtn = actionsRow.Locator("button").Nth(0);

        var beforeText = await likeBtn.InnerTextAsync();
        var before = ExtractLastInt(beforeText);

        await likeBtn.ClickAsync();

        // posle refresh-a DOM se re-renderuje, pa re-lociraj dugme i čekaj da broj poraste
        int after = before;
        for (int i = 0; i < 20; i++) // ~20s
        {
            await page.WaitForTimeoutAsync(1000);

            var afterText = await actionsRow.Locator("button").Nth(0).InnerTextAsync();
            after = ExtractLastInt(afterText);

            if (after > before)
                break;
        }

        Assert.That(after, Is.GreaterThan(before),
            $"Like count nije porastao. Pre={before}, Posle={after}");
    }

    // helper: izvuci poslednji int iz stringa (brojač na kraju dugmeta)
    private static int ExtractLastInt(string s)
    {
        var m = System.Text.RegularExpressions.Regex.Match(s ?? "", @"(\d+)\s*$");
        return m.Success ? int.Parse(m.Groups[1].Value) : 0;
    }
}