namespace Warcraft.Api.Services;

public static class ResetService
{
    /// <summary>
    /// Returns the most recent Tuesday (US) or Wednesday (EU) at midnight UTC —
    /// the start of the current weekly reset period for this character's region.
    /// </summary>
    public static DateOnly GetCurrentWeekStart(string region)
        => GetCurrentWeekStart(region, DateTime.UtcNow);

    /// <summary>
    /// Overload that accepts an explicit clock value — use in tests for deterministic results.
    /// </summary>
    public static DateOnly GetCurrentWeekStart(string region, DateTime now)
    {
        var targetDay = region.ToUpperInvariant() == "EU"
            ? DayOfWeek.Wednesday
            : DayOfWeek.Tuesday;

        var daysBack = ((int)now.DayOfWeek - (int)targetDay + 7) % 7;
        return DateOnly.FromDateTime(now.AddDays(-daysBack).Date);
    }

    /// <summary>Returns today's UTC calendar date (resets at midnight UTC).</summary>
    public static DateOnly GetCurrentDayStart()
        => DateOnly.FromDateTime(DateTime.UtcNow.Date);

    /// <summary>
    /// Returns true if the profession CD is ready to use:
    /// never used, or enough time has passed since last use.
    /// </summary>
    public static bool IsProfessionCdReady(DateTime? lastUsedAt, int periodDays)
        => lastUsedAt == null || DateTime.UtcNow >= lastUsedAt.Value.AddDays(periodDays);

    /// <summary>
    /// Returns when the profession CD will be ready (null if already ready).
    /// </summary>
    public static DateTime? GetProfessionCdReadyAt(DateTime? lastUsedAt, int periodDays)
    {
        if (lastUsedAt == null) return null;
        var readyAt = lastUsedAt.Value.AddDays(periodDays);
        return DateTime.UtcNow < readyAt ? readyAt : null;
    }
}
