using Microsoft.Playwright;
using NUnit.Framework;

namespace PlaywrightTests.E2ETests;

public class PostPageTests : UiFixtureBase
{
    [Test]
    public async Task CreatePost_FromHome_ShowsInList()
    {
        var (ctx, page) = await NewAuthedPageAsync();

        try
        {
            await page.GotoAsync($"{UiBaseUrl}/home", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded
            });

            await page.GetByRole(AriaRole.Button, new() { Name = "Kreiraj post" }).ClickAsync();

            var title = $"UI_Naslov_{Guid.NewGuid():N}"[..20];

            await page.GetByPlaceholder("Naslov").FillAsync(title);
            await page.GetByPlaceholder("Tekst (opciono)").FillAsync("UI body");

            var waitResp = page.WaitForResponseAsync(r =>
                r.Request.Method == "POST" && r.Url.Contains("/api/Post"));

            await page.GetByRole(AriaRole.Button, new() { Name = "Objavi" }).ClickAsync();

            var resp = await waitResp;
            Assert.That(resp.Status, Is.AnyOf(200, 201), await resp.TextAsync());

            var postCard = page.Locator("article").Filter(new() { HasTextString = title }).First;
            await Assertions.Expect(postCard).ToBeVisibleAsync(new() { Timeout = 15000 });
        }
        finally
        {
            await ctx.DisposeAsync();
        }
    }
}