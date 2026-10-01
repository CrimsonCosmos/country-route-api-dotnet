namespace CountryRoute.Api;

/// <summary>
/// Simplified North American land-border map from the project brief, plus a
/// breadth-first search for the shortest land route between two countries.
/// </summary>
public static class CountryGraph
{
    public const string Origin = "USA";

    // Borders are listed in both directions on purpose; a unit test verifies symmetry.
    public static readonly IReadOnlyDictionary<string, string[]> Borders =
        new Dictionary<string, string[]>
        {
            ["CAN"] = ["USA"],
            ["USA"] = ["CAN", "MEX"],
            ["MEX"] = ["USA", "GTM", "BLZ"],
            ["BLZ"] = ["MEX", "GTM"],
            ["GTM"] = ["MEX", "BLZ", "SLV", "HND"],
            ["SLV"] = ["GTM", "HND"],
            ["HND"] = ["GTM", "SLV", "NIC"],
            ["NIC"] = ["HND", "CRI"],
            ["CRI"] = ["NIC", "PAN"],
            ["PAN"] = ["CRI"],
        };

    /// <summary>
    /// Route with the fewest border crossings from <paramref name="from"/> to
    /// <paramref name="to"/>, inclusive of both endpoints. Codes are
    /// case-insensitive. Returns null if either code is unknown or no route exists.
    /// </summary>
    public static IReadOnlyList<string>? FindRoute(string to, string from = Origin)
    {
        var start = from.ToUpperInvariant();
        var goal = to.ToUpperInvariant();
        if (!Borders.ContainsKey(start) || !Borders.ContainsKey(goal)) return null;

        var previous = new Dictionary<string, string?> { [start] = null };
        var queue = new Queue<string>();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == goal) break;
            foreach (var next in Borders[current])
            {
                if (previous.TryAdd(next, current)) queue.Enqueue(next);
            }
        }

        if (!previous.ContainsKey(goal)) return null;

        var route = new List<string>();
        for (string? c = goal; c is not null; c = previous[c]) route.Add(c);
        route.Reverse();
        return route;
    }
}
