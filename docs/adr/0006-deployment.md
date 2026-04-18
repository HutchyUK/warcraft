# ADR-0006: Deployment — Vercel (Frontend) + Railway (Backend)

**Status:** Accepted  
**Date:** 2026-04-04

---

## Context

The app has two separately deployable components: the Next.js frontend and the ASP.NET
Core API. Options considered for hosting:

| Option | Frontend | Backend | Cost | Notes |
|--------|----------|---------|------|-------|
| **Vercel + Railway** | Vercel free | Railway hobby ($5/mo) | ~$5/mo | Separate platforms optimised for each runtime |
| **Vercel + Fly.io** | Vercel free | Fly.io free tier | $0–$3/mo | Fly has Docker-native deploys; free tier has cold starts |
| **Single VPS (DigitalOcean/Hetzner)** | nginx | ASP.NET Core | $4–$6/mo | Full control; requires manual setup, TLS, process management |
| **Azure App Service** | Static Web App | App Service | Free tiers available | First-class .NET support; free tier cold starts; more config overhead |
| **Render** | Static site | Web service | Free tier | Free tier has cold starts (~30s); limited .NET support |

### Why not a single host?

Vercel is the dominant platform for Next.js and provides zero-config GitHub integration,
preview deployments per PR, and global CDN. Its free tier is appropriate for a personal
project. Giving this up to use a single server would add friction.

### Railway vs the alternatives

- **Fly.io**: Docker-native, good .NET support, free tier available. However, the free
  tier requires a credit card and has usage limits that can cause unexpected charges.
  Railway's hobby plan ($5/mo) is predictable.
- **Render**: Free tier has ~30s cold starts for web services — unacceptable for an app
  designed around 30-second glances.
- **Azure**: First-class .NET hosting, but more configuration overhead than warranted for
  a personal side project with one user.
- **Railway**: Simple GitHub integration, automatic deploys from a subdirectory, built-in
  environment variable management, predictable pricing.

## Decision

Deploy the **frontend to Vercel** and the **backend to Railway**.

### Vercel (frontend)

- Connect `frontend/` subdirectory
- Set `NEXT_PUBLIC_API_URL` environment variable to the Railway backend URL
- Preview deployments automatically for each PR

### Railway (backend)

- Connect `backend/` subdirectory; Railway auto-detects .NET and uses `dotnet publish`
- Set environment variables: `ConnectionStrings__DefaultConnection`, `Blizzard__ClientId`,
  `Blizzard__ClientSecret`, `Blizzard__RedirectUri`, `FrontendUrl`,
  `Cors__AllowedOrigins__0`, `ASPNETCORE_URLS=http://+:8080`
- Railway provides a `*.railway.app` HTTPS subdomain — satisfies the `Secure` cookie
  requirement in production

### Cross-origin cookie flow in production

```
Browser → https://warcraft.vercel.app (Next.js, GET page)
Browser → https://warcraft-api.railway.app/api/auth/login (full navigation, sets cookie)
Blizzard OAuth → https://warcraft-api.railway.app/api/auth/callback (sets cookie, redirects to FrontendUrl)
Browser → https://warcraft.vercel.app (lands back on frontend, cookie is set for railway.app domain)
Browser → https://warcraft-api.railway.app/api/tasks/dashboard/1 (fetch with credentials: 'include', cookie sent)
```

The cookie domain is `railway.app` (the API origin). The frontend sends it on every
request via `credentials: 'include'`. CORS on the backend allows the Vercel origin.

## Consequences

**Positive:**
- Zero-config deployments: push to `main` → both Vercel and Railway deploy automatically
- Preview deployments on Vercel let you test PRs before merging
- HTTPS everywhere in production without any cert management
- Railway's `ASPNETCORE_URLS=http://+:8080` means the app listens on Railway's expected
  port without any Dockerfile changes

**Negative:**
- ~$5/month Railway cost (hobby plan). Free plan is available but has cold starts.
- Two platforms to manage, two sets of environment variables to maintain
- The Blizzard redirect URI must be updated in the developer portal when the Railway URL
  changes (Railway generates a fixed subdomain per project — it only changes if the
  project is deleted and recreated)
- Cross-origin cookies add local dev friction (HTTPS dev cert; see ADR-0003 and SETUP.md)

**Neutral:**
- Containerisation (WAR-25) will make it straightforward to switch Railway's deploy
  method from buildpack to Docker image if needed.
