using System.Text.Json;
using Microsoft.Playwright;
using NUnit.Framework;
using Microsoft.Playwright.NUnit;
using System.Text.RegularExpressions;



namespace PlaywrightTests;

public class BaseTest : PageTest
{
    protected const string ApiBaseUrl = "http://localhost:5000";
    protected const string UiBaseUrl = "http://localhost:5173";

    protected static string Unique(string prefix)
    {
        var s = $"{prefix}_{Guid.NewGuid():N}";
        return s.Length <= 24 ? s : s.Substring(0, 24);
    }

    protected async Task<IBrowserContext> NewContextWithJwtAsync(string jwt)
    {
        var ctx = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = UiBaseUrl
        });

        var escaped = jwt.Replace("\\", "\\\\").Replace("'", "\\'");

        await ctx.AddInitScriptAsync($@"
            (() => {{
                const token = '{escaped}';

                localStorage.setItem('jwt', token);
                localStorage.setItem('token', token);
                localStorage.setItem('accessToken', token);

                sessionStorage.setItem('jwt', token);
                sessionStorage.setItem('token', token);
                sessionStorage.setItem('accessToken', token);
            }})();
        ");

        return ctx;
    }
    protected async Task UiLoginAsync(string password)
    {
        Page.Dialog += async (_, dialog) =>
        {
            Console.WriteLine($"DIALOG ({dialog.Type}): {dialog.Message}");
            await dialog.DismissAsync();
        };

        var username = Unique("user");
        var email = $"{username}@example.com";

        // REGISTER
        await Page.GotoAsync($"{UiBaseUrl}/register", new() { WaitUntil = WaitUntilState.DOMContentLoaded });

        await Page.GetByPlaceholder("Username").FillAsync(username);
        await Page.GetByPlaceholder("Email").FillAsync(email);
        await Page.GetByPlaceholder("Password").FillAsync(password);

        var registerResponseTask = Page.WaitForResponseAsync(r =>
            r.Url.Contains("/api/Auth/register") && r.Request.Method == "POST",
            new() { Timeout = 15000 });

        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();

        var registerResp = await registerResponseTask;
        Console.WriteLine($"REGISTER RESP: {registerResp.Status} {registerResp.Url}");
        Console.WriteLine("REGISTER BODY: " + await registerResp.TextAsync());

        Assert.That(registerResp.Status, Is.InRange(200, 299),
            "Registracija nije uspela (nije 2xx). Pogledaj REGISTER BODY iznad.");

        await Assertions.Expect(Page).ToHaveURLAsync(new Regex(".*/login$", RegexOptions.IgnoreCase), new() { Timeout = 15000 });

        // LOGIN
        await Page.GetByPlaceholder("Username").FillAsync(username);
        await Page.GetByPlaceholder("Password").FillAsync(password);

        var loginResponseTask = Page.WaitForResponseAsync(r =>
            r.Url.Contains("/api/Auth/login") && r.Request.Method == "POST",
            new() { Timeout = 15000 });

        await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();

        var loginResp = await loginResponseTask;
        Console.WriteLine($"LOGIN RESP: {loginResp.Status} {loginResp.Url}");
        Console.WriteLine("LOGIN BODY: " + await loginResp.TextAsync());

        Assert.That(loginResp.Status, Is.InRange(200, 299),
            "Login nije uspeo (nije 2xx). Pogledaj LOGIN BODY iznad.");

        await Page.WaitForFunctionAsync("() => !!localStorage.getItem('jwt')", null, new() { Timeout = 15000 });
        await Assertions.Expect(Page).ToHaveURLAsync(new Regex(".*/home$", RegexOptions.IgnoreCase), new() { Timeout = 15000 });
    }

    private async Task<ILocator> FindByPlaceholderAnyAsync(params string[] placeholders)
    {
        foreach (var ph in placeholders)
        {
            var loc = Page.GetByPlaceholder(ph);
            if (await loc.CountAsync() > 0)
                return loc.First;
        }

        Assert.Fail("Ne mogu da nađem input po placeholder-u (username/password). Proveri placeholder tekstove na login strani.");
        return null!;
    }

    protected async Task UiOpenHomeWithJwtAsync(string jwt)
    {
        await SetJwtForUiAsync(jwt);
        await Page.GotoAsync($"{UiBaseUrl}/home", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await Assertions.Expect(Page).ToHaveURLAsync(new Regex(".*/home$", RegexOptions.IgnoreCase), new() { Timeout = 15000 });
    }


    protected async Task<IAPIRequestContext> CreateApiContextAsync(IPlaywright playwright, string? token = null)
    {
        await Page.ScreenshotAsync(new() { Path = "debug-home.png", FullPage = true });

        var headers = new Dictionary<string, string>
        {
            ["Accept"] = "application/json",
        };

        if (!string.IsNullOrWhiteSpace(token))
            headers["Authorization"] = $"Bearer {token}";

        return await playwright.APIRequest.NewContextAsync(new APIRequestNewContextOptions
        {
            BaseURL = ApiBaseUrl,
            ExtraHTTPHeaders = headers
        });
    }

   protected async Task RegisterAsync(IAPIRequestContext api, string username, string password)
    {
        var resp = await api.PostAsync("/api/Auth/register", new()
        {
            DataObject = new
            {
                username,
                email = $"{username}@test.local",
                password
            }
        });

        Assert.That(resp.Status, Is.AnyOf(200, 201),
            $"Register failed. Status={resp.Status}. Body={await resp.TextAsync()}");
    }



    protected async Task<string> LoginAndGetTokenAsync(IAPIRequestContext api, string username, string password)
    {
        var resp = await api.PostAsync("/api/Auth/login", new()
        {
            DataObject = new { username, password }
        });

        Assert.That(resp.Status, Is.EqualTo(200), $"Login failed. Status={resp.Status}. Body={await resp.TextAsync()}");

        var json = await resp.TextAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("token").GetString()!;
    }

    protected async Task<string> GetMyUserIdFromClaimsAsync(IAPIRequestContext apiWithAuth)
    {
        var resp = await apiWithAuth.GetAsync("/api/User/claims-test");
        Assert.That(resp.Status, Is.EqualTo(200), $"claims-test failed. Status={resp.Status}. Body={await resp.TextAsync()}");

        var json = await resp.TextAsync();
        using var doc = JsonDocument.Parse(json);

        foreach (var item in doc.RootElement.EnumerateArray())
        {
            var type = item.GetProperty("type").GetString() ?? "";
            var value = item.GetProperty("value").GetString() ?? "";

            if (type.Contains("nameidentifier", StringComparison.OrdinalIgnoreCase) ||
                type.Equals("sub", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }
        }

        Assert.Fail("Ne mogu da nađem userId u /api/User/claims-test (nema nameidentifier/sub).");
        return "";
    }

    protected async Task<string> CreateCategoryAsync(IAPIRequestContext api, string name)
    {
        var resp = await api.PostAsync("/api/Category", new()
        {
            DataObject = new { name }
        });

        Assert.That(resp.Status, Is.EqualTo(200), $"CreateCategory failed. Status={resp.Status}. Body={await resp.TextAsync()}");

        var json = await resp.TextAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("id").GetString()!;
    }


    protected async Task<string> CreatePostAsync(
        IAPIRequestContext apiWithAuth,
        string authorId,
        string title,
        string body)
    {
        await Page.ScreenshotAsync(new() { Path = "debug-home.png", FullPage = true });

        var now = DateTime.UtcNow;

        var payload = new
        {
            id = GenerateObjectIdLike(),
            authorId = authorId,
            title = title,
            body = body,
            mediaUrls = new List<string>(),
            tagsIds = new List<string>(),
            likedByUserIds = new List<string>(),
            createdAt = now,
            updatedAt = now
        };

        var resp = await apiWithAuth.PostAsync("/api/Post", new()
        {
            DataObject = payload
        });

        Assert.That(resp.Status, Is.AnyOf(200, 201),
            $"CreatePost failed. Status={resp.Status}. Body={await resp.TextAsync()}");

        var json = await resp.TextAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("id").GetString()!;
    }



    protected static string GenerateObjectIdLike()
        => Guid.NewGuid().ToString("N")[..24];


    protected async Task DeletePostAsync(IAPIRequestContext apiWithAuth, string postId)
    {
        var resp = await apiWithAuth.DeleteAsync($"/api/Post/{postId}");
        Assert.That(resp.Status, Is.EqualTo(204), $"DeletePost failed. Status={resp.Status}. Body={await resp.TextAsync()}");
    }

    protected async Task<string> CreateCommentAsync(
        IAPIRequestContext apiWithAuth,
        string postId,
        string authorId,
        string body,
        string? parentCommentId = null)
    {
        await Page.ScreenshotAsync(new() { Path = "debug-home.png", FullPage = true });

        var resp = await apiWithAuth.PostAsync("/api/Comment", new()
        {
            DataObject = new
            {
                postId = postId,
                authorId = authorId,
                parentCommentId = parentCommentId,
                body = body
            }
        });

        Assert.That(resp.Status, Is.EqualTo(200),
            $"CreateComment failed. Status={resp.Status}. Body={await resp.TextAsync()}");

        var json = await resp.TextAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("id").GetString()!;
    }

    protected async Task DeleteCommentAsync(IAPIRequestContext apiWithAuth, string commentId)
    {
        var resp = await apiWithAuth.DeleteAsync($"/api/Comment/{commentId}");
        Assert.That(resp.Status, Is.EqualTo(204), $"DeleteComment failed. Status={resp.Status}. Body={await resp.TextAsync()}");
    }

    protected async Task SetJwtForUiAsync(string jwt)
        {
                // pokrij više mogućih ključeva koje frontend može da koristi
                await Context.AddInitScriptAsync(@"(token) => {
                    localStorage.setItem('jwt', token);
                    localStorage.setItem('token', token);
                    localStorage.setItem('accessToken', token);
                    sessionStorage.setItem('jwt', token);
                    sessionStorage.setItem('token', token);
                }", jwt);
        }

    protected async Task<IPage> OpenHomeAsJwtAsync(string jwt)
    {
        var ctx = await NewContextWithJwtAsync(jwt);
        var page = await ctx.NewPageAsync();

        await page.GotoAsync($"{UiBaseUrl}/home", new() { WaitUntil = WaitUntilState.NetworkIdle });

        var lsJwt = await page.EvaluateAsync<string>("() => localStorage.getItem('jwt') || ''");
        Console.WriteLine($"DEBUG ls.jwt len={lsJwt.Length}, url={page.Url}");

        if (page.Url.EndsWith("/login", StringComparison.OrdinalIgnoreCase))
        {
            await page.ScreenshotAsync(new() { Path = "debug-redirected-to-login.png", FullPage = true });
            Assert.Fail($"Redirect na /login iako je init script upisan. ls.jwt len={lsJwt.Length}");
        }

        await Assertions.Expect(page).ToHaveURLAsync(new Regex(".*/home$", RegexOptions.IgnoreCase), new() { Timeout = 15000 });
        return page;
    }

    protected async Task<(string PostId, string Title)> UiCreatePostAsync(IPage page)
    {
        await page.Locator("button", new() { HasTextString = "Kreiraj" }).First.ClickAsync();

        var title = Unique("UI_POST");
        await page.GetByPlaceholder("Naslov").FillAsync(title);
        await page.GetByPlaceholder("Tekst (opciono)").FillAsync("UI body");

        var createPostRespTask = page.WaitForResponseAsync(r =>
            r.Url.Contains("/api/Post") && r.Request.Method == "POST",
            new() { Timeout = 20000 });

        await page.Locator("button", new() { HasTextString = "Objavi" }).First.ClickAsync();

        var resp = await createPostRespTask;
        var body = await resp.TextAsync();

        Console.WriteLine($"CREATE POST RESP: {resp.Status} {resp.Url}");
        Console.WriteLine("CREATE POST BODY: " + body);

        Assert.That(resp.Status, Is.InRange(200, 299), "UI kreiranje posta nije uspelo (nije 2xx).");

        using var doc = System.Text.Json.JsonDocument.Parse(body);
        var postId = doc.RootElement.GetProperty("id").GetString()!;

        return (postId, title);
    }

    protected async Task<(IPage page, ILocator postCard, string postId, string postTitle)> ArrangeHomePostWithCommentsAsync()
    {
        await using var api = await CreateApiContextAsync(this.Playwright);

        var u = Unique("pw_user");
        var p = "Pass123!";

        await RegisterAsync(api, u, p);
        var token = await LoginAndGetTokenAsync(api, u, p);

        var page = await OpenHomeAsJwtAsync(token);

        // kreiraj post i uzmi (id,title)
        var (postId, postTitle) = await UiCreatePostAsync(page);

        // uvek se vrati na home posle kreiranja
        await page.GotoAsync($"{UiBaseUrl}/home", new() { WaitUntil = WaitUntilState.NetworkIdle });

        // locator za post card po naslovu (h2 PostView)
        ILocator PostCard() =>
            page.Locator("h2", new() { HasTextString = postTitle })
                .First
                .Locator("xpath=ancestor::div[contains(@class,'rounded-3xl')][1]");

        // retry 4 puta: čekaj da feed učita i da se post pojavi
        for (int attempt = 1; attempt <= 4; attempt++)
        {
            // ako UI kaže da ne može da učita postove, uradi reload
            if (await page.GetByText("Ne mogu da učitam postove").CountAsync() > 0)
                await page.ReloadAsync(new() { WaitUntil = WaitUntilState.NetworkIdle });

            var postCard = PostCard();

            if (await postCard.CountAsync() > 0)
            {
                // čekaj vidljivost
                try
                {
                    await Assertions.Expect(postCard).ToBeVisibleAsync(new() { Timeout = 8000 });
                    return (page, postCard, postId, postTitle);
                }
                catch
                {
                    
                }
            }

            // nije nadjen -> reload i pokušaj ponovo
            await page.ReloadAsync(new() { WaitUntil = WaitUntilState.NetworkIdle });
            await page.WaitForTimeoutAsync(300);
        }

        await page.ScreenshotAsync(new() { Path = "debug-post-not-in-feed.png", FullPage = true });
        var bodyText = await page.Locator("body").InnerTextAsync();
        Assert.Fail($"Post '{postTitle}' nije pronađen na /home nakon retry. BODY:\n{bodyText}");

        return default;
    }

    protected async Task<string> AddCommentInPostCardAsync(ILocator postCard)
    {
        // 1) Otvori komentar thread
        var toggle = postCard.Locator("button", new() { HasTextString = "Komentari" }).First;
        await Assertions.Expect(toggle).ToBeVisibleAsync(new() { Timeout = 20000 });
        await toggle.ClickAsync();

        var threadHeader = postCard.Locator("h3", new() { HasTextString = "Komentari" });
        await Assertions.Expect(threadHeader).ToBeVisibleAsync(new() { Timeout = 20000 });

        // 2) Uhvati CommentForm textarea (placeholder "Napiši komentar…" / "Odgovori na komentar…")
        var commentBox = postCard
            .Locator("textarea[placeholder^='Napiši komentar'], textarea[placeholder^='Odgovori na komentar'], textarea")
            .First;

        await Assertions.Expect(commentBox).ToBeVisibleAsync(new() { Timeout = 20000 });

        var text = Unique("Komentar");
        await commentBox.FillAsync(text);

        // 3) Submit dugme u istoj formi (type="submit", text "Objavi komentar" ili "Odgovori")
        var form = commentBox.Locator("xpath=ancestor::form[1]");
        await Assertions.Expect(form).ToBeVisibleAsync(new() { Timeout = 20000 });

        var submit = form.Locator("button[type='submit']").First;
        await Assertions.Expect(submit).ToBeVisibleAsync(new() { Timeout = 20000 });

        await submit.ClickAsync();

        var emptyState = postCard.Locator("text=Još nema komentara.");
        if (await emptyState.CountAsync() > 0)
        {
            await Assertions.Expect(emptyState).ToHaveCountAsync(0, new() { Timeout = 20000 });
        }

        await Assertions.Expect(postCard).ToContainTextAsync(text, new() { Timeout = 20000 });

        return text;
    }

}
