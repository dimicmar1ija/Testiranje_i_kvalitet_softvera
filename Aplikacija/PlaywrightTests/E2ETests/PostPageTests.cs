using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace PlaywrightTests.E2ETests;

public class PostPageTests : BaseTest
{
    [Test]
    public async Task CreatePost_FromHome_ShowsInList()
    {
        await using var api = await CreateApiContextAsync(this.Playwright);

        // napravi novog usera i uzmi token (API)
        var u = Unique("pw_user");
        var p = "Pass123!";

        await RegisterAsync(api, u, p);
        var token = await LoginAndGetTokenAsync(api, u, p);

        // otvori UI kao ulogovan korisnik (JWT ubačen pre učitavanja React-a)
        var page = await OpenHomeAsJwtAsync(token);

        // otvori formu (dugme sadrži "Kreiraj")
        await page.Locator("button", new() { HasTextString = "Kreiraj" }).First.ClickAsync();

        var title = Unique("UI_Naslov");
        await page.GetByPlaceholder("Naslov").FillAsync(title);

        // textarea placeholder iz fronta
        await page.GetByPlaceholder("Tekst (opciono)").FillAsync("UI body");

        await page.GetByRole(AriaRole.Button, new() { Name = "Objavi" }).ClickAsync();

        // umesto body (brzo ali grubo), može i article lista, ali ovo je ok za početak
        await Assertions.Expect(page.Locator("body")).ToContainTextAsync(title, new() { Timeout = 15000 });
    }

}
