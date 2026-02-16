using Microsoft.Playwright;
using NUnit.Framework;

namespace PlaywrightTests.APITests;

public abstract class ApiFixtureBase : BaseTest
{
    protected IPlaywright? pw;
    protected IAPIRequestContext? api;
    protected IAPIRequestContext? apiAuth;
    protected string token = "";
    protected string authorId = "";

    [OneTimeSetUp]
    public async Task OneTimeSetUp_ApiAuth()
    {
        pw = await Microsoft.Playwright.Playwright.CreateAsync();

        api = await CreateApiContextAsync(pw);

        var u = Unique("api_user");
        var p = "Pass123!";

        await RegisterAsync(api, u, p);
        token = await LoginAndGetTokenAsync(api, u, p);

        apiAuth = await CreateApiContextAsync(pw, token);
        authorId = await GetMyUserIdFromClaimsAsync(apiAuth);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown_Api()
    {
        if (apiAuth != null) await apiAuth.DisposeAsync();
        if (api != null) await api.DisposeAsync();
        pw?.Dispose();
    }
}