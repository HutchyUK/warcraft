# ADR-0005: Heroic Dungeons — Separate DailyTasks Table

**Status:** Accepted  
**Date:** 2026-04-04  
**Triggered by:** Adversarial design review (iteration 2)

---

## Context

The initial design placed all repeatable tasks in a single `WeeklyTasks` table with a
`week_start` date column. During adversarial review, a reviewer identified that heroic
dungeon lockouts in TBC Classic reset **daily at midnight UTC**, not weekly with the
Tuesday/Wednesday raid lockout.

Storing heroic dungeons in `WeeklyTasks` would produce incorrect behaviour:

- A player who runs Shadow Labyrinth on Tuesday would see it as "done" for the rest of
  the week, even though the lockout resets on Wednesday.
- The `week_start` logic (last Tuesday/Wednesday) does not map onto a daily boundary.

Two fixes were considered:

| Option | Description |
|--------|-------------|
| **Add a `reset_type` column** | Add `reset_type ENUM('weekly', 'daily')` to `WeeklyTasks`. Query uses `week_start` for weekly tasks and `CURRENT_DATE` for daily tasks, with the column deciding which. |
| **Separate `DailyTasks` table** | A new table with `day_start DATE` (UTC calendar day) instead of `week_start`. Heroic dungeons live here exclusively. |

The single-table approach with `reset_type` conflates two different temporal concepts
into one table, complicating queries and the template-merge logic. A fresh table is
cleaner.

## Decision

Create a separate **`DailyTasks`** table with `day_start DATE` (UTC calendar day).
Heroic dungeons are stored exclusively in this table.

```
DailyTasks
  id               INT
  character_id     INT (FK → Characters)
  task_key         TEXT        -- e.g. "heroic_shadow_labs"
  task_name        TEXT
  day_start        DATE        -- UTC calendar date; "today" = DateOnly.FromDateTime(DateTime.UtcNow.Date)
  checked_at       TIMESTAMP?  -- NULL = not done today
```

`ResetService.GetCurrentDayStart()` returns `DateOnly.FromDateTime(DateTime.UtcNow.Date)`.
The dashboard query uses `WHERE day_start = today` — heroics not in the DB for today
render as unchecked (same lazy-insert pattern as ADR-0004).

`WeeklyTasks` retains only raid lockouts and weekly quests (`task_type = "RAID"` or
`"WEEKLY_QUEST"`). The `task_type` field in `WeeklyTasks` is kept for future weekly
quest tracking but heroic dungeons no longer appear there.

## Consequences

**Positive:**
- The temporal semantics of each table are unambiguous: `WeeklyTasks.week_start` is
  always a Tuesday/Wednesday; `DailyTasks.day_start` is always a UTC calendar date.
- Query logic for each reset type is independent — changing heroic reset logic does not
  risk touching raid logic.
- Enables future distinction between daily resets and other potential sub-weekly cadences
  (e.g. a 3-day quest) without a schema change.

**Negative:**
- Two tables to query and merge instead of one. The dashboard controller loads from
  both and merges each with its template list separately — minor code duplication.
- Two unique indexes required: `(CharacterId, TaskKey, WeekStart)` for weekly,
  `(CharacterId, TaskKey, DayStart)` for daily.

**Neutral:**
- `ProfessionCooldowns` has its own reset model (last-used + period, independent of any
  calendar boundary) and was never a candidate for either table. See `ResetService`.
