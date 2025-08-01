// https://lucid.app/lucidchart/362ac097-3f4a-40d5-bfaf-5c3c9bb5af82/edit?beaconFlowId=2B53C647ACE44AB9&page=0_0&invitationId=inv_ceb0ae1a-85c8-45e3-bb77-68b189d9a869#
// https://lucid.app/lucidchart/b79f1ba4-9c48-47c0-b85d-e85dd7a07c8c/edit?beaconFlowId=297A22F2FE9FB6B0&invitationId=inv_7f394199-45d1-479b-b5e8-5a617bbe803f&page=0_0#
var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5085); // Replace with your actual port
});

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.MapGet("/", () => "Login backend running");

app.MapGet("/login", async (HttpContext ctx) =>
{
    var sessionId = ctx.Request.Query["sessionId"];
    var tvIp = ctx.Request.Query["tvIp"];

    string html = $@"
    <html>
    <body>
        <h2>Login Session: {sessionId}</h2>
        <form method='POST' action='/complete'>
            <input type='hidden' name='sessionId' value='{sessionId}' />
            <input type='hidden' name='tvIp' value='{tvIp}' />
            <input name='name' placeholder='Your Name' />
            <button type='submit'>Login</button>
        </form>
    </body>
    </html>";
    ctx.Response.ContentType = "text/html";
    await ctx.Response.WriteAsync(html);
});

app.MapPost("/complete", async (HttpContext ctx) =>
{
    var form = await ctx.Request.ReadFormAsync();
    var name = form["name"];
    var tvIp = form["tvIp"];
    var token = Guid.NewGuid().ToString();

    using var http = new HttpClient();
    try
    {
        string callbackUrl = $"http://{tvIp}:8652/success?name={Uri.EscapeDataString(name)}&token={token}";
        await http.GetAsync(callbackUrl);
        await ctx.Response.WriteAsync("Login successful! You may return to the app.");
    }
    catch
    {
        await ctx.Response.WriteAsync("Error: Failed to contact the app.");
    }
});

app.Run();