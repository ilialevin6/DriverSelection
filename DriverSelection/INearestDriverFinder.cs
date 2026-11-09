namespace DriverSelection;

public interface INearestDriverFinder
{
    string Name { get; }

    /// <summary>
    /// Найти k ближайших к заказу водителей.
    /// Порядок результата: сначала ближе, при равной дистанции — меньший Id.
    /// Если водителей меньше k — вернуть всех.
    /// </summary>
    List<DriverMatch> Find(
        IReadOnlyList<Driver> drivers,
        int mapWidth,
        int mapHeight,
        Order order,
        int k = 5);
}
