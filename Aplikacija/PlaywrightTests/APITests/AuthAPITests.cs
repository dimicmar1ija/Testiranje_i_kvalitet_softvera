using System.Diagnostics;
using NUnit.Framework;

namespace PlaywrightTests.APITests;

public class AuthAPITests : ApiFixtureBase
{
    [Test]
    public async Task Register_Then_Login_ReturnsToken()
    {
        var u = Unique("user");
        var p = "Pass123!";

        await using var api = await CreateApiContextAsync(pw!);

        await RegisterAsync(api, u, p);
        var token = await LoginAndGetTokenAsync(api, u, p);

        Assert.That(token, Is.Not.Empty);
    }

    [Test]
    public async Task Login_WrongPassword_Returns401or400()
    {
        var u = Unique("user");
        var p = "Pass123!";

        await using var api = await CreateApiContextAsync(pw!);
        await RegisterAsync(api, u, p);

        var resp = await api.PostAsync("/api/Auth/login", new()
        {
            DataObject = new { username = u, password = "WRONG" }
        });

        Assert.That(resp.Status, Is.AnyOf(400, 401));
    }

    [Test]
    public async Task ClaimsTest_WithToken_Returns200()
    {
        var resp = await apiAuth.GetAsync("/api/User/claims-test");
        Assert.That(resp.Status, Is.EqualTo(200), await resp.TextAsync());
    }

    [Test]
    public async Task ClaimsTest_WithoutToken_Returns401or403()
    {
        var resp = await api.GetAsync("/api/User/claims-test");
        Assert.That(resp.Status, Is.AnyOf(401, 403));
    }
}