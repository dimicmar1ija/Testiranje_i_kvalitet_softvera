using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace NUnitTests;

public class TestApiClient
{
    public HttpClient Http { get; }

    public TestApiClient(string baseUrl = "http://localhost:5000")
    {
        Http = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public void SetBearer(string token)
    {
        Http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public static StringContent JsonContent(object body)
        => new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");


    public async Task<HttpResponseMessage> RegisterAsync(string username, string email, string password)
    {
        var dto = new { username, email, password };
        return await Http.PostAsync("/api/Auth/register", JsonContent(dto));
    }
    
    public async Task<(string token, string userId)> EnsureAuthedAsync()
    {
        var uname = "nunit_" + Guid.NewGuid().ToString("N")[..8];
        var email = uname + "@test.com";
        var pass = "Test123!";

        var (token, userId) = await RegisterAndLoginAsync(uname, email, pass);
        SetBearer(token);
        return (token, userId);
    }

    public async Task<string> LoginAndGetTokenAsync(string username, string password)
    {
        var dto = new { username, password };
        var resp = await Http.PostAsync("/api/Auth/login", JsonContent(dto));
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync();
        dynamic obj = JsonConvert.DeserializeObject<dynamic>(json)!;

        return (string)(obj.token ?? obj.Token);
    }

    public async Task<(string token, string userId)> RegisterAndLoginAsync(string username, string email, string password)
    {
        var reg = await RegisterAsync(username, email, password);

        var token = await LoginAndGetTokenAsync(username, password);

        var userId = ExtractSubFromJwt(token);
        return (token, userId);
    }

    private static string ExtractSubFromJwt(string jwt)
    {
        var parts = jwt.Split('.');
        if (parts.Length < 2) return "";

        string payload = parts[1]
            .Replace('-', '+')
            .Replace('_', '/');

        switch (payload.Length % 4)
        {
            case 2: payload += "=="; break;
            case 3: payload += "="; break;
        }

        var bytes = Convert.FromBase64String(payload);
        var json = Encoding.UTF8.GetString(bytes);

        dynamic obj = JsonConvert.DeserializeObject<dynamic>(json)!;
        return (string)(obj.sub ?? obj.Sub ?? "");
    }

    public async Task<HttpResponseMessage> PostAsync(string url)
        => await Http.PostAsync(url.StartsWith("/") ? url : "/" + url, null);

    public async Task<HttpResponseMessage> PostAsync(string url, object body)
        => await Http.PostAsync(url.StartsWith("/") ? url : "/" + url, JsonContent(body));
}
