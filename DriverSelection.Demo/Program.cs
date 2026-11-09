using DriverSelection;

var store = new DriverStore(width: 20, height: 15);

store.AddOrUpdate(1, 2, 3);
store.AddOrUpdate(2, 10, 8);
store.AddOrUpdate(3, 4, 4);
store.AddOrUpdate(4, 18, 1);
store.AddOrUpdate(5, 7, 7);
store.AddOrUpdate(6, 0, 14);
store.AddOrUpdate(7, 11, 9);
store.AddOrUpdate(8, 5, 5);

var order = new Order(6, 6);
Console.WriteLine($"Карта {store.Width}x{store.Height}, водителей: {store.Count}");
Console.WriteLine($"Заказ: {order}");
Console.WriteLine();

INearestDriverFinder[] finders =
{
    new BruteForceFinder(),
    new HeapTopKFinder(),
    new GridRingFinder(),
    new KdTreeFinder()
};

foreach (var finder in finders)
{
    var matches = store.FindNearest(order, finder, k: 5);
    Console.WriteLine($"[{finder.Name}]");
    foreach (var m in matches)
        Console.WriteLine($"  #{m.Driver.Id,-3} ({m.Driver.X,2},{m.Driver.Y,2})  dist={m.Distance:0.###}");
    Console.WriteLine();
}
