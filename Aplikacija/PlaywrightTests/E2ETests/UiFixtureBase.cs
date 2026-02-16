using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace PlaywrightTests.E2ETests;

public abstract class UiFixtureBase : PageTest
{
    protected const string ApiBaseUrl = "http://localhost:5000";
    protected const string UiBaseUrl  = "http://localhost:5173";

    protected static string Unique(string prefix)
    {
        var s = $"{prefix}_{Guid.NewGuid():N}";
        return s.Length <= 24 ? s : s.Substring(0, 24);
    }

    protected string AuthStatePath =>
        Path.Combine(TestContext.CurrentContext.WorkDirectory, "auth.json");

    private IPlaywright? _pw;
    private IBrowser? _browser;

    [OneTimeSetUp]
    public async Task OneTimeSetUp_CreateStorageState()
    {
        if (File.Exists(AuthStatePath))
            File.Delete(AuthStatePath);

        _pw = await Microsoft.Playwright.Playwright.CreateAsync();
        _browser = await _pw.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });

        await using var api = await _pw.APIRequest.NewContextAsync(new APIRequestNewContextOptions
        {
            BaseURL = ApiBaseUrl,
            ExtraHTTPHeaders = new Dictionary<string, string> { ["Accept"] = "application/json" }
        });

        var u = ("pw_user_" + Guid.NewGuid().ToString("N"))[..20];
        var p = "Pass123!";

        var reg = await api.PostAsync("/api/Auth/register", new()
        {
            DataObject = new { username = u, email = $"{u}@test.local", password = p }
        });
        Assert.That(reg.Status, Is.AnyOf(200, 201), await reg.TextAsync());

        var login = await api.PostAsync("/api/Auth/login", new()
        {
            DataObject = new { username = u, password = p }
        });
        Assert.That(login.Status, Is.EqualTo(200), await login.TextAsync());

        var token = JsonDocument.Parse(await login.TextAsync())
            .RootElement.GetProperty("token").GetString()!;

        var ctx = await _browser.NewContextAsync(new BrowserNewContextOptions { BaseURL = UiBaseUrl });
        var page = await ctx.NewPageAsync();

        await page.GotoAsync($"{UiBaseUrl}/", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        await page.EvaluateAsync(@"(t) => {
            localStorage.setItem('jwt', t);
            localStorage.setItem('token', t);
            localStorage.setItem('accessToken', t);
            sessionStorage.setItem('jwt', t);
            sessionStorage.setItem('token', t);
            sessionStorage.setItem('accessToken', t);
        }", token);

        await ctx.StorageStateAsync(new BrowserContextStorageStateOptions { Path = AuthStatePath });
        await ctx.DisposeAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown_Ui()
    {
        if (_browser != null) await _browser.CloseAsync();
        _pw?.Dispose();
    }

    protected async Task<(IBrowserContext ctx, IPage page)> NewAuthedPageAsync()
    {
        var ctx = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = UiBaseUrl,
            StorageStatePath = AuthStatePath
        });

        var page = await ctx.NewPageAsync();
        return (ctx, page);
    }
}