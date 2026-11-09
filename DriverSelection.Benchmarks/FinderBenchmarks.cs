using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DriverSelection;

namespace DriverSelection.Benchmarks;

[MemoryDiagnoser]
[RankColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[SimpleJob(warmupCount: 2, iterationCount: 8)]
public class FinderBenchmarks
{
    // небольшая / средняя / побольше выборка, чтобы было видно разницу
    [Params(500, 5000, 25000)]
    public int DriverCount { get; set; }

    private const int MapN = 1000;
    private const int MapM = 1000;
    private const int K = 5;

    private List<Driver> _drivers = null!;
    private Order _order = null!;

    private readonly BruteForceFinder _brute = new();
    private readonly HeapTopKFinder _heap = new();
    private readonly GridRingFinder _grid = new();
    private readonly KdTreeFinder _kd = new();

    [GlobalSetup]
    public void Setup()
    {
        var rng = new Random(42);
        var used = new HashSet<(int, int)>();
        _drivers = new List<Driver>(DriverCount);

        int id = 1;
        while (_drivers.Count < DriverCount)
        {
            int x = rng.Next(MapN);
            int y = rng.Next(MapM);
            if (!used.Add((x, y))) continue;
            _drivers.Add(new Driver(id++, x, y));
        }

        _order = new Order(MapN / 2, MapM / 2);
    }

    [Benchmark(Baseline = true)]
    public int BruteForce() => _brute.Find(_drivers, MapN, MapM, _order, K).Count;

    [Benchmark]
    public int HeapTopK() => _heap.Find(_drivers, MapN, MapM, _order, K).Count;

    [Benchmark]
    public int GridRings() => _grid.Find(_drivers, MapN, MapM, _order, K).Count;

    [Benchmark]
    public int KdTree() => _kd.Find(_drivers, MapN, MapM, _order, K).Count;
}
