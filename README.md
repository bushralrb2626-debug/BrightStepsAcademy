# BrightSteps Academy

Multi-tenant School Management System — ASP.NET Core MVC (.NET 10).

## Run locally (normal setup)

```bash
cd BrightStepsAcademy
dotnet run --launch-profile http
```

Open **http://localhost:5182/**

- Database: **SQLite** file `brightsteps.db` (auto-created in project folder on first run)
- No SQL Server / LocalDB / Render / `.com` needed
- Demo data seeds automatically on startup

### Login

**http://localhost:5182/Login** — password for all demos: **`Demo@12345`**

| Login | Role |
|-------|------|
| `student_demo` | Student |
| `parent_demo` | Parent |
| `teacher_demo` | Teacher |
| `superadmin@platform.com` | Super Admin |
| `admin@brightfuture.academy` | School Admin |

## Reset database

Stop the app, delete `brightsteps.db`, run again — fresh seed.

## Live on Render (with database)

**URL:** https://brightsteps-academy.onrender.com  
**Login:** https://brightsteps-academy.onrender.com/Login

Uses **SQLite** at `/app/data/brightsteps.db` — created and seeded automatically when the container starts.

1. Open [Render Blueprint](https://dashboard.render.com/blueprint/new)
2. Connect repo `bushralrb2626-debug/BrightStepsAcademy` → branch `main`
3. Click **Deploy Blueprint**

Pushes to `main` auto-deploy. First load may take ~1 minute (free tier cold start).

## Stack

ASP.NET Core · EF Core · SQLite · Identity · Razor
