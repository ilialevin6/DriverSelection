namespace DriverSelection;

/// <summary>
/// Алгоритм 4. KD-дерево (2D). Делим плоскость по очереди по X и по Y,
/// в узле — медиана. Поиск k ближайших: идём сначала в «близкое» поддерево,
/// дальнее трогаем только если плоскость разреза может дать кого-то ближе
/// текущего k-го.
///
/// Построение O(n log n), запрос в среднем сильно лучше линейного,
/// худший случай (вырожденное дерево) близок к O(n).
/// </summary>
public sealed class KdTreeFinder : INearestDriverFinder
{
    public string Name => "KD-tree";

    public List<DriverMatch> Find(
        IReadOnlyList<Driver> drivers,
        int mapWidth,
        int mapHeight,
        Order order,
        int k = 5)
    {
        if (drivers.Count == 0 || k <= 0)
            return new List<DriverMatch>();

        var arr = drivers.ToArray();
        var root = Build(arr, 0, arr.Length - 1, axis: 0);

        var heap = new PriorityQueue<DriverMatch, WorstKey>();
        Search(root, order, k, heap);

        var result = new List<DriverMatch>(heap.Count);
        while (heap.Count > 0)
            result.Add(heap.Dequeue());

        result.Sort((a, b) => MatchComparer.Compare(
            a.DistanceSquared, a.Driver.Id,
            b.DistanceSquared, b.Driver.Id));
        return result;
    }

    private static KdNode? Build(Driver[] arr, int left, int right, int axis)
    {
        if (left > right) return null;

        int mid = (left + right) / 2;
        if (axis == 0)
            Select(arr, left, right, mid, (a, b) => a.X.CompareTo(b.X));
        else
            Select(arr, left, right, mid, (a, b) => a.Y.CompareTo(b.Y));

        return new KdNode
        {
            Driver = arr[mid],
            Axis = axis,
            Left = Build(arr, left, mid - 1, 1 - axis),
            Right = Build(arr, mid + 1, right, 1 - axis)
        };
    }

    private static void Search(KdNode? node, Order order, int k, PriorityQueue<DriverMatch, WorstKey> heap)
    {
        if (node is null) return;

        var d = node.Driver;
        long dist2 = Geo.Dist2(d, order);
        Consider(heap, k, new DriverMatch(d, dist2));

        int split = node.Axis == 0 ? d.X : d.Y;
        int value = node.Axis == 0 ? order.X : order.Y;
        long plane = (long)value - split;
        long plane2 = plane * plane;

        var near = value < split ? node.Left : node.Right;
        var far = value < split ? node.Right : node.Left;

        Search(near, order, k, heap);

        // дальнюю ветку имеет смысл смотреть, если куча ещё не набрана
        // или до плоскости ближе, чем до текущего худшего
        if (heap.Count < k || plane2 <= PeekWorst(heap).Dist2)
            Search(far, order, k, heap);
    }

    private static void Consider(PriorityQueue<DriverMatch, WorstKey> heap, int k, DriverMatch match)
    {
        if (heap.Count < k)
        {
            heap.Enqueue(match, new WorstKey(match.DistanceSquared, match.Driver.Id));
            return;
        }

        var worst = PeekWorst(heap);
        if (MatchComparer.Compare(match.DistanceSquared, match.Driver.Id, worst.Dist2, worst.Id) < 0)
        {
            heap.Dequeue();
            heap.Enqueue(match, new WorstKey(match.DistanceSquared, match.Driver.Id));
        }
    }

    private static WorstKey PeekWorst(PriorityQueue<DriverMatch, WorstKey> heap)
    {
        heap.TryPeek(out _, out var key);
        return key;
    }

    // nth_element, чтобы не сортировать весь кусок на каждом уровне
    private static void Select(Driver[] arr, int left, int right, int n, Comparison<Driver> cmp)
    {
        while (left < right)
        {
            int pivotIndex = Partition(arr, left, right, left + (right - left) / 2, cmp);
            if (n == pivotIndex) return;
            if (n < pivotIndex) right = pivotIndex - 1;
            else left = pivotIndex + 1;
        }
    }

    private static int Partition(Driver[] arr, int left, int right, int pivotIndex, Comparison<Driver> cmp)
    {
        var pivot = arr[pivotIndex];
        (arr[pivotIndex], arr[right]) = (arr[right], arr[pivotIndex]);
        int store = left;
        for (int i = left; i < right; i++)
        {
            if (cmp(arr[i], pivot) < 0)
            {
                (arr[store], arr[i]) = (arr[i], arr[store]);
                store++;
            }
        }
        (arr[store], arr[right]) = (arr[right], arr[store]);
        return store;
    }

    private sealed class KdNode
    {
        public Driver Driver = null!;
        public int Axis;
        public KdNode? Left;
        public KdNode? Right;
    }

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
            int c = other.Dist2.CompareTo(Dist2);
            return c != 0 ? c : other.Id.CompareTo(Id);
        }
    }
}
