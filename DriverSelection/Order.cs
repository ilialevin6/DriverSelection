namespace DriverSelection;

/// <summary>
/// Заказ. Координаты в пределах карты: 0 &lt;= X &lt; N, 0 &lt;= Y &lt; M.
/// </summary>
public sealed class Order
{
    public int X { get; }
    public int Y { get; }

    public Order(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override string ToString() => $"Order ({X}, {Y})";
}
