namespace AngryMonkey.CloudCommon.Models;

public sealed record DateTimeRange
{
    public DateTimeOffset Start { get; }
    public DateTimeOffset End { get; }
    public DateTimeRange(DateTimeOffset start, DateTimeOffset end)
    {
        if (end <= start)
            throw new ArgumentException("End must follow start.", nameof(end));
        Start = start;
        End = end;
    }
    public bool Contains(DateTimeOffset instant) => instant >= Start && instant < End;
    public bool Overlaps(DateTimeRange other) => Start < other.End && other.Start < End;
    public TimeSpan Duration => End - Start;
}

