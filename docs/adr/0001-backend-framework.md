# ADR-0001: Backend Framework — ASP.NET Core (.NET 10)

**Status:** Accepted  
**Date:** 2026-04-04

---

## Context

The original design doc proposed a fully JavaScript stack: Next.js with Turso (SQLite via
libsql) + Drizzle ORM, deployed entirely to Vercel. After the design was approved, the user
specified a hard constraint: the backend must be in .NET Core.

Two realistic options existed at that point:

| Option | Stack | Notes |
|--------|-------|-------|
| A | Next.js API routes (Node.js) | Stays JS, simpler deployment |
| B | ASP.NET Core Web API (.NET 10) | .NET preference, separate deploy target |

Option A would have satisfied the "backend" requirement only superficially — Next.js API
routes are co-located with the frontend and deploy on Vercel's edge/serverless runtime.
Option B is a proper standalone API server.

## Decision

Use **ASP.NET Core Web API (.NET 10)** as the backend, deployed separately from the Next.js
frontend.

- .NET 10 (current LTS channel as of November 2025)
- EF Core 10 with Npgsql provider for PostgreSQL
- Cookie-based session auth via ASP.NET Core's built-in OAuth middleware
- Deployed to Railway (see ADR-0006)

## Consequences

**Positive:**
- Satisfies user's .NET preference — familiar ecosystem, strong typing end-to-end
- ASP.NET Core's built-in OAuth middleware handles Blizzard OAuth cleanly without
  additional libraries (no NextAuth.js dependency)
- EF Core migrations give a controlled, auditable schema evolution path
- .NET 10 ships with no separate `Authentication.Cookies` package needed — in-box

**Negative:**
- Two separate deployment targets (frontend on Vercel, backend on Railway) instead of one
- Cross-origin cookie setup required (`SameSite=None; Secure`) — adds local dev complexity
  (HTTPS dev cert required; see SETUP.md)
- Cannot use Vercel Edge runtime for any backend logic

**Neutral:**
- Original Turso/Drizzle plan replaced with PostgreSQL/EF Core (see ADR-0002)
- TypeScript frontend talks to C# backend via JSON over HTTP — no type sharing without
  a code-gen step (acceptable for this project's scale)
