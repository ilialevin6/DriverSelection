using DriverSelection;

namespace DriverSelection.Tests;

public class DriverStoreTests
{
    [Test]
    public void Add_PutsDriverOnCell()
    {
        var store = new DriverStore(10, 10);
        store.AddOrUpdate(1, 3, 4);

        Assert.That(store.Count, Is.EqualTo(1));
        var d = store.GetById(1);
        Assert.That(d, Is.Not.Null);
        Assert.That(d!.X, Is.EqualTo(3));
        Assert.That(d.Y, Is.EqualTo(4));
        Assert.That(store.GetAt(3, 4)!.Id, Is.EqualTo(1));
    }

    [Test]
    public void Add_TwoDriversOnSameCell_Throws()
    {
        var store = new DriverStore(10, 10);
        store.AddOrUpdate(1, 2, 2);

        var ex = Assert.Throws<InvalidOperationException>(() => store.AddOrUpdate(2, 2, 2));
        Assert.That(ex!.Message, Does.Contain("#1"));
    }

    [Test]
    public void Update_MovesDriverAndFreesOldCell()
    {
        var store = new DriverStore(10, 10);
        store.AddOrUpdate(7, 0, 0);
        store.AddOrUpdate(7, 5, 6);

        Assert.That(store.Count, Is.EqualTo(1));
        Assert.That(store.GetAt(0, 0), Is.Null);
        Assert.That(store.GetAt(5, 6)!.Id, Is.EqualTo(7));
        Assert.That(store.GetById(7)!.X, Is.EqualTo(5));
    }

    [Test]
    public void Update_ToOccupiedCell_Throws()
    {
        var store = new DriverStore(10, 10);
        store.AddOrUpdate(1, 1, 1);
        store.AddOrUpdate(2, 2, 2);

        Assert.Throws<InvalidOperationException>(() => store.AddOrUpdate(1, 2, 2));
    }

    [Test]
    public void Add_OutOfBounds_Throws()
    {
        var store = new DriverStore(5, 5);
        Assert.Throws<ArgumentOutOfRangeException>(() => store.AddOrUpdate(1, 5, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => store.AddOrUpdate(1, 0, 5));
        Assert.Throws<ArgumentOutOfRangeException>(() => store.AddOrUpdate(1, -1, 0));
    }

    [Test]
    public void Remove_DeletesDriver()
    {
        var store = new DriverStore(8, 8);
        store.AddOrUpdate(3, 4, 4);
        Assert.That(store.Remove(3), Is.True);
        Assert.That(store.Count, Is.EqualTo(0));
        Assert.That(store.GetAt(4, 4), Is.Null);
        Assert.That(store.Remove(3), Is.False);
    }
}
