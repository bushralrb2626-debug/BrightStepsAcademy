using System.Reflection;
using BrightStepsAcademy.Domain;
using BrightStepsAcademy.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;
        var db = sp.GetRequiredService<AppDbContext>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();

        await EnsureRolesAsync(roleManager);
        await EnsurePermissionsAsync(db);

        var school = await EnsureDemoSchoolAsync(db);
        await EnsureDemoSubscriptionAndSettingsAsync(db, school);
        await EnsureUsersAsync(userManager, db, school.Id);
        await EnsureBuildingsAsync(db, school.Id);
        await EnsureStaffCategoriesAsync(db, school.Id);
        await SchoolBootstrap.EnsureAcademicStructureAsync(db, school.Id);
        await EnsureWebsiteContentAsync(db, school);
        await ApplyEnglishWebsiteDefaultsAsync(db, school.Id);
        await SchoolBootstrap.EnsureAllSchoolsBootstrappedAsync(db);
    }

    private static async Task EnsureDemoSubscriptionAndSettingsAsync(AppDbContext db, School school)
    {
        if (!await db.PlatformSettings.AnyAsync())
        {
            db.PlatformSettings.Add(new PlatformSettings
            {
                PlatformName = "BrightSteps Platform",
                SupportEmail = "support@brightsteps.academy",
                SupportPhone = "+1 (555) 010-2000",
                Website = "https://brightsteps.academy",
                DefaultSubscriptionMonths = 12,
                ExpiryWarningDays = 30,
                AvailablePlansJson = """["Basic","Standard","Premium","Enterprise"]""",
                UpdatedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var warningDays = (await db.PlatformSettings.AsNoTracking()
            .Select(s => (int?)s.ExpiryWarningDays).FirstOrDefaultAsync()) ?? 30;

        var sub = await db.SchoolSubscriptions.FirstOrDefaultAsync(s => s.SchoolId == school.Id);
        if (sub is null)
        {
            var now = DateTimeOffset.UtcNow;
            sub = new SchoolSubscription
            {
                SchoolId = school.Id,
                PlanCode = "Premium",
                PlanName = "Premium",
                StartDate = now,
                ExpiryDate = now.AddYears(1),
                BillingCycle = BillingCycle.Yearly,
                Price = 0,
                Notes = "Demo school seed subscription",
                CreatedAt = now
            };
            BrightStepsAcademy.Services.SubscriptionStatusHelper.Refresh(sub, warningDays);
            db.SchoolSubscriptions.Add(sub);
            db.SubscriptionChangeLogs.Add(new SubscriptionChangeLog
            {
                SchoolSubscriptionId = sub.Id,
                SchoolId = school.Id,
                ChangedByUserName = "DbSeeder",
                Summary = "Seed subscription created",
                Details = "Premium yearly for demo school",
                Timestamp = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
        }
        else
        {
            BrightStepsAcademy.Services.SubscriptionStatusHelper.Refresh(sub, warningDays);
            await db.SaveChangesAsync();
        }
    }

    private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles =
        [
            AppRoleNames.SuperAdmin,
            AppRoleNames.SchoolAdmin,
            AppRoleNames.CustomAdmin,
            AppRoleNames.Staff,
            AppRoleNames.Student,
            AppRoleNames.Guardian,
            AppRoleNames.Teacher
        ];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private static async Task EnsurePermissionsAsync(AppDbContext db)
    {
        var existing = await db.AppPermissions.ToListAsync();
        var byCode = existing.ToDictionary(p => p.Code, StringComparer.OrdinalIgnoreCase);

        foreach (var entry in PermissionCatalog.All)
        {
            if (byCode.TryGetValue(entry.Code, out var perm))
            {
                perm.Name = entry.Name;
                perm.Module = entry.Module;
                perm.Description = entry.Description;
            }
            else
            {
                db.AppPermissions.Add(new AppPermission
                {
                    Code = entry.Code,
                    Name = entry.Name,
                    Module = entry.Module,
                    Description = entry.Description
                });
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task<School> EnsureDemoSchoolAsync(AppDbContext db)
    {
        var school = await db.Schools.FirstOrDefaultAsync(s => s.SchoolCode == "BFA-001");
        if (school is null && await db.Schools.AnyAsync())
            school = await db.Schools.OrderBy(s => s.CreatedAt).FirstAsync();

        if (school is not null)
        {
            school.Name = "Scuola Materna";
            school.ShortName = "Scuola Materna";
            school.Tagline = "Learn. Explore. Grow.";
            school.SchoolType = "Primary & Middle";
            school.Description =
                "Scuola Materna began with a simple idea: childhood should be colourful, safe, and full of discovery. Today we welcome more than a thousand learners across bright classrooms, creative studios, and lively fields.";
            school.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            return school;
        }

        school = new School
        {
            SchoolCode = "BFA-001",
            Name = "Scuola Materna",
            ShortName = "Scuola Materna",
            Tagline = "Learn. Explore. Grow.",
            RegistrationNumber = "REG-BFA-2011",
            Email = "hello@brightfuture.academy",
            Phone = "+1 (555) 214-8800",
            Website = "https://brightfuture.academy",
            Address = "42 Maple Grove, Riverside",
            City = "Riverside",
            Country = "USA",
            PrincipalName = "Grace Okonkwo",
            EstablishedYear = 2011,
            SchoolType = "Primary & Middle",
            Description =
                "Scuola Materna began with a simple idea: childhood should be colourful, safe, and full of discovery. Today we welcome more than a thousand learners across bright classrooms, creative studios, and lively fields.",
            EmergencyContact = "+1 (555) 214-8899",
            LogoPath = Images.School,
            FaviconPath = Images.School,
            Status = SchoolStatus.Active
        };

        db.Schools.Add(school);
        await db.SaveChangesAsync();
        return school;
    }

    private static async Task EnsureUsersAsync(
        UserManager<ApplicationUser> userManager,
        AppDbContext db,
        Guid schoolId)
    {
        const string password = "Demo@12345";

        var superAdmin = await userManager.FindByEmailAsync("superadmin@platform.com");
        if (superAdmin is null)
        {
            superAdmin = new ApplicationUser
            {
                UserName = "superadmin@platform.com",
                Email = "superadmin@platform.com",
                EmailConfirmed = true,
                FullName = "Platform Owner",
                SchoolId = null,
                IsActive = true,
                LoginId = "PLATFORM-SA"
            };
            var result = await userManager.CreateAsync(superAdmin, password);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    "Failed to create Super Admin: " + string.Join("; ", result.Errors.Select(e => e.Description)));

            await userManager.AddToRoleAsync(superAdmin, AppRoleNames.SuperAdmin);
        }

        var schoolAdmin = await userManager.FindByEmailAsync("admin@brightfuture.academy");
        if (schoolAdmin is null)
        {
            schoolAdmin = new ApplicationUser
            {
                UserName = "admin@brightfuture.academy",
                Email = "admin@brightfuture.academy",
                EmailConfirmed = true,
                FullName = "School Administrator",
                SchoolId = schoolId,
                IsActive = true,
                LoginId = "BFA-ADMIN"
            };
            var result = await userManager.CreateAsync(schoolAdmin, password);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    "Failed to create School Admin: " + string.Join("; ", result.Errors.Select(e => e.Description)));

            await userManager.AddToRoleAsync(schoolAdmin, AppRoleNames.SchoolAdmin);
        }

        var hasProfile = await db.SchoolAdminProfiles
            .AnyAsync(p => p.UserId == schoolAdmin.Id && p.SchoolId == schoolId);
        if (!hasProfile)
        {
            db.SchoolAdminProfiles.Add(new SchoolAdminProfile
            {
                UserId = schoolAdmin.Id,
                SchoolId = schoolId,
                AdminType = "School Admin",
                IsPrimary = true,
                IsActive = true
            });
            await db.SaveChangesAsync();
        }
    }

    private static async Task EnsureBuildingsAsync(AppDbContext db, Guid schoolId)
    {
        if (await db.Buildings.AnyAsync(b => b.SchoolId == schoolId))
            return;

        var building = new Building
        {
            SchoolId = schoolId,
            Name = "Main Building",
            BuildingNumber = "MB-1",
            Description = "Primary academic building"
        };
        db.Buildings.Add(building);
        await db.SaveChangesAsync();

        var ground = new Floor
        {
            SchoolId = schoolId,
            BuildingId = building.Id,
            FloorNumber = 0,
            FloorName = "Ground Floor"
        };
        var first = new Floor
        {
            SchoolId = schoolId,
            BuildingId = building.Id,
            FloorNumber = 1,
            FloorName = "First Floor"
        };
        db.Floors.AddRange(ground, first);
        await db.SaveChangesAsync();

        var rooms = new[]
        {
            new Room
            {
                SchoolId = schoolId,
                BuildingId = building.Id,
                FloorId = ground.Id,
                RoomNumber = "001",
                RoomName = "Classroom 001",
                RoomType = nameof(RoomTypeKind.Classroom),
                Capacity = 30
            },
            new Room
            {
                SchoolId = schoolId,
                BuildingId = building.Id,
                FloorId = ground.Id,
                RoomNumber = "002",
                RoomName = "Classroom 002",
                RoomType = nameof(RoomTypeKind.Classroom),
                Capacity = 30
            },
            new Room
            {
                SchoolId = schoolId,
                BuildingId = building.Id,
                FloorId = first.Id,
                RoomNumber = "101",
                RoomName = "Classroom 101",
                RoomType = nameof(RoomTypeKind.Classroom),
                Capacity = 32
            },
            new Room
            {
                SchoolId = schoolId,
                BuildingId = building.Id,
                FloorId = first.Id,
                RoomNumber = "102",
                RoomName = "Classroom 102",
                RoomType = nameof(RoomTypeKind.Classroom),
                Capacity = 32
            }
        };
        db.Rooms.AddRange(rooms);
        await db.SaveChangesAsync();

        var room101 = rooms.First(r => r.RoomNumber == "101");
        db.FurnitureItems.AddRange(
            new FurnitureItem
            {
                SchoolId = schoolId,
                RoomId = room101.Id,
                Category = "Desks",
                Name = "Student Desk",
                Quantity = 25,
                Condition = FurnitureCondition.Good
            },
            new FurnitureItem
            {
                SchoolId = schoolId,
                RoomId = room101.Id,
                Category = "Chairs",
                Name = "Student Chair",
                Quantity = 25,
                Condition = FurnitureCondition.Good
            },
            new FurnitureItem
            {
                SchoolId = schoolId,
                RoomId = room101.Id,
                Category = "Desks",
                Name = "Teacher Desk",
                Quantity = 1,
                Condition = FurnitureCondition.Good
            },
            new FurnitureItem
            {
                SchoolId = schoolId,
                RoomId = room101.Id,
                Category = "Chairs",
                Name = "Teacher Chair",
                Quantity = 1,
                Condition = FurnitureCondition.Good
            },
            new FurnitureItem
            {
                SchoolId = schoolId,
                RoomId = room101.Id,
                Category = "Boards",
                Name = "Whiteboard",
                Quantity = 1,
                Condition = FurnitureCondition.Good
            });
        await db.SaveChangesAsync();
    }

    private static async Task EnsureStaffCategoriesAsync(AppDbContext db, Guid schoolId)
    {
        if (await db.StaffCategories.AnyAsync(c => c.SchoolId == schoolId))
            return;

        string[] names = ["Teachers", "Helpers", "Accountants", "Security", "Reception"];
        foreach (var name in names)
        {
            db.StaffCategories.Add(new StaffCategory
            {
                SchoolId = schoolId,
                Name = name,
                Description = $"{name} staff category"
            });
        }

        await db.SaveChangesAsync();
    }

    private static async Task EnsureWebsiteContentAsync(AppDbContext db, School school)
    {
        var schoolId = school.Id;

        if (!await db.WebsiteSettings.AnyAsync(w => w.SchoolId == schoolId))
        {
            db.WebsiteSettings.Add(new WebsiteSettings
            {
                SchoolId = schoolId,
                PrimaryColor = "#1565D8",
                SecondaryColor = "#F5C518",
                IsPublished = true
            });
        }

        if (!await db.HeroContents.AnyAsync(h => h.SchoolId == schoolId))
        {
            db.HeroContents.Add(new HeroContent
            {
                SchoolId = schoolId,
                Heading = "WHERE LITTLE MINDS GROW INTO BIG DREAMS",
                Description =
                    "A colorful campus where curiosity blooms, creativity shines, and every child gets to learn, explore and dream — every single day.",
                ImagePath = Images.KidsRead,
                CtaText = "Explore Our School",
                CtaLink = "#about"
            });
        }

        if (!await db.AboutContents.AnyAsync(a => a.SchoolId == schoolId))
        {
            db.AboutContents.Add(new AboutContent
            {
                SchoolId = schoolId,
                Heading = "A story of bright beginnings",
                Description =
                    "Not just a campus — a colorful journey families love to join. Sunlit classrooms, curious questions, and teachers who know every child by name.",
                ImagePath = Images.Classroom
            });
        }

        if (!await db.ContactContents.AnyAsync(c => c.SchoolId == schoolId))
        {
            db.ContactContents.Add(new ContactContent
            {
                SchoolId = schoolId,
                Address = school.Address,
                Phone = school.Phone,
                Email = school.Email,
                OfficeHours = "Mon–Fri 8:00 AM – 4:00 PM"
            });
        }

        if (!await db.HighlightItems.AnyAsync(h => h.SchoolId == schoolId))
        {
            db.HighlightItems.AddRange(
                new HighlightItem
                {
                    SchoolId = schoolId,
                    Title = "Where learning begins",
                    Description =
                        "Sunlit classrooms, curious questions, and teachers who know every child by name — that's the Scuola Materna way.",
                    ImageOrIcon = Images.Classroom,
                    DisplayOrder = 1
                },
                new HighlightItem
                {
                    SchoolId = schoolId,
                    Title = "Our mission",
                    Description =
                        "To nurture curious, kind and confident learners through joyful teaching and meaningful experiences.",
                    ImageOrIcon = Images.KidsRead,
                    DisplayOrder = 2
                },
                new HighlightItem
                {
                    SchoolId = schoolId,
                    Title = "Our vision",
                    Description =
                        "A world where every child feels seen, challenged and inspired to grow — one bright step at a time.",
                    ImageOrIcon = Images.Campus,
                    DisplayOrder = 3
                },
                new HighlightItem
                {
                    SchoolId = schoolId,
                    Title = "Why families choose us",
                    Description =
                        "Safe spaces, creative learning, strong academics, and a portal that keeps parents close to every milestone.",
                    ImageOrIcon = Images.Play,
                    DisplayOrder = 4
                });
        }

        if (!await db.FacilityItems.AnyAsync(f => f.SchoolId == schoolId))
        {
            db.FacilityItems.AddRange(
                new FacilityItem
                {
                    SchoolId = schoolId,
                    Name = "Modern Library",
                    Description = "Books, reading spaces and learning resources.",
                    ImagePath = Images.Library,
                    DisplayOrder = 1
                },
                new FacilityItem
                {
                    SchoolId = schoolId,
                    Name = "Science Laboratory",
                    Description = "Hands-on experiments and discovery.",
                    ImagePath = Images.Science,
                    DisplayOrder = 2
                },
                new FacilityItem
                {
                    SchoolId = schoolId,
                    Name = "Computer Lab",
                    Description = "Technology and digital learning.",
                    ImagePath = Images.Computers,
                    DisplayOrder = 3
                },
                new FacilityItem
                {
                    SchoolId = schoolId,
                    Name = "Sports Ground",
                    Description = "Outdoor sports and physical activities.",
                    ImagePath = Images.Sports,
                    DisplayOrder = 4
                },
                new FacilityItem
                {
                    SchoolId = schoolId,
                    Name = "Art & Creativity Room",
                    Description = "Painting, crafts and creative expression.",
                    ImagePath = Images.Art,
                    DisplayOrder = 5
                },
                new FacilityItem
                {
                    SchoolId = schoolId,
                    Name = "Music Room",
                    Description = "Music, instruments and performance.",
                    ImagePath = Images.Music,
                    DisplayOrder = 6
                });
        }

        if (!await db.GalleryItems.AnyAsync(g => g.SchoolId == schoolId))
        {
            var gallery = new (string Path, string Title, string Category)[]
            {
                (Images.Classroom, "Classroom moments", "Campus"),
                (Images.KidsRead, "Story time", "Learning"),
                (Images.Library, "Library quiet hours", "Campus"),
                (Images.Sports, "Sports Day", "Events"),
                (Images.Art, "Art studio", "Creative"),
                (Images.Science, "Science Fair", "Events"),
                (Images.Campus, "Campus view", "Campus"),
                (Images.Play, "Playground joy", "Campus"),
                (Images.Annual, "Annual celebration", "Events"),
                (Images.Music, "Music class", "Creative"),
                (Images.FieldTrip, "Field trip", "Events"),
                (Images.School, "Welcome gate", "Campus")
            };

            for (var i = 0; i < gallery.Length; i++)
            {
                db.GalleryItems.Add(new GalleryItem
                {
                    SchoolId = schoolId,
                    ImagePath = gallery[i].Path,
                    Title = gallery[i].Title,
                    Category = gallery[i].Category,
                    DisplayOrder = i + 1,
                    IsFeatured = i < 4
                });
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task ApplyEnglishWebsiteDefaultsAsync(AppDbContext db, Guid schoolId)
    {
        var hero = await db.HeroContents.FirstOrDefaultAsync(h => h.SchoolId == schoolId);
        if (hero is not null)
        {
            hero.Heading = "WHERE LITTLE MINDS GROW INTO BIG DREAMS";
            hero.Description =
                "A colorful campus where curiosity blooms, creativity shines, and every child gets to learn, explore and dream — every single day.";
            hero.CtaText = "Explore Our School";
        }

        var about = await db.AboutContents.FirstOrDefaultAsync(a => a.SchoolId == schoolId);
        if (about is not null)
        {
            about.Heading = "A story of bright beginnings";
            about.Description =
                "Not just a campus — a colorful journey families love to join. Sunlit classrooms, curious questions, and teachers who know every child by name.";
        }

        var contact = await db.ContactContents.FirstOrDefaultAsync(c => c.SchoolId == schoolId);
        if (contact is not null)
            contact.OfficeHours = "Mon–Fri 8:00 AM – 4:00 PM";

        var highlightDefaults = new (int Order, string Title, string Description)[]
        {
            (1, "Where learning begins",
                "Sunlit classrooms, curious questions, and teachers who know every child by name — that's the Scuola Materna way."),
            (2, "Our mission",
                "To nurture curious, kind and confident learners through joyful teaching and meaningful experiences."),
            (3, "Our vision",
                "A world where every child feels seen, challenged and inspired to grow — one bright step at a time."),
            (4, "Why families choose us",
                "Safe spaces, creative learning, strong academics, and a portal that keeps parents close to every milestone.")
        };

        var highlights = await db.HighlightItems.Where(h => h.SchoolId == schoolId).ToListAsync();
        foreach (var item in highlights)
        {
            var match = highlightDefaults.FirstOrDefault(h => h.Order == item.DisplayOrder);
            if (match.Title is null) continue;
            item.Title = match.Title;
            item.Description = match.Description;
        }

        var facilityDefaults = new (int Order, string Name, string Description)[]
        {
            (1, "Modern Library", "Books, reading spaces and learning resources."),
            (2, "Science Laboratory", "Hands-on experiments and discovery."),
            (3, "Computer Lab", "Technology and digital learning."),
            (4, "Sports Ground", "Outdoor sports and physical activities."),
            (5, "Art & Creativity Room", "Painting, crafts and creative expression."),
            (6, "Music Room", "Music, instruments and performance.")
        };

        var facilities = await db.FacilityItems.Where(f => f.SchoolId == schoolId).ToListAsync();
        foreach (var item in facilities)
        {
            var match = facilityDefaults.FirstOrDefault(f => f.Order == item.DisplayOrder);
            if (match.Name is null) continue;
            item.Name = match.Name;
            item.Description = match.Description;
        }

        var galleryDefaults = new (int Order, string Title, string Category)[]
        {
            (1, "Classroom moments", "Campus"),
            (2, "Story time", "Learning"),
            (3, "Library quiet hours", "Campus"),
            (4, "Sports Day", "Events"),
            (5, "Art studio", "Creative"),
            (6, "Science Fair", "Events"),
            (7, "Campus view", "Campus"),
            (8, "Playground joy", "Campus"),
            (9, "Annual celebration", "Events"),
            (10, "Music class", "Creative"),
            (11, "Field trip", "Events"),
            (12, "Welcome gate", "Campus")
        };

        var gallery = await db.GalleryItems.Where(g => g.SchoolId == schoolId).ToListAsync();
        foreach (var item in gallery)
        {
            var match = galleryDefaults.FirstOrDefault(g => g.Order == item.DisplayOrder);
            if (match.Title is null) continue;
            item.Title = match.Title;
            item.Category = match.Category;
        }

        var school = await db.Schools.FirstOrDefaultAsync(s => s.Id == schoolId);
        if (school is not null)
        {
            school.Tagline = "Learn. Explore. Grow.";
            school.Description =
                "Scuola Materna began with a simple idea: childhood should be colourful, safe, and full of discovery. Today we welcome more than a thousand learners across bright classrooms, creative studios, and lively fields.";
            school.SchoolType = "Primary & Middle";
        }

        await db.SaveChangesAsync();
    }

}
