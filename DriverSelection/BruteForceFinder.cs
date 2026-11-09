namespace DriverSelection;

/// <summary>
/// Алгоритм 1. Полный перебор: считаем расстояние до каждого, сортируем, берём k.
/// Простой, всегда правильный, на больших N уже медленный (O(n log n)).
/// </summary>
public sealed class BruteForceFinder : INearestDriverFinder
{
    public string Name => "Brute force (sort)";

    public List<DriverMatch> Find(
        IReadOnlyList<Driver> drivers,
        int mapWidth,
        int mapHeight,
        Order order,
        int k = 5)
    {
        var matches = new List<DriverMatch>(drivers.Count);
        for (int i = 0; i < drivers.Count; i++)
        {
            var d = drivers[i];
            matches.Add(new DriverMatch(d, Geo.Dist2(d, order)));
        }

        return MatchComparer.SortAndTake(matches, k);
    }
}
