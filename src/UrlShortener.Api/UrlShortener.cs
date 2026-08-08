namespace UrlShortener.Api;

public sealed record CreateShortUrlRequest(string Url);

public sealed record ShortenedUrl(string Code, string OriginalUrl);

public interface IUrlRepository
{
    void Add(ShortenedUrl shortenedUrl);
    ShortenedUrl? Find(string code);
}

public sealed class InMemoryUrlRepository : IUrlRepository
{
    private readonly Dictionary<string, ShortenedUrl> _urls = [];

    public void Add(ShortenedUrl shortenedUrl) => _urls[shortenedUrl.Code] = shortenedUrl;

    public ShortenedUrl? Find(string code) => _urls.GetValueOrDefault(code);
}

public sealed class UrlShortenerService(IUrlRepository repository)
{
    public ShortenedUrl Shorten(string url)
    {
        if (!UrlValidation.IsValid(url))
        {
            throw new ArgumentException(UrlValidation.ErrorMessage, nameof(url));
        }

        string code;
        do
        {
            code = Guid.NewGuid().ToString("N")[..7];
        }
        while (repository.Find(code) is not null);

        var shortenedUrl = new ShortenedUrl(code, url);
        repository.Add(shortenedUrl);
        return shortenedUrl;
    }
}

public static class UrlValidation
{
    public const string ErrorMessage = "URL must be an absolute HTTP or HTTPS URL.";

    public static bool IsValid(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
