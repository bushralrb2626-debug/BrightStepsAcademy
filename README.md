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

## Stack

ASP.NET Core · EF Core · SQLite · Identity · Razor
