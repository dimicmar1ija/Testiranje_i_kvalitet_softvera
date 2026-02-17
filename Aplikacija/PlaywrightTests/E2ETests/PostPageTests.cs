using Microsoft.Playwright;
using NUnit.Framework;

namespace PlaywrightTests.E2ETests;

public class PostPageTests : UiFixtureBase
{
    [Test]
    public async Task CreatePost_FromHome_ShowsInList()
    {
        var (ctx, page) = await NewAuthedPageAsync();

        await page.GotoAsync($"{UiBaseUrl}/home", new PageGotoOptions
        {
             WaitUntil = WaitUntilState.DOMContentLoaded
        });

        await page.GetByRole(AriaRole.Button, new() { Name = "Kreiraj post" }).ClickAsync();

        var title = $"UI_Naslov_{Guid.NewGuid():N}"[..20];

        await page.GetByPlaceholder("Naslov").FillAsync(title);
        await page.GetByPlaceholder("Tekst (opciono)").FillAsync("UI body");

        await page.GetByRole(AriaRole.Button, new() { Name = "Objavi" }).ClickAsync();

        await Assertions.Expect(page.Locator("body")).ToContainTextAsync(title, new() { Timeout = 15000 });
        await ctx.DisposeAsync();
        
    }

    [Test]
    public async Task EditPost_AsAuthor_UpdatesTitleAndShowsEdited()
    {
        var (ctx, page) = await NewAuthedPageAsync();

        await page.GotoAsync($"{UiBaseUrl}/home", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        await page.GetByRole(AriaRole.Button, new() { Name = "Kreiraj post" }).ClickAsync();

        var oldTitle = Unique("UI_EDIT");
        await page.GetByPlaceholder("Naslov").FillAsync(oldTitle);
        await page.GetByPlaceholder("Tekst (opciono)").FillAsync("UI body");

        var createRespTask = page.WaitForResponseAsync(r =>
            r.Url.Contains("/api/Post") && r.Request.Method == "POST",
            new() { Timeout = 20000 });

        await page.GetByRole(AriaRole.Button, new() { Name = "Objavi" }).ClickAsync();
        var createResp = await createRespTask;

        Assert.That(createResp.Status, Is.InRange(200, 299), await createResp.TextAsync());

        await page.GotoAsync($"{UiBaseUrl}/home", new() { WaitUntil = WaitUntilState.DOMContentLoaded });

        var postCard = await FindPostCardByTitleAsync(page, oldTitle);

        await postCard.GetByRole(AriaRole.Button, new() { Name = "Izmeni" }).ClickAsync();

        var titleBox = page.GetByPlaceholder("Naslov");
        await Assertions.Expect(titleBox).ToBeVisibleAsync(new() { Timeout = 20000 });

        var newTitle = Unique("UI_EDIT2");
        await titleBox.FillAsync(newTitle);

        var putRespTask = page.WaitForResponseAsync(r =>
            r.Url.Contains("/api/Post") && r.Request.Method == "PUT",
            new() { Timeout = 20000 });

        var saveBtn = page.GetByRole(AriaRole.Button, new() { Name = "Objavi" });
        if (await saveBtn.CountAsync() == 0)
            saveBtn = page.GetByRole(AriaRole.Button, new() { Name = "Sačuvaj" }).First;

        await Assertions.Expect(saveBtn.First).ToBeVisibleAsync(new() { Timeout = 20000 });
        await saveBtn.First.ClickAsync();

        var putResp = await putRespTask;
        Assert.That(putResp.Status, Is.InRange(200, 299), $"PUT nije uspeo. Body={await putResp.TextAsync()}");

        await page.ReloadAsync(new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await Assertions.Expect(page.Locator("h2", new() { HasTextString = newTitle }).First)
            .ToBeVisibleAsync(new() { Timeout = 20000 });

        await ctx.DisposeAsync();
    }

    [Test]
    public async Task DeletePost_AsAuthor_RemovesFromList()
    {
        var (ctx, page) = await NewAuthedPageAsync();

        await page.GotoAsync($"{UiBaseUrl}/home", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        await page.GetByRole(AriaRole.Button, new() { Name = "Kreiraj post" }).ClickAsync();

        var title = Unique("UI_DEL");
        await page.GetByPlaceholder("Naslov").FillAsync(title);
        await page.GetByPlaceholder("Tekst (opciono)").FillAsync("UI body");

        var createRespTask = page.WaitForResponseAsync(r =>
            r.Url.Contains("/api/Post") && r.Request.Method == "POST",
            new() { Timeout = 20000 });

        await page.GetByRole(AriaRole.Button, new() { Name = "Objavi" }).ClickAsync();
        var createResp = await createRespTask;

        Assert.That(createResp.Status, Is.InRange(200, 299), await createResp.TextAsync());

        await page.GotoAsync($"{UiBaseUrl}/home", new() { WaitUntil = WaitUntilState.DOMContentLoaded });

        var postCard = await FindPostCardByTitleAsync(page, title);

        page.Dialog += async (_, d) => await d.AcceptAsync();

        var delRespTask = page.WaitForResponseAsync(r =>
            r.Url.Contains("/api/Post") && r.Request.Method == "DELETE",
            new() { Timeout = 20000 });

        await postCard.GetByRole(AriaRole.Button, new() { Name = "Obriši" }).ClickAsync();

        var delResp = await delRespTask;
        Assert.That(delResp.Status, Is.AnyOf(200, 204), $"DELETE nije 200/204. Body={await delResp.TextAsync()}");

        await page.ReloadAsync(new() { WaitUntil = WaitUntilState.DOMContentLoaded });

        var titleLoc = page.GetByText(title, new() { Exact = true });
        await Assertions.Expect(titleLoc).ToHaveCountAsync(0, new() { Timeout = 20000 });

        await ctx.DisposeAsync();
    }

    //helpers
    private async Task<ILocator> FindPostCardByTitleAsync(IPage page, string title)
    {
        var h2 = page.Locator("h2", new() { HasTextString = title }).First;
        await Assertions.Expect(h2).ToBeVisibleAsync(new() { Timeout = 20000 });

        var card = h2.Locator("xpath=ancestor::div[contains(@class,'rounded-3xl')][1]");
        await Assertions.Expect(card).ToBeVisibleAsync(new() { Timeout = 20000 });
        return card;
    }
}