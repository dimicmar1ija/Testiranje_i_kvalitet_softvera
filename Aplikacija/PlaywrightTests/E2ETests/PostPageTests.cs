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
}