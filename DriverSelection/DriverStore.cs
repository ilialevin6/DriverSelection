namespace DriverSelection;

/// <summary>
/// Карта N×M. В клетке максимум один водитель.
/// AddOrUpdate — добавить нового или подвинуть существующего по Id.
/// </summary>
public sealed class DriverStore
{
    private readonly Dictionary<int, Driver> _byId = new();
    private readonly Dictionary<(int X, int Y), int> _cell = new();

    public int Width { get; }
    public int Height { get; }
    public int Count => _byId.Count;

    public DriverStore(int width, int height)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        Width = width;
        Height = height;
    }

    public IReadOnlyCollection<Driver> GetAll() => _byId.Values;

    public Driver? GetById(int id) => _byId.TryGetValue(id, out var d) ? d : null;

    public Driver? GetAt(int x, int y)
        => _cell.TryGetValue((x, y), out var id) ? _byId[id] : null;

    public void AddOrUpdate(int id, int x, int y)
    {
        CheckBounds(x, y);

        if (_cell.TryGetValue((x, y), out var occupant) && occupant != id)
            throw new InvalidOperationException(
                $"Клетка ({x}, {y}) уже занята водителем #{occupant}");

        if (_byId.TryGetValue(id, out var existing))
        {
            _cell.Remove((existing.X, existing.Y));
            existing.X = x;
            existing.Y = y;
            _cell[(x, y)] = id;
            return;
        }

        var driver = new Driver(id, x, y);
        _byId[id] = driver;
        _cell[(x, y)] = id;
    }

    public bool Remove(int id)
    {
        if (!_byId.TryGetValue(id, out var driver))
            return false;

        _byId.Remove(id);
        _cell.Remove((driver.X, driver.Y));
        return true;
    }

    public List<DriverMatch> FindNearest(Order order, INearestDriverFinder finder, int k = 5)
    {
        CheckBounds(order.X, order.Y);
        if (k <= 0) throw new ArgumentOutOfRangeException(nameof(k));
        return finder.Find(_byId.Values.ToList(), Width, Height, order, k);
    }

    private void CheckBounds(int x, int y)
    {
        if (x < 0 || x >= Width)
            throw new ArgumentOutOfRangeException(nameof(x), $"X должен быть в [0; {Width})");
        if (y < 0 || y >= Height)
            throw new ArgumentOutOfRangeException(nameof(y), $"Y должен быть в [0; {Height})");
    }
}
