using UrlShortener.Api;

namespace UrlShortener.UnitTests;

public sealed class UrlShortenerServiceTests
{
    private readonly UrlShortenerService _shortener = new(new InMemoryUrlRepository());

    [Fact]
    public void Shorten_ValidUrl_GeneratesCodeAndPreservesUrl()
    {
        const string originalUrl = "https://example.com/articles/github-stacks";

        var result = _shortener.Shorten(originalUrl);

        Assert.NotEmpty(result.Code);
        Assert.Equal(7, result.Code.Length);
        Assert.Equal(originalUrl, result.OriginalUrl);
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("/relative/path")]
    [InlineData("")]
    public void Shorten_InvalidUrl_Throws(string url)
    {
        Assert.Throws<ArgumentException>(() => _shortener.Shorten(url));
    }

    [Theory]
    [InlineData("ftp://example.com/file")]
    [InlineData("mailto:hello@example.com")]
    public void Shorten_NonHttpUrl_Throws(string url)
    {
        Assert.Throws<ArgumentException>(() => _shortener.Shorten(url));
    }
}
