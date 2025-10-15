using System;
using System.Collections.Concurrent;

namespace codecrafters_redis.src.Services;

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
    private readonly ConcurrentDictionary<string, SortedSet<SetItem>> SortedSets = new();

    public int ZAdd(string key, IEnumerable<SetItem> members)
    {
        var sortedSet = SortedSets.GetOrAdd(key, _ => new SortedSet<SetItem>());
        int addedCount = 0;
        foreach (var member in members)
        {
            // Remove existing member if it exists (to update score)
            var existingMember = sortedSet.FirstOrDefault(x => x.Member == member.Member);
            if (existingMember != null)
            {
                sortedSet.Remove(existingMember);
            }
            else
            {
                addedCount++;
            }
            sortedSet.Add(member);
        }
        return addedCount;
    }

    public int ZRank(string key, string member)
    {
        if (!SortedSets.TryGetValue(key, out var sortedSet))
        {
            return -1;
        }

        var index = 0;
        foreach (var item in sortedSet)
        {
            if (item.Member == member)
            {
                return index;
            }
            index++;
        }
        return -1;
    }

    public int ZRevRank(string key, string member)
    {
        if (!SortedSets.TryGetValue(key, out var sortedSet))
        {
            return -1;
        }

        var reversedList = sortedSet.Reverse().ToList();
        for (int i = 0; i < reversedList.Count; i++)
        {
            if (reversedList[i].Member == member)
            {
                return i;
            }
        }
        return -1;
    }

    public List<string> ZRange(string key, int start, int stop, bool withScores = false)
    {
        if (!SortedSets.TryGetValue(key, out var sortedSet))
        {
            return new List<string>();
        }

        var items = sortedSet.ToList();
        
        // Handle negative indices
        if (start < 0) start = items.Count + start;
        if (stop < 0) stop = items.Count + stop;
        
        // Clamp indices
        start = Math.Max(0, start);
        stop = Math.Min(items.Count - 1, stop);
        
        if (start > stop || start >= items.Count)
        {
            return new List<string>();
        }

        var result = new List<string>();
        for (int i = start; i <= stop; i++)
        {
            result.Add(items[i].Member);
            if (withScores)
            {
                result.Add(items[i].Score.ToString());
            }
        }
        
        return result;
    }

    public List<string> ZRevRange(string key, int start, int stop, bool withScores = false)
    {
        if (!SortedSets.TryGetValue(key, out var sortedSet))
        {
            return new List<string>();
        }

        var items = sortedSet.Reverse().ToList();
        
        // Handle negative indices
        if (start < 0) start = items.Count + start;
        if (stop < 0) stop = items.Count + stop;
        
        // Clamp indices
        start = Math.Max(0, start);
        stop = Math.Min(items.Count - 1, stop);
        
        if (start > stop || start >= items.Count)
        {
            return new List<string>();
        }

        var result = new List<string>();
        for (int i = start; i <= stop; i++)
        {
            result.Add(items[i].Member);
            if (withScores)
            {
                result.Add(items[i].Score.ToString());
            }
        }
        
        return result;
    }

    public double ZIncrBy(string key, string member, double increment)
    {
        var sortedSet = SortedSets.GetOrAdd(key, _ => new SortedSet<SetItem>());
        
        // Find existing member
        var existingMember = sortedSet.FirstOrDefault(x => x.Member == member);
        double newScore;
        
        if (existingMember != null)
        {
            newScore = existingMember.Score + increment;
            sortedSet.Remove(existingMember);
        }
        else
        {
            newScore = increment;
        }
        
        sortedSet.Add(new SetItem(newScore, member));
        return newScore;
    }

    public int ZRem(string key, IEnumerable<string> members)
    {
        if (!SortedSets.TryGetValue(key, out var sortedSet))
        {
            return 0;
        }

        int removedCount = 0;
        foreach (var member in members)
        {
            var existingMember = sortedSet.FirstOrDefault(x => x.Member == member);
            if (existingMember != null)
            {
                sortedSet.Remove(existingMember);
                removedCount++;
            }
        }

        // Remove empty sorted set
        if (sortedSet.Count == 0)
        {
            SortedSets.TryRemove(key, out _);
        }

        return removedCount;
    }

    public int ZCard(string key)
    {
        if (!SortedSets.TryGetValue(key, out var sortedSet))
        {
            return 0;
        }
        return sortedSet.Count;
    }

    public double? ZScore(string key, string member)
    {
        if (!SortedSets.TryGetValue(key, out var sortedSet))
        {
            return null;
        }

        var existingMember = sortedSet.FirstOrDefault(x => x.Member == member);
        return existingMember?.Score;
    }
}
