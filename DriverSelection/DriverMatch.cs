namespace DriverSelection;

/// <summary>
/// Результат поиска: водитель + квадрат евклидова расстояния до заказа.
/// DistanceSquared удобнее для сравнений (без sqrt), Distance — уже «нормальное» расстояние.
/// </summary>
public sealed class DriverMatch
{
    public Driver Driver { get; }
    public long DistanceSquared { get; }
    public double Distance => Math.Sqrt(DistanceSquared);

    public DriverMatch(Driver driver, long distanceSquared)
    {
        Driver = driver;
        DistanceSquared = distanceSquared;
    }

    public override string ToString() =>
        $"{Driver}  dist={Distance:0.###}";
}
