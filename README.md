# The War Room

WoW Classic Anniversary Alt Manager — a personal dashboard for tracking raid lockouts,
profession cooldowns, and gear progression across multiple alts.

## Quick start

**With Docker** (recommended — no SDK required):
```bash
cp .env.docker.example .env.docker   # fill in Blizzard credentials
docker-compose --env-file .env.docker up --build
# → http://localhost:3000
```

**Without Docker** (requires .NET 10 SDK + Node 20):
```bash
# Backend (https://localhost:7211)
cd backend/Warcraft.Api && dotnet run --launch-profile https

# Frontend (http://localhost:3000)
cd frontend && npm run dev
```

See [SETUP.md](SETUP.md) for the full setup guide including Blizzard app registration.

## Project structure

- `backend/Warcraft.Api/` — ASP.NET Core Web API (.NET 10)
- `frontend/` — Next.js App Router + Tailwind CSS
- `DESIGN.md` — Full design doc (problem statement, data model, phases)
- `SETUP.md` — Local development setup guide
- `docs/adr/` — Architecture Decision Records

## Stack

| Layer | Tech |
|-------|------|
| Frontend | Next.js 14 App Router, Tailwind CSS |
| Backend | ASP.NET Core Web API, .NET 10 |
| Database | PostgreSQL via EF Core 10 (Neon on free tier) |
| Auth | Blizzard OAuth via ASP.NET Core OAuth middleware |
| Deploy | Vercel (frontend) + Railway (backend) |

## Tickets

Issues tracked in Linear — WAR project.
