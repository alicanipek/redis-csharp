using System;
using System.Collections.Concurrent;

namespace codecrafters_redis.src.Services;

// public class SetItemComparer : IComparer<SetItem>
// {
//     public int Compare(SetItem? x, SetItem? y)
//     {
//         if (x == null && y == null) return 0;
//         if (x == null) return -1;
//         if (y == null) return 1;

//         var scoreComparison = x.Score.CompareTo(y.Score);
//         return scoreComparison != 0 ? scoreComparison : -1;
//     }
// }

public class SetItem : IComparable<SetItem>, IEquatable<SetItem>
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

    public bool Equals(SetItem? other)
    {
        if (other == null) return false;
        return Member == other.Member;
    }
}

public class SortedSetStorageService
{
    private readonly ConcurrentDictionary<string, SortedDictionary<string, double>> SortedSets = new();

    public int ZAdd(string key, IEnumerable<SetItem> members)
    {
        var sortedSet = SortedSets.GetOrAdd(key, _ => []);
        int addedCount = 0;
        foreach (var member in members)
        {
            if (sortedSet.TryAdd(member.Member, member.Score))
            {
                addedCount++;
            }
            else
            {
                sortedSet[member.Member] = member.Score; // Update score if member exists
            }
        }
        return addedCount;
    }
}
