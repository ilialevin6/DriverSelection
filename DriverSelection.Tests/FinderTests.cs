using DriverSelection;

namespace DriverSelection.Tests;

public class FinderTests
{
    private static readonly INearestDriverFinder[] All =
    {
        new BruteForceFinder(),
        new HeapTopKFinder(),
        new GridRingFinder(),
        new KdTreeFinder()
    };

    private static DriverStore MapWith(params (int id, int x, int y)[] drivers)
    {
        int maxX = 0, maxY = 0;
        foreach (var d in drivers)
        {
            if (d.x > maxX) maxX = d.x;
            if (d.y > maxY) maxY = d.y;
        }

        var store = new DriverStore(Math.Max(maxX + 1, 8), Math.Max(maxY + 1, 8));
        foreach (var d in drivers)
            store.AddOrUpdate(d.id, d.x, d.y);
        return store;
    }

    [TestCaseSource(nameof(All))]
    public void EmptyMap_ReturnsEmpty(INearestDriverFinder finder)
    {
        var store = new DriverStore(10, 10);
        var result = store.FindNearest(new Order(1, 1), finder, 5);
        Assert.That(result, Is.Empty);
    }

    [TestCaseSource(nameof(All))]
    public void LessThanK_ReturnsAllSorted(INearestDriverFinder finder)
    {
        var store = MapWith((1, 0, 0), (2, 3, 4), (3, 1, 0));
        var result = store.FindNearest(new Order(0, 0), finder, 5);

        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.Select(m => m.Driver.Id), Is.EqualTo(new[] { 1, 3, 2 }));
        Assert.That(result[0].DistanceSquared, Is.EqualTo(0));
        Assert.That(result[1].DistanceSquared, Is.EqualTo(1));
        Assert.That(result[2].DistanceSquared, Is.EqualTo(9 + 16));
    }

    [TestCaseSource(nameof(All))]
    public void FindsFiveClosest_KnownLayout(INearestDriverFinder finder)
    {
        // заказ в (5,5)
        // 1:(5,5) dist 0
        // 2:(6,5) dist 1
        // 3:(5,7) dist 2
        // 4:(8,5) dist 3
        // 5:(5,1) dist 4
        // 6:(0,0) далеко
        // 7:(9,9) далеко
        var store = MapWith(
            (6, 0, 0),
            (5, 5, 1),
            (1, 5, 5),
            (2, 6, 5),
            (3, 5, 7),
            (4, 8, 5),
            (7, 9, 9)
        );

        var result = store.FindNearest(new Order(5, 5), finder, 5);
        Assert.That(result.Select(m => m.Driver.Id), Is.EqualTo(new[] { 1, 2, 3, 4, 5 }));
    }

    [TestCaseSource(nameof(All))]
    public void TieByDistance_PrefersSmallerId(INearestDriverFinder finder)
    {
        // все на расстоянии 1 от (2,2), кроме #9 который дальше
        var store = MapWith(
            (4, 3, 2),
            (1, 1, 2),
            (7, 2, 3),
            (2, 2, 1),
            (9, 0, 0)
        );

        var result = store.FindNearest(new Order(2, 2), finder, 3);
        Assert.That(result.Select(m => m.Driver.Id), Is.EqualTo(new[] { 1, 2, 4 }));
        Assert.That(result.All(m => m.DistanceSquared == 1), Is.True);
    }

    [TestCaseSource(nameof(All))]
    public void DriverOnOrderCell_IsFirst(INearestDriverFinder finder)
    {
        var store = MapWith((10, 4, 4), (11, 0, 0), (12, 7, 7));
        var result = store.FindNearest(new Order(4, 4), finder, 5);
        Assert.That(result[0].Driver.Id, Is.EqualTo(10));
        Assert.That(result[0].DistanceSquared, Is.EqualTo(0));
    }

    [Test]
    public void AllAlgorithms_ReturnSameIds_RandomMaps()
    {
        var rng = new Random(12345);
        for (int iter = 0; iter < 25; iter++)
        {
            int n = rng.Next(12, 40);
            int m = rng.Next(12, 40);
            var store = new DriverStore(n, m);
            int count = rng.Next(0, Math.Min(80, n * m));
            int id = 1;
            var used = new HashSet<(int, int)>();
            while (store.Count < count)
            {
                int x = rng.Next(n);
                int y = rng.Next(m);
                if (!used.Add((x, y))) continue;
                store.AddOrUpdate(id++, x, y);
            }

            var order = new Order(rng.Next(n), rng.Next(m));
            int k = 5;

            var expected = store.FindNearest(order, All[0], k)
                .Select(x => (x.Driver.Id, x.DistanceSquared))
                .ToArray();

            foreach (var finder in All.Skip(1))
            {
                var actual = store.FindNearest(order, finder, k)
                    .Select(x => (x.Driver.Id, x.DistanceSquared))
                    .ToArray();

                Assert.That(actual, Is.EqualTo(expected),
                    $"расхождение у {finder.Name}, итерация {iter}, заказ {order}");
            }
        }
    }

    [Test]
    public void AfterMove_NearestChanges()
    {
        var store = MapWith((1, 0, 0), (2, 9, 9));
        var finder = new BruteForceFinder();

        var before = store.FindNearest(new Order(8, 8), finder, 1);
        Assert.That(before[0].Driver.Id, Is.EqualTo(2));

        store.AddOrUpdate(1, 8, 7);
        var after = store.FindNearest(new Order(8, 8), finder, 1);
        Assert.That(after[0].Driver.Id, Is.EqualTo(1));
    }
}
