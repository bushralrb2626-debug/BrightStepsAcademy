using System.Reflection;
using BrightStepsAcademy.Domain;
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
        await EnsureWebsiteContentAsync(db, school);
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
            AppRoleNames.Student
        ];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private static async Task EnsurePermissionsAsync(AppDbContext db)
    {
        var codes = typeof(PermissionCodes)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!)
            .ToList();

        var existing = await db.AppPermissions.Select(p => p.Code).ToListAsync();
        foreach (var code in codes)
        {
            if (existing.Contains(code))
                continue;

            var parts = code.Split('.', 2);
            var module = parts[0];
            var action = parts.Length > 1 ? parts[1] : code;
            db.AppPermissions.Add(new AppPermission
            {
                Code = code,
                Name = $"{ToTitle(module)} {ToTitle(action)}",
                Module = ToTitle(module),
                Description = $"Allows {action} on {module}."
            });
        }

        await db.SaveChangesAsync();
    }

    private static string ToTitle(string value) =>
        string.IsNullOrEmpty(value)
            ? value
            : char.ToUpperInvariant(value[0]) + value[1..];

    private static async Task<School> EnsureDemoSchoolAsync(AppDbContext db)
    {
        var school = await db.Schools.FirstOrDefaultAsync(s => s.SchoolCode == "BFA-001");
        if (school is null && await db.Schools.AnyAsync())
            school = await db.Schools.OrderBy(s => s.CreatedAt).FirstAsync();

        if (school is not null)
        {
            school.Name = "Scuola Materna";
            school.ShortName = "Scuola Materna";
            school.Tagline = "Impara. Esplora. Cresci.";
            school.SchoolType = "Primaria e media";
            school.Description =
                "La Scuola Materna è nata da un’idea semplice: l’infanzia deve essere colorata, sicura e piena di scoperte. Oggi accogliamo più di mille allievi in aule luminose, laboratori creativi e campi vivaci.";
            school.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            return school;
        }

        school = new School
        {
            SchoolCode = "BFA-001",
            Name = "Scuola Materna",
            ShortName = "Scuola Materna",
            Tagline = "Impara. Esplora. Cresci.",
            RegistrationNumber = "REG-BFA-2011",
            Email = "hello@brightfuture.academy",
            Phone = "+1 (555) 214-8800",
            Website = "https://brightfuture.academy",
            Address = "42 Maple Grove, Riverside",
            City = "Riverside",
            Country = "USA",
            PrincipalName = "Grace Okonkwo",
            EstablishedYear = 2011,
            SchoolType = "Primaria e media",
            Description =
                "La Scuola Materna è nata da un’idea semplice: l’infanzia deve essere colorata, sicura e piena di scoperte. Oggi accogliamo più di mille allievi in aule luminose, laboratori creativi e campi vivaci.",
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
                Heading = "DOVE LE PICCOLE MENTI CRESCONO IN GRANDI SOGNI",
                Description =
                    "Un campus colorato dove fiorisce la curiosità, brilla la creatività e ogni bambino può imparare, esplorare e sognare — ogni singolo giorno.",
                ImagePath = Images.KidsRead,
                CtaText = "Esplora la nostra scuola",
                CtaLink = "#about"
            });
        }

        if (!await db.AboutContents.AnyAsync(a => a.SchoolId == schoolId))
        {
            db.AboutContents.Add(new AboutContent
            {
                SchoolId = schoolId,
                Heading = "Una storia di inizi luminosi",
                Description =
                    "Non solo un campus — un percorso colorato che le famiglie amano condividere. Aule illuminate, domande curiose e insegnanti che conoscono ogni bambino per nome.",
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
                OfficeHours = "Lun–Ven 8:00 – 16:00"
            });
        }

        if (!await db.HighlightItems.AnyAsync(h => h.SchoolId == schoolId))
        {
            db.HighlightItems.AddRange(
                new HighlightItem
                {
                    SchoolId = schoolId,
                    Title = "Dove inizia l’apprendimento",
                    Description =
                        "Aule illuminate, domande curiose e insegnanti che conoscono ogni bambino per nome — questo è lo stile Scuola Materna.",
                    ImageOrIcon = Images.Classroom,
                    DisplayOrder = 1
                },
                new HighlightItem
                {
                    SchoolId = schoolId,
                    Title = "La nostra missione",
                    Description =
                        "Coltivare apprendisti curiosi, gentili e sicuri di sé attraverso un insegnamento gioioso ed esperienze significative.",
                    ImageOrIcon = Images.KidsRead,
                    DisplayOrder = 2
                },
                new HighlightItem
                {
                    SchoolId = schoolId,
                    Title = "La nostra visione",
                    Description =
                        "Un mondo in cui ogni bambino si sente visto, stimolato e ispirato a crescere — un passo luminoso alla volta.",
                    ImageOrIcon = Images.Campus,
                    DisplayOrder = 3
                },
                new HighlightItem
                {
                    SchoolId = schoolId,
                    Title = "Perché le famiglie ci scelgono",
                    Description =
                        "Spazi sicuri, apprendimento creativo, solida didattica e un portale che tiene i genitori vicini a ogni traguardo.",
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
                    Name = "Biblioteca moderna",
                    Description = "Libri, spazi di lettura e risorse didattiche.",
                    ImagePath = Images.Library,
                    DisplayOrder = 1
                },
                new FacilityItem
                {
                    SchoolId = schoolId,
                    Name = "Laboratorio di scienze",
                    Description = "Esperimenti pratici e scoperta.",
                    ImagePath = Images.Science,
                    DisplayOrder = 2
                },
                new FacilityItem
                {
                    SchoolId = schoolId,
                    Name = "Laboratorio di informatica",
                    Description = "Tecnologia e apprendimento digitale.",
                    ImagePath = Images.Computers,
                    DisplayOrder = 3
                },
                new FacilityItem
                {
                    SchoolId = schoolId,
                    Name = "Campo sportivo",
                    Description = "Sport all’aperto e attività motorie.",
                    ImagePath = Images.Sports,
                    DisplayOrder = 4
                },
                new FacilityItem
                {
                    SchoolId = schoolId,
                    Name = "Sala arte e creatività",
                    Description = "Pittura, laboratori e espressione creativa.",
                    ImagePath = Images.Art,
                    DisplayOrder = 5
                },
                new FacilityItem
                {
                    SchoolId = schoolId,
                    Name = "Sala musica",
                    Description = "Musica, strumenti e performance.",
                    ImagePath = Images.Music,
                    DisplayOrder = 6
                });
        }

        if (!await db.GalleryItems.AnyAsync(g => g.SchoolId == schoolId))
        {
            var gallery = new (string Path, string Title, string Category)[]
            {
                (Images.Classroom, "Momenti in aula", "Campus"),
                (Images.KidsRead, "Ora della storia", "Apprendimento"),
                (Images.Library, "Ore di silenzio in biblioteca", "Campus"),
                (Images.Sports, "Giornata dello sport", "Eventi"),
                (Images.Art, "Studio d’arte", "Creativo"),
                (Images.Science, "Fiera della scienza", "Eventi"),
                (Images.Campus, "Vista del campus", "Campus"),
                (Images.Play, "Gioia nel cortile", "Campus"),
                (Images.Annual, "Festa annuale", "Eventi"),
                (Images.Music, "Lezione di musica", "Creativo"),
                (Images.FieldTrip, "Gita scolastica", "Eventi"),
                (Images.School, "Cancello di benvenuto", "Campus")
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
}
