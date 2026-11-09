namespace DriverSelection;

/// <summary>
/// Алгоритм 3. Поиск кольцами по сетке (Chebyshev / шахматное короля).
/// Карта и так клеточная, в клетке один водитель — можно идти от заказа наружу
/// квадратами радиуса 0, 1, 2, ... и рано остановиться.
///
/// Евклидово расстояние до клетки на кольце r не меньше r.
/// Как только набрали k штук и (r+1)^2 уже больше текущего k-го расстояния —
/// дальше искать бессмысленно.
///
/// На плотной карте, когда ближайшие рядом, работает быстро.
/// Если водителей мало и они далеко — придётся обойти много пустых клеток,
/// тогда обычный перебор выгоднее.
/// </summary>
public sealed class GridRingFinder : INearestDriverFinder
{
    public string Name => "Grid rings";

    public List<DriverMatch> Find(
        IReadOnlyList<Driver> drivers,
        int mapWidth,
        int mapHeight,
        Order order,
        int k = 5)
    {
        if (drivers.Count == 0 || k <= 0)
            return new List<DriverMatch>();

        var cell = new Dictionary<(int X, int Y), Driver>(drivers.Count);
        for (int i = 0; i < drivers.Count; i++)
        {
            var d = drivers[i];
            cell[(d.X, d.Y)] = d;
        }

        var found = new List<DriverMatch>(Math.Min(k * 4, drivers.Count));
        int maxR = Math.Max(mapWidth, mapHeight);

        for (int r = 0; r <= maxR; r++)
        {
            VisitRing(cell, mapWidth, mapHeight, order, r, found);

            if (found.Count == drivers.Count)
                break;

            if (found.Count >= k)
            {
                long kth = KthDist2(found, k);
                long minNext = (long)(r + 1) * (r + 1);
                // следующее кольцо не может дать Dist2 < minNext
                if (minNext > kth)
                    break;
            }
        }

        return MatchComparer.SortAndTake(found, k);
    }

    private static void VisitRing(
        Dictionary<(int X, int Y), Driver> cell,
        int width,
        int height,
        Order order,
        int r,
        List<DriverMatch> found)
    {
        if (r == 0)
        {
            TryAdd(cell, order.X, order.Y, order, found);
            return;
        }

        int x0 = order.X - r;
        int x1 = order.X + r;
        int y0 = order.Y - r;
        int y1 = order.Y + r;

        // верх и низ квадрата
        for (int x = x0; x <= x1; x++)
        {
            TryAdd(cell, x, y0, order, found);
            TryAdd(cell, x, y1, order, found);
        }

        // бока без углов (углы уже взяли)
        for (int y = y0 + 1; y <= y1 - 1; y++)
        {
            TryAdd(cell, x0, y, order, found);
            TryAdd(cell, x1, y, order, found);
        }
    }

    private static void TryAdd(
        Dictionary<(int X, int Y), Driver> cell,
        int x,
        int y,
        Order order,
        List<DriverMatch> found)
    {
        if (!cell.TryGetValue((x, y), out var driver))
            return;

        found.Add(new DriverMatch(driver, Geo.Dist2(driver, order)));
    }

    private static long KthDist2(List<DriverMatch> found, int k)
    {
        // k небольшое (обычно 5), сортировка копии нормальная
        var copy = found.ToArray();
        Array.Sort(copy, (a, b) => MatchComparer.Compare(
            a.DistanceSquared, a.Driver.Id,
            b.DistanceSquared, b.Driver.Id));
        int idx = Math.Min(k, copy.Length) - 1;
        return copy[idx].DistanceSquared;
    }
}
