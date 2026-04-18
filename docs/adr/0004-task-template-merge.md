# ADR-0004: Task Storage — Lazy Insert with In-Memory Template Merge

**Status:** Accepted  
**Date:** 2026-04-04

---

## Context

Weekly tasks (raid lockouts, heroic dungeons) follow a fixed template — the same list
of raids applies to every character every week. Two approaches exist for representing
"this character has not yet done Karazhan this week":

| Approach | Description |
|----------|-------------|
| **Pre-seed** | On each weekly reset, insert a row for every template task for every character, all with `checked_at = NULL`. |
| **Lazy insert** | Store only checked (or explicitly unchecked) tasks. Derive the unchecked state at query time by comparing DB rows against the in-memory template. |

### Problems with pre-seeding

- Requires a scheduled job (cron) to run at Tuesday/Wednesday midnight UTC per region.
  This adds infrastructure complexity (a background service or external trigger) and a
  failure mode: if the job misses a reset, the dashboard shows a blank checklist.
- Inserts N rows per character per week regardless of whether the user opens the app
  that week. For a personal project with infrequent use, most of those rows are noise.
- Adds a migration concern: if the task template changes (new raid opens), the seeder
  must handle both "insert new rows" and "remove obsolete rows" correctly.

### The lazy-insert approach

The template (`TaskTemplates.WeeklyRaids`, `TaskTemplates.HeroicDungeons`) is a static
list in code. At dashboard load:

1. Query the DB for rows matching `(character_id, week_start = current_week_start)`.
2. Left-join in memory: for each template entry, find the matching DB row (if any).
3. Template entries with no DB row are emitted as `isChecked = false`.
4. The first time a user checks a task, a row is inserted. Subsequent checks update it.

This is the "materialise on write" pattern.

## Decision

Use **lazy insert with in-memory template merge** for both `WeeklyTasks` and `DailyTasks`.

The merge happens in `TasksController.GetDashboard` via LINQ:

```csharp
var raids = TaskTemplates.WeeklyRaids.Select(template =>
{
    var dbRow = weeklyChecked.FirstOrDefault(t => t.TaskKey == template.Key);
    return new TaskDto(template.Key, template.Name, template.Type,
        dbRow?.CheckedAt != null, dbRow?.CheckedAt);
});
```

`ProfessionCooldowns` follows the same pattern — a DB row only exists after the first
"use" click.

## Consequences

**Positive:**
- No scheduled job or background service needed — zero infrastructure overhead
- New weeks start cleanly: no migration, no seeder, the first `GET /dashboard` call
  simply finds no DB rows and returns all tasks as unchecked
- Adding a new raid to the template (e.g. Sunwell opens) requires only a code change
  to `TaskTemplates.cs` — no data migration needed
- Database stays small: only checked/interacted tasks create rows

**Negative:**
- `GetDashboard` loads all of the current period's rows into memory before merging. For
  a single user with ~10 characters this is negligible (at most ~200 rows total). It
  would not scale to thousands of users without pagination or a server-side JOIN.
- The merge logic must be duplicated for each task type (weekly raids, heroic dungeons).
  Currently duplicated in two `Select` calls in `GetDashboard`; acceptable at this size.
- If a task key is renamed in the template, old DB rows become orphaned (they won't match
  any template key and will be ignored). This is a silent data loss. Key renames should
  be accompanied by a DB migration to update existing rows.

**Neutral:**
- The unique index on `(CharacterId, TaskKey, WeekStart)` in `AppDbContext` prevents
  duplicate rows if concurrent requests try to insert the same task simultaneously.
