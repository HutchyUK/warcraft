# Architecture Decision Records

This directory captures the key architectural decisions made during the development of
The War Room. Each record follows the standard ADR format:
**Status → Context → Decision → Consequences**.

| ADR | Title | Status |
|-----|-------|--------|
| [0001](0001-backend-framework.md) | Backend Framework — ASP.NET Core (.NET 10) | Accepted |
| [0002](0002-database.md) | Database — PostgreSQL + EF Core on Neon | Accepted |
| [0003](0003-auth.md) | Authentication — Blizzard OAuth + Cookie Session | Accepted |
| [0004](0004-task-template-merge.md) | Task Storage — Lazy Insert with In-Memory Template Merge | Accepted |
| [0005](0005-daily-vs-weekly-tasks.md) | Heroic Dungeons — Separate DailyTasks Table | Accepted |
| [0006](0006-deployment.md) | Deployment — Vercel (Frontend) + Railway (Backend) | Accepted |

## Adding a new ADR

1. Copy an existing ADR as a template
2. Number it sequentially (`0007-your-topic.md`)
3. Add a row to the table above
4. Set status to `Proposed` until the decision is implemented, then `Accepted`
