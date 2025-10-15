using System;
using System.Collections.Concurrent;

namespace codecrafters_redis.src.Services;

public class SetItem : IComparable<SetItem>
{
    public double Score { get; set; }
    public string Member { get; set; }

    public SetItem(double score, string member)
    {
        Score = score;
        Member = member;
    }

    public int CompareTo(SetItem? other)
    {
        if (other == null) return 1;

        var scoreComparison = Score.CompareTo(other.Score);
        return scoreComparison != 0 ? scoreComparison : string.Compare(Member, other.Member, StringComparison.Ordinal);
    }
}

public class SortedSetStorageService
{
    private readonly ConcurrentDictionary<string, SortedSet<SetItem>> SortedSets = new();

    public int ZAdd(string key, IEnumerable<SetItem> members)
    {
        var sortedSet = SortedSets.GetOrAdd(key, _ => []);

        int addedCount = 0;
        foreach (var member in members)
        {
            if (sortedSet.Add(member))
            {
                addedCount++;
            }
        }

        return addedCount;
    }
}
