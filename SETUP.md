# Local Development Setup

Everything you need to run The War Room on your machine.

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10)
- [Node.js 20+](https://nodejs.org/) and npm
- [Git](https://git-scm.com/)
- A Blizzard account (for OAuth login + character import)

---

## 1. Clone and install

```bash
git clone https://github.com/HutchyUK/warcraft.git
cd warcraft

# Install frontend dependencies
cd frontend && npm install && cd ..
```

---

## 2. Trust the dev HTTPS certificate

The backend uses HTTPS in development (required for the `SameSite=None` auth cookie).
Trust the .NET developer certificate once:

```bash
dotnet dev-certs https --trust
```

On Windows this opens a prompt — click Yes. You only need to do this once per machine.

---

## 3. Register a Blizzard Developer App

1. Go to [developer.battle.net](https://develop.battle.net/access/clients)
2. Click **Create Client**
3. Fill in:
   - **Client Name:** War Room (or anything you like)
   - **Redirect URIs:** `https://localhost:7211/api/auth/callback`
   - **Service URL:** `http://localhost:3000`
   - **Intended Use:** Personal / non-commercial project
4. After creating, copy your **Client ID** and **Client Secret**

> For production (Railway), you will need to add a second redirect URI:
> `https://your-api.railway.app/api/auth/callback`

---

## 4. Provision a PostgreSQL database (Neon)

1. Create a free account at [neon.tech](https://neon.tech)
2. Create a new project — name it `warcraft`
3. Copy the **connection string** — it looks like:
   `postgresql://user:password@host.neon.tech/warcraft?sslmode=require`

---

## 5. Configure the backend

```bash
cd backend/Warcraft.Api

# Copy the template and fill in your values
cp appsettings.Development.template.json appsettings.Development.json
```

Open `appsettings.Development.json` and replace the placeholders:

```json
{
  "FrontendUrl": "http://localhost:3000",
  "ConnectionStrings": {
    "DefaultConnection": "postgresql://user:password@host.neon.tech/warcraft?sslmode=require"
  },
  "Blizzard": {
    "ClientId": "your_client_id_here",
    "ClientSecret": "your_client_secret_here",
    "RedirectUri": "https://localhost:7211/api/auth/callback"
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000"]
  }
}
```

---

## 6. Configure the frontend

```bash
cd frontend

# Copy the example and set the API URL
cp .env.local.example .env.local
```

The default value (`https://localhost:7211`) matches the HTTPS backend profile — no changes needed for local dev.

---

## 7. Run EF Core migrations

The app auto-migrates on startup, but you can also run migrations manually:

```bash
cd backend/Warcraft.Api
dotnet ef database update
```

---

## 8. Start the app

Open two terminals:

**Terminal 1 — Backend** (from `backend/Warcraft.Api/`):
```bash
dotnet run --launch-profile https
```
The API runs at `https://localhost:7211`.

**Terminal 2 — Frontend** (from `frontend/`):
```bash
npm run dev
```
The app runs at `http://localhost:3000`.

---

## 9. Verify it works

1. Open [http://localhost:3000](http://localhost:3000)
2. Click **Login with Battle.net**
3. Authenticate with your Blizzard account
4. You should land back at `http://localhost:3000` — logged in
5. Click **Import Characters** — your Classic Anniversary characters should appear
6. If the Blizzard API import fails (says "Classic API unavailable"), characters can be added manually

---

## Running with Docker

If you have Docker installed you can run the full stack with a single command instead
of the steps above. No .NET SDK or Node.js required on your machine.

```bash
# 1. Copy the example env file and fill in your credentials
cp .env.docker.example .env.docker
# Edit .env.docker with your Blizzard credentials and a Postgres password

# 2. Build and start all three services (db + api + frontend)
docker-compose --env-file .env.docker up --build

# App will be available at http://localhost:3000
```

> **Note:** The Blizzard redirect URI registered in the developer portal must be
> `http://localhost:8080/api/auth/callback` when using docker-compose locally.

To stop:
```bash
docker-compose down
# Add -v to also delete the postgres_data volume (removes all data)
```

---

## Environment variable reference

### Backend (`appsettings.Development.json` — gitignored)

| Key | Description |
|-----|-------------|
| `FrontendUrl` | URL of the Next.js frontend (used for post-OAuth redirect) |
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection string |
| `Blizzard:ClientId` | Blizzard OAuth app client ID |
| `Blizzard:ClientSecret` | Blizzard OAuth app client secret |
| `Blizzard:RedirectUri` | Registered callback URI (must match Blizzard developer portal exactly) |
| `Cors:AllowedOrigins` | Allowed CORS origins (must include frontend URL) |

### Frontend (`.env.local` — gitignored)

| Key | Description |
|-----|-------------|
| `NEXT_PUBLIC_API_URL` | Base URL of the backend API |

---

## Troubleshooting

**"Your connection is not private" on localhost:7211**
Run `dotnet dev-certs https --trust` and restart your browser.

**Import says "Classic API unavailable"**
The Blizzard Classic character namespace (`profile-classic1x-us`) sometimes returns empty results.
Use the manual character add form — it works identically for all app features.

**Login redirects to a blank .NET page**
Check that `FrontendUrl` is set correctly in `appsettings.Development.json`. It must match the
port your Next.js app is running on (default: `http://localhost:3000`).

**CORS errors in the browser console**
Check that `Cors:AllowedOrigins` includes `http://localhost:3000` in your dev config. Also
confirm you're running the backend on the HTTPS profile (port 7211), not HTTP (5154).
