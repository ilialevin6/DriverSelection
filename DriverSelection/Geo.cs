namespace DriverSelection;

/// <summary>
/// Расстояние на сетке. Берём евклидово: sqrt(dx^2 + dy^2).
/// Для сравнений считаем квадрат, чтобы не таскать корень и не ловить погрешность.
/// </summary>
public static class Geo
{
    public static long Dist2(int x1, int y1, int x2, int y2)
    {
        long dx = x1 - x2;
        long dy = y1 - y2;
        return dx * dx + dy * dy;
    }

    public static long Dist2(Driver driver, Order order)
        => Dist2(driver.X, driver.Y, order.X, order.Y);
}
