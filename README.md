# BrightSteps Academy

Multi-tenant School Management System SaaS — ASP.NET Core MVC (.NET 10).

**BrightSteps Academy · Learn. Explore. Grow.**

## Quick start

```bash
dotnet restore
dotnet ef database update
dotnet run --launch-profile http
```

Open **http://localhost:5182/**

### Database

- **Local development:** SQL Server LocalDB (`BrightStepsAcademy` database)
- **Render / Docker:** SQLite (`USE_SQLITE=1`, file at `/tmp/brightsteps.db`)

Connection string is in `appsettings.json`. Migrations run automatically on startup.

## Demo logins

Password for all demo accounts: **`Demo@12345`**

| Login | Role |
|-------|------|
| `superadmin@platform.com` | Super Admin |
| `admin@brightfuture.academy` | School Admin |
| `grace.okonkwo@brightsteps.academy` | Headmaster |
| `teacher_demo` | Teacher |
| `parent_demo` | Parent / Guardian |
| `student_demo` | Student |

Login URL: **http://localhost:5182/Login**

## What's included

### Platform
- Multi-tenant isolation (school-scoped data)
- ASP.NET Identity with role-based access
- Super Admin portal (schools, subscriptions, platform settings)
- School Admin portal (staff, students, infrastructure, CMS, permissions)
- Audit logging

### Academic portals
- **Teacher:** classes, attendance, grade book, report cards, assignments
- **Parent:** children, diary, attendance, marks, announcements, materials
- **Student:** dashboard, timetable, assignments, marks, diary, notifications

### Operations
- Buildings, floors, rooms, furniture
- Fee management
- Timetable
- Website content management (public homepage driven from DB)

### Account notifications
- Email templates for account lifecycle events
- SMTP or file outbox (`EmailOutbox/` when `Email:Enabled` is false)
- Admin resend credentials on student/staff edit pages

## Solution structure

```
BrightStepsAcademy.sln
├── Controllers/          # Public site + role portals + Manage/*
├── Data/                 # EF Core DbContext, migrations, seeding
├── Domain/               # Entities
├── Services/             # Business logic, email, tenant context
├── Views/                # Razor views
├── EmailTemplates/       # HTML email templates
├── wwwroot/              # CSS, JS, static assets
├── Dockerfile            # Render deployment
└── render.yaml           # Render service config
```

## Deploy (Render)

1. Push to GitHub: `https://github.com/bushralrb2626-debug/BrightStepsAcademy`
2. Connect repo on Render as a Docker web service
3. Set env vars from `render.yaml` (`USE_SQLITE`, connection string, etc.)

## Stack

ASP.NET Core MVC · EF Core · Identity · SQL Server / SQLite · Razor · Chart.js · Bootstrap
