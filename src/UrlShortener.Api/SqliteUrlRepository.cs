using Microsoft.Data.Sqlite;

namespace UrlShortener.Api;

public sealed class SqliteUrlRepository : IUrlRepository
{
    private readonly SqliteConnection _connection;

    public SqliteUrlRepository(SqliteConnection connection)
    {
        _connection = connection;

        using var command = _connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS ShortenedUrls (
                Code TEXT PRIMARY KEY,
                OriginalUrl TEXT NOT NULL
            );
            """;
        command.ExecuteNonQuery();
    }

    public void Add(ShortenedUrl shortenedUrl)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = "INSERT INTO ShortenedUrls (Code, OriginalUrl) VALUES ($code, $url);";
        command.Parameters.AddWithValue("$code", shortenedUrl.Code);
        command.Parameters.AddWithValue("$url", shortenedUrl.OriginalUrl);
        command.ExecuteNonQuery();
    }

    public ShortenedUrl? Find(string code)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = "SELECT Code, OriginalUrl FROM ShortenedUrls WHERE Code = $code;";
        command.Parameters.AddWithValue("$code", code);

        using var reader = command.ExecuteReader();
        return reader.Read()
            ? new ShortenedUrl(reader.GetString(0), reader.GetString(1))
            : null;
    }
}
