using System.Text.RegularExpressions;
using CountryRoute.Api;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Serves the React UI (built into wwwroot) at "/".
app.UseDefaultFiles();
app.UseStaticFiles();

string[] supported = [.. CountryGraph.Borders.Keys];

IResult Lookup(string code)
{
    if (!Regex.IsMatch(code, "^[A-Za-z]{3}$"))
        return Results.BadRequest(new { error = $"\"{code}\" is not a three-letter country code." });

    var destination = code.ToUpperInvariant();
    var list = CountryGraph.FindRoute(destination);
    return list is null
        ? Results.NotFound(new { error = $"No route from {CountryGraph.Origin} to \"{destination}\".", supported })
        : Results.Ok(new { destination, list });
}

// Usage info (the root path serves the UI).
app.MapGet("/api", () => Results.Ok(new
{
    usage = "GET /{three-letter country code}, e.g. /PAN",
    origin = CountryGraph.Origin,
    supported,
}));

// Same endpoint under /api (what the UI calls) and directly after the domain,
// as the brief's examples show (e.g. /PAN). Static files win over this route.
app.MapGet("/api/{code}", Lookup);
app.MapGet("/{code:alpha:length(3)}", Lookup);

app.Run();

public partial class Program; // lets the test project use WebApplicationFactory
