var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

const string UploadDir = "/data/incoming";
const string ApiToken = "СЮДА_ДЛИННЫЙ_СЛУЧАЙНЫЙ_ТОКЕН";   // см. ниже как сгенерить

Directory.CreateDirectory(UploadDir);

app.MapPost("/upload", async (HttpRequest req) =>
{
    // 1. Авторизация по токену
    if (!req.Headers.TryGetValue("X-Api-Token", out var token) || token != ApiToken)
        return Results.Unauthorized();

    // 2. Имя файла из query: /upload?name=Fun_2026-10-10_14-30-00.log
    var name = req.Query["name"].ToString();
    if (string.IsNullOrWhiteSpace(name) || name.Contains('/') || name.Contains('\\'))
        return Results.BadRequest("bad filename");

    // 3. Принимаем тело и пишем в файл
    var path = Path.Combine(UploadDir, name);
    await using (var fs = File.Create(path))
    {
        await req.Body.CopyToAsync(fs);
    }

    var size = new FileInfo(path).Length;
    Console.WriteLine($"[{DateTime.UtcNow:u}] received {name} ({size} bytes)");
    return Results.Ok(new { ok = true, size });
});

app.MapGet("/health", () => Results.Ok("alive"));

app.Run();