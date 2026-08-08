using UrlShortener.Api;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IUrlRepository, InMemoryUrlRepository>();
builder.Services.AddSingleton<UrlShortenerService>();

var app = builder.Build();

app.MapGet("/", () => "URL Shortener API");

app.MapPost("/urls", (CreateShortUrlRequest request, UrlShortenerService shortener) =>
{
    try
    {
        var shortenedUrl = shortener.Shorten(request.Url);
        return Results.Created($"/{shortenedUrl.Code}", shortenedUrl);
    }
    catch (ArgumentException exception)
    {
        return Results.BadRequest(new { error = exception.Message });
    }
});

app.MapGet("/{code}", (string code, IUrlRepository repository) =>
    repository.Find(code) is { } shortenedUrl
        ? Results.Redirect(shortenedUrl.OriginalUrl)
        : Results.NotFound());

app.Run();

public partial class Program;
