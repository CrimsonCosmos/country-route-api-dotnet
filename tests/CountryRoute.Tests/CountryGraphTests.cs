using CountryRoute.Api;

namespace CountryRoute.Tests;

public class CountryGraphTests
{
    [Fact]
    public void Pan_MatchesBrief() =>
        Assert.Equal(["USA", "MEX", "GTM", "HND", "NIC", "CRI", "PAN"], CountryGraph.FindRoute("PAN"));

    [Fact]
    public void Blz_MatchesBrief() =>
        Assert.Equal(["USA", "MEX", "BLZ"], CountryGraph.FindRoute("BLZ"));

    [Fact]
    public void EdgeCases_UsaCanMex()
    {
        Assert.Equal(["USA"], CountryGraph.FindRoute("USA"));
        Assert.Equal(["USA", "CAN"], CountryGraph.FindRoute("CAN"));
        Assert.Equal(["USA", "MEX"], CountryGraph.FindRoute("MEX"));
    }

    [Fact]
    public void Slv_TakesShortestPathViaGuatemala() =>
        Assert.Equal(["USA", "MEX", "GTM", "SLV"], CountryGraph.FindRoute("SLV"));

    [Fact]
    public void Lowercase_IsAccepted() =>
        Assert.Equal(["USA", "MEX", "BLZ"], CountryGraph.FindRoute("blz"));

    [Fact]
    public void UnknownCode_ReturnsNull() => Assert.Null(CountryGraph.FindRoute("FRA"));

    [Fact]
    public void EveryCountry_IsReachableViaValidBorders()
    {
        foreach (var code in CountryGraph.Borders.Keys)
        {
            var route = CountryGraph.FindRoute(code);
            Assert.NotNull(route);
            Assert.Equal("USA", route[0]);
            Assert.Equal(code, route[^1]);
            for (var i = 1; i < route.Count; i++)
                Assert.Contains(route[i], CountryGraph.Borders[route[i - 1]]);
        }
    }

    [Fact]
    public void BorderMap_IsSymmetric()
    {
        foreach (var (a, neighbors) in CountryGraph.Borders)
            foreach (var b in neighbors)
                Assert.Contains(a, CountryGraph.Borders[b]);
    }
}
