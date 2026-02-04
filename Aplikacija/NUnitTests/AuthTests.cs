using NUnit.Framework;
using System.Net;

namespace NUnitTests;

[TestFixture]
public class AuthTests
{
    private TestApiClient _api = null!;

    [SetUp]
    public void Setup()
    {
        var baseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:5000";
        _api = new TestApiClient(baseUrl);
    }

    [Test]
    public async Task Register_Then_Login_ReturnsToken()
    {
        var u = "test_" + Guid.NewGuid().ToString("N")[..8];
        var email = $"{u}@test.local";
        var pass = "Test123!";

        var reg = await _api.RegisterAsync(u, email, pass);
        Assert.That((int)reg.StatusCode, Is.EqualTo(200).Or.EqualTo(201));

        var token = await _api.LoginAndGetTokenAsync(u, pass);
        Assert.That(token, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public async Task TestAuthorization_WithoutToken_Returns401()
    {
        var resp = await _api.PostAsync("api/Auth/testAuthorization");
        Assert.That((int)resp.StatusCode, Is.EqualTo(401));
    }

    [Test]
    public async Task TestAuthorization_WithBearerToken_Returns200()
    {
        var u = "test_" + Guid.NewGuid().ToString("N")[..8];
        var email = $"{u}@test.local";
        var pass = "Test123!";

        var reg = await _api.RegisterAsync(u, email, pass);
        Assert.That((int)reg.StatusCode, Is.EqualTo(200).Or.EqualTo(201));

        var token = await _api.LoginAndGetTokenAsync(u, pass);
        _api.SetBearer(token);

        var resp = await _api.PostAsync("api/Auth/testAuthorization");
        Assert.That((int)resp.StatusCode, Is.EqualTo(200));
    }
}
