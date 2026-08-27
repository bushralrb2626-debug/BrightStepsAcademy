# BrightSteps Academy

Frontend-only ASP.NET Core MVC (.NET 10) prototype of a colorful school management platform.

**BrightSteps Academy · Learn. Explore. Grow.**

This phase is **100% frontend**: mock data only — no SQL Server, EF, Identity, APIs, or real persistence.

## Run

```bash
cd BrightStepsAcademy
dotnet run --launch-profile http
```

Open **http://localhost:5182/**

## What’s included

### Public website
Home journey with hero, about, programs, why choose us, facilities, teachers, activities, events, achievements, gallery (lightbox), notices, portal CTA, contact form (toast), and footer.

### Portal
Six role cards → mock login (any email/password) → role dashboard.

### Role dashboards
| Role | Highlights |
|------|------------|
| Super Admin | Platform stats, charts, user management tabs, schools |
| Admin | Students / teachers / parents / classes, notices, attendance |
| Headmaster | Command center, approvals, assign teacher, performance |
| Teacher | Classes, attendance marking, assignments, timetable |
| Parent | Children cards, homework, results, messages |
| Student | Playful home, stars/badges, homework, achievements |

Shared modules: Messages, Reports, Settings, Profile, Assignments, Attendance, Results, Timetable, Notices, Events.

## Demo tip

1. Open http://localhost:5182/
2. Click **Login** (or a public portal card: Teacher / Parent / Student / Headmaster)
3. Use a demo email — **role is resolved from the account**, not a dropdown

| Email | Opens |
|-------|--------|
| `sarah.wilson@brightsteps.academy` | Teacher |
| `amelia.johnson@email.com` | Parent |
| `alex.rivera@student.brightsteps.academy` | Student |
| `grace.okonkwo@brightsteps.academy` | Headmaster |
| `daniel.reeves@brightsteps.academy` | Admin *(staff — not shown as a public portal)* |
| `nora.patel@brightsteps.academy` | Super Admin *(staff — not shown as a public portal)* |

Password for demos: `demo1234`

Public UI never advertises Admin / Super Admin portals — only a general **Login**.

## Architecture (ready for a real backend later)

```
Data/ISchoolData.cs      → contract used by controllers
Data/MockSchoolData.cs   → in-memory demo data (swap later)
Data/Images.cs           → centralized image URLs
Data/NavCatalog.cs       → role-specific sidebar navigation
```

Register a future `EfSchoolData` in `Program.cs` instead of `MockSchoolData` — controllers and views stay the same.

## Design

- Fonts: Fredoka + Nunito  
- CSS: `wwwroot/css/site.css` + `wwwroot/css/bridge.css`  
- JS: `wwwroot/js/site.js` (toasts, modals, gallery, counters, sidebar, charts)  
- Soft cream backgrounds with sunshine / sky / coral / mint accents — **no neon**

## Stack

ASP.NET Core MVC · Razor · Chart.js (CDN) · Bootstrap (light utilities only) · custom BSA UI
