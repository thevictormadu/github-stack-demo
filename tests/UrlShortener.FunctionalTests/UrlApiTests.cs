using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using UrlShortener.Api;

namespace UrlShortener.FunctionalTests;

public sealed class UrlApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UrlApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task PostUrl_ValidUrl_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/urls", new { url = "https://example.com/article" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var shortenedUrl = await response.Content.ReadFromJsonAsync<ShortenedUrl>();
        Assert.NotNull(shortenedUrl);
        Assert.Equal("https://example.com/article", shortenedUrl.OriginalUrl);
    }

    [Fact]
    public async Task PostUrl_InvalidUrl_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/urls", new { url = "not-a-url" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetCode_AfterShortening_RedirectsToOriginalUrl()
    {
        const string originalUrl = "https://example.com/articles/github-stacks";
        var postResponse = await _client.PostAsJsonAsync("/urls", new { url = originalUrl });
        var shortenedUrl = await postResponse.Content.ReadFromJsonAsync<ShortenedUrl>();

        var redirectResponse = await _client.GetAsync($"/{shortenedUrl!.Code}");

        Assert.Equal(HttpStatusCode.Redirect, redirectResponse.StatusCode);
        Assert.Equal(originalUrl, redirectResponse.Headers.Location!.ToString());
    }
}
