using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UrlShortener.Api;

namespace UrlShortener.IntegrationTests;

public sealed class SqliteApiTests : IClassFixture<SqliteApiFactory>
{
    private readonly SqliteApiFactory _factory;

    public SqliteApiTests(SqliteApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Api_ShortensPersistsAndRedirectsUrl()
    {
        const string originalUrl = "https://example.com/persisted";
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var postResponse = await client.PostAsJsonAsync("/urls", new { url = originalUrl });
        var shortenedUrl = await postResponse.Content.ReadFromJsonAsync<ShortenedUrl>();

        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        using var scope = _factory.Services.CreateScope();
        var stored = scope.ServiceProvider.GetRequiredService<IUrlRepository>().Find(shortenedUrl!.Code);
        Assert.Equal(originalUrl, stored?.OriginalUrl);

        var redirectResponse = await client.GetAsync($"/{shortenedUrl.Code}");
        Assert.Equal(HttpStatusCode.Redirect, redirectResponse.StatusCode);
        Assert.Equal(originalUrl, redirectResponse.Headers.Location!.ToString());
    }
}

public sealed class SqliteApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public SqliteApiFactory() => _connection.Open();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<SqliteConnection>();
            services.RemoveAll<IUrlRepository>();
            services.AddSingleton(_connection);
            services.AddSingleton<IUrlRepository, SqliteUrlRepository>();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
