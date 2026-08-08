var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "URL Shortener API");

app.Run();
