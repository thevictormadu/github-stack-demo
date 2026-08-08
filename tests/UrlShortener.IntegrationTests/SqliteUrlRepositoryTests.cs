using Microsoft.Data.Sqlite;
using UrlShortener.Api;

namespace UrlShortener.IntegrationTests;

public sealed class SqliteUrlRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly SqliteUrlRepository _repository;

    public SqliteUrlRepositoryTests()
    {
        _connection.Open();
        _repository = new SqliteUrlRepository(_connection);
    }

    [Fact]
    public void Add_ThenFind_PersistsCodeAndOriginalUrl()
    {
        var shortenedUrl = new ShortenedUrl("abc1234", "https://example.com/original");

        _repository.Add(shortenedUrl);
        var stored = _repository.Find(shortenedUrl.Code);

        Assert.Equal(shortenedUrl.Code, stored?.Code);
        Assert.Equal(shortenedUrl.OriginalUrl, stored?.OriginalUrl);
    }

    [Fact]
    public void Find_UnknownCode_ReturnsNull()
    {
        Assert.Null(_repository.Find("missing"));
    }

    public void Dispose() => _connection.Dispose();
}
