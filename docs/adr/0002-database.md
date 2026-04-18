# ADR-0002: Database — PostgreSQL + EF Core on Neon

**Status:** Accepted  
**Date:** 2026-04-04

---

## Context

The original plan called for Turso (SQLite via libsql over HTTP) with Drizzle ORM. After
switching to ASP.NET Core (ADR-0001), Turso was no longer appropriate — the libsql HTTP
client has no first-class .NET support and Drizzle is JavaScript-only.

Options considered:

| Option | Notes |
|--------|-------|
| SQLite (file-based) | Simple, zero-cost, no network. Poor fit for a cloud deployment; no good EF Core provider for Railway's ephemeral filesystem |
| SQL Server / Azure SQL | First-class .NET support. Free tier is limited; heavy for a personal project |
| PostgreSQL (Neon) | Full EF Core support via Npgsql. Neon free tier is generous (0.5 GB storage, always-on). Industry-standard open source DB |
| MySQL / PlanetScale | EF Core support exists but Npgsql/PostgreSQL ecosystem is better integrated with .NET |

## Decision

Use **PostgreSQL** via **EF Core 10 + Npgsql 10**, hosted on **Neon** (serverless PostgreSQL
free tier).

- Schema managed via EF Core migrations (checked into git under `Migrations/`)
- Auto-migrate on startup (`db.Database.MigrateAsync()` in `Program.cs`) for simplicity
  at this project scale — acceptable since migrations are reviewed before merge
- Local dev: same Neon database (dev branch) or a local PostgreSQL instance

## Consequences

**Positive:**
- Npgsql is the gold-standard PostgreSQL provider for .NET; excellent EF Core integration
- Neon's free tier (0.5 GB, scale-to-zero) is sufficient for a personal project with one user
- Schema migrations are version-controlled and auditable
- PostgreSQL's `DateOnly` native type maps cleanly to EF Core's `DateOnly` support (added
  in EF Core 6), which is used throughout the task reset logic

**Negative:**
- Neon scale-to-zero means the first request after inactivity has ~500ms cold start latency
- Connection string contains credentials — must not be committed (gitignored via
  `appsettings.Development.json`; see `appsettings.Development.template.json`)
- Migrations must be reviewed carefully; auto-migrate on startup means a bad migration
  runs immediately on deploy

**Neutral:**
- `DateOnly` (used for `WeekStart`, `DayStart`) requires Npgsql 6+ and EF Core 6+.
  Both are satisfied by the current package versions.
- The 6 tables in the schema (Users, Characters, WeeklyTasks, DailyTasks,
  ProfessionCooldowns, GearSlots) fit comfortably within Neon's free tier.
