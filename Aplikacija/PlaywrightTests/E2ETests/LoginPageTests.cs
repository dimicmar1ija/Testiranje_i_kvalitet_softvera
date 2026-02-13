using System.Text.RegularExpressions;
using Microsoft.Playwright;
using NUnit.Framework;

namespace PlaywrightTests.E2ETests
{
    public class LoginPageTests : BaseTest
    {
        [Test]
        public async Task Register_Then_Login_NavigatesToHome()
        {
            var username = Unique("user");
            var email = $"{username}@example.com";
            var password = "Test123!";

            await Page.GotoAsync($"{UiBaseUrl}/register", new() { WaitUntil = WaitUntilState.NetworkIdle });

            await Page.GetByPlaceholder("Username").FillAsync(username);
            await Page.GetByPlaceholder("Email").FillAsync(email);
            await Page.GetByPlaceholder("Password").FillAsync(password);

            var regRespTask = Page.WaitForResponseAsync(r =>
                r.Url.Contains("/api/Auth/register") && r.Request.Method == "POST",
                new() { Timeout = 15000 });

            await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
            var regResp = await regRespTask;

            Assert.That(regResp.Status, Is.InRange(200, 299),
                $"Register nije uspeo. Status={regResp.Status}. Body={await regResp.TextAsync()}");

            await Assertions.Expect(Page).ToHaveURLAsync(new Regex(".*/login$", RegexOptions.IgnoreCase), new() { Timeout = 15000 });

            await Page.GetByPlaceholder("Username").FillAsync(username);
            await Page.GetByPlaceholder("Password").FillAsync(password);

            var loginRespTask = Page.WaitForResponseAsync(r =>
                r.Url.Contains("/api/Auth/login") && r.Request.Method == "POST",
                new() { Timeout = 15000 });

            await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();
            var loginResp = await loginRespTask;

            Assert.That(loginResp.Status, Is.InRange(200, 299),
                $"Login nije uspeo. Status={loginResp.Status}. Body={await loginResp.TextAsync()}");

            // ključno: sačekaj token u localStorage
            await Page.WaitForFunctionAsync("() => !!localStorage.getItem('jwt')", null, new() { Timeout = 15000 });

            await Assertions.Expect(Page).ToHaveURLAsync(new Regex(".*/home$", RegexOptions.IgnoreCase), new() { Timeout = 15000 });
        }
    }
}