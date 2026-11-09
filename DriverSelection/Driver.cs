namespace DriverSelection;

/// <summary>
/// Водитель на карте. Id уникальный, в одной клетке может стоять только один.
/// </summary>
public sealed class Driver
{
    public int Id { get; }
    public int X { get; internal set; }
    public int Y { get; internal set; }

    public Driver(int id, int x, int y)
    {
        Id = id;
        X = x;
        Y = y;
    }

    public override string ToString() => $"Driver #{Id} ({X}, {Y})";
}
