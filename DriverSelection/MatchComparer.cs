namespace DriverSelection;

/// <summary>
/// Общее правило сортировки: меньше расстояние лучше,
/// при равенстве берём меньший Id (чтобы алгоритмы сходились 1 в 1).
/// </summary>
internal static class MatchComparer
{
    public static int Compare(long dist2A, int idA, long dist2B, int idB)
    {
        var byDist = dist2A.CompareTo(dist2B);
        return byDist != 0 ? byDist : idA.CompareTo(idB);
    }

    public static List<DriverMatch> SortAndTake(List<DriverMatch> matches, int k)
    {
        matches.Sort((a, b) => Compare(a.DistanceSquared, a.Driver.Id, b.DistanceSquared, b.Driver.Id));
        if (matches.Count > k)
            matches.RemoveRange(k, matches.Count - k);
        return matches;
    }
}
