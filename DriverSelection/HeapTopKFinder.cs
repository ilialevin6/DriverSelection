namespace DriverSelection;

/// <summary>
/// Алгоритм 2. Тот же перебор, но вместо полной сортировки держим кучу размера k.
/// Худший в куче — самый далёкий из текущих k. Если кандидат лучше — вытесняем.
/// Сложность O(n log k), памяти O(k). На k=5 это уже заметно быстрее полной сортировки.
/// </summary>
public sealed class HeapTopKFinder : INearestDriverFinder
{
    public string Name => "Top-K heap";

    public List<DriverMatch> Find(
        IReadOnlyList<Driver> drivers,
        int mapWidth,
        int mapHeight,
        Order order,
        int k = 5)
    {
        if (k <= 0) return new List<DriverMatch>();
        if (drivers.Count == 0) return new List<DriverMatch>();

        // max-heap: наверху самый «плохой» из текущих k (дальше / больше Id)
        var heap = new PriorityQueue<DriverMatch, WorstKey>();

        for (int i = 0; i < drivers.Count; i++)
        {
            var d = drivers[i];
            long dist2 = Geo.Dist2(d, order);
            var match = new DriverMatch(d, dist2);

            if (heap.Count < k)
            {
                heap.Enqueue(match, new WorstKey(dist2, d.Id));
                continue;
            }

            heap.TryPeek(out _, out var worst);
            if (MatchComparer.Compare(dist2, d.Id, worst.Dist2, worst.Id) < 0)
            {
                heap.Dequeue();
                heap.Enqueue(match, new WorstKey(dist2, d.Id));
            }
        }

        var result = new List<DriverMatch>(heap.Count);
        while (heap.Count > 0)
            result.Add(heap.Dequeue());

        result.Reverse(); // доставали от худшего к лучшему
        result.Sort((a, b) => MatchComparer.Compare(
            a.DistanceSquared, a.Driver.Id,
            b.DistanceSquared, b.Driver.Id));
        return result;
    }

    // Для PriorityQueue: больший Dist2 / больший Id = выше приоритет (хуже).
    private readonly struct WorstKey : IComparable<WorstKey>
    {
        public readonly long Dist2;
        public readonly int Id;

        public WorstKey(long dist2, int id)
        {
            Dist2 = dist2;
            Id = id;
        }

        public int CompareTo(WorstKey other)
        {
            // max-heap: сравниваем наоборот
            int c = other.Dist2.CompareTo(Dist2);
            return c != 0 ? c : other.Id.CompareTo(Id);
        }
    }
}
