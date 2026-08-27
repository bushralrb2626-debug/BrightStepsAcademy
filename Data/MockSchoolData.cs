using BrightStepsAcademy.Models;

namespace BrightStepsAcademy.Data;

public class MockSchoolData : ISchoolData
{
    public IReadOnlyList<School> Schools { get; } =
    [
        new() { Id = "sch-1", Name = "Scuola Materna", Address = "42 Maple Grove, Riverside", Phone = "+1 (555) 214-8800", Email = "hello@brightsteps.academy", Students = 1248, Teachers = 86 },
        new() { Id = "sch-2", Name = "Little Oaks Primary", Address = "18 Willow Lane, Harborview", Phone = "+1 (555) 214-8801", Email = "oaks@brightsteps.academy", Students = 420, Teachers = 28 },
        new() { Id = "sch-3", Name = "Horizon Middle School", Address = "7 Cedar Point, Lakeside", Phone = "+1 (555) 214-8802", Email = "horizon@brightsteps.academy", Students = 610, Teachers = 41 }
    ];

    public IReadOnlyList<UserAccount> Users { get; } =
    [
        U("usr-1", "BSA-SA-001", "Nora Patel", "nora.patel@brightsteps.academy", "Super Admin", "Platform", Images.Teachers[1]),
        U("usr-2", "BSA-ADM-012", "Daniel Reeves", "daniel.reeves@brightsteps.academy", "Admin", "Scuola Materna", Images.Teachers[2]),
        U("usr-3", "BSA-HM-003", "Grace Okonkwo", "grace.okonkwo@brightsteps.academy", "Headmaster", "Scuola Materna", Images.Teachers[5]),
        U("usr-4", "BSA-TCH-041", "Sarah Wilson", "sarah.wilson@brightsteps.academy", "Teacher", "Scuola Materna", Images.Teachers[0], "Mathematics"),
        U("usr-5", "BSA-TCH-028", "David Chen", "david.chen@brightsteps.academy", "Teacher", "Scuola Materna", Images.Teachers[4], "Science"),
        U("usr-6", "BSA-PAR-118", "Amelia Johnson", "amelia.johnson@email.com", "Parent", "Scuola Materna", Images.Parents[0]),
        U("usr-7", "BSA-STU-204", "Alex Rivera", "alex.rivera@student.brightsteps.academy", "Student", "Scuola Materna", Images.Students[0]),
        U("usr-8", "BSA-ADM-019", "Lina Moreau", "lina.moreau@brightsteps.academy", "Admin", "Little Oaks Primary", Images.Teachers[3]),
        U("usr-9", "BSA-HM-007", "Hassan Malik", "hassan.malik@brightsteps.academy", "Headmaster", "Horizon Middle School", Images.Teachers[6]),
        U("usr-10", "BSA-TCH-055", "Amina Rahman", "amina.rahman@brightsteps.academy", "Teacher", "Scuola Materna", Images.Teachers[1], "English"),
        U("usr-11", "BSA-PAR-142", "Marcus Rivera", "marcus.rivera@email.com", "Parent", "Scuola Materna", Images.Parents[1]),
        U("usr-12", "BSA-STU-311", "Emma Johnson", "emma.johnson@student.brightsteps.academy", "Student", "Scuola Materna", Images.Students[1])
    ];

    public IReadOnlyList<Student> Students { get; } =
    [
        S("st-1", "BSA-STU-204", "Alex Rivera", Images.Students[0], "Grade 5", "A", "Marcus Rivera", "par-4", 96),
        S("st-2", "BSA-STU-311", "Emma Johnson", Images.Students[1], "Grade 4", "A", "Amelia Johnson", "par-1", 94),
        S("st-3", "BSA-STU-188", "Noah Patel", Images.Students[2], "Grade 3", "B", "Priya Patel", "par-2", 91),
        S("st-4", "BSA-STU-220", "Lily Chen", Images.Students[3], "Grade 5", "A", "Wei Chen", "par-3", 98),
        S("st-5", "BSA-STU-176", "Omar Hassan", Images.Students[4], "Grade 4", "B", "Layla Hassan", "par-5", 88),
        S("st-6", "BSA-STU-145", "Sofia Martinez", Images.Students[5], "Grade 3", "A", "Elena Martinez", "par-6", 93),
        S("st-7", "BSA-STU-233", "Ethan Brooks", Images.Students[6], "Grade 5", "B", "James Brooks", "par-7", 85),
        S("st-8", "BSA-STU-301", "Zara Ahmed", Images.Students[7], "Grade 4", "A", "Amelia Johnson", "par-1", 97),
        S("st-9", "BSA-STU-159", "Lucas Kim", Images.Students[8], "Grade 3", "A", "Min-jun Kim", "par-8", 90),
        S("st-10", "BSA-STU-241", "Maya Singh", Images.Students[9], "Grade 5", "A", "Anika Singh", "par-9", 95),
        S("st-11", "BSA-STU-198", "Daniel Wright", Images.Students[10], "Grade 4", "B", "Owen Wright", "par-10", 87),
        S("st-12", "BSA-STU-167", "Hana Yoshida", Images.Students[11], "Grade 3", "B", "Yuki Yoshida", "par-11", 92)
    ];

    public IReadOnlyList<Teacher> Teachers { get; } =
    [
        T("tc-1", "BSA-TCH-041", "Sarah Wilson", Images.Teachers[0], "Mathematics", "Grade 3-A, 4-B, 5-A", 8, "Sarah turns numbers into stories. Her classroom is full of puzzles, color, and quiet confidence."),
        T("tc-2", "BSA-TCH-028", "David Chen", Images.Teachers[4], "Science", "Grade 4-A, 5-A, 5-B", 6, "David believes every question is an experiment waiting to happen."),
        T("tc-3", "BSA-TCH-055", "Amina Rahman", Images.Teachers[1], "English", "Grade 3-B, 4-A, 5-A", 10, "Amina helps children find their voice through stories, poetry and debate."),
        T("tc-4", "BSA-TCH-033", "James Okonkwo", Images.Teachers[2], "Physical Education", "All grades", 7, "James coaches teamwork, grit and joy — on the field and off it."),
        T("tc-5", "BSA-TCH-019", "Priya Sharma", Images.Teachers[3], "Art", "Grade 3, 4, 5", 5, "Priya’s studio is a place where mess is welcome and imagination leads."),
        T("tc-6", "BSA-TCH-061", "Michael Torres", Images.Teachers[6], "Computer Science", "Grade 4-B, 5-A, 5-B", 9, "Michael introduces coding as a creative language, not a chore."),
        T("tc-7", "BSA-TCH-012", "Fatima Ali", Images.Teachers[5], "Urdu", "Grade 3-A, 4-A, 5-B", 12, "Fatima celebrates language, culture and the beauty of careful words."),
        T("tc-8", "BSA-TCH-074", "Emma Brooks", Images.Teachers[7], "Music", "All grades", 4, "Emma fills the halls with rhythm, choirs and a little everyday magic.")
    ];

    public IReadOnlyList<Parent> Parents { get; } =
    [
        P("par-1", "BSA-PAR-118", "Amelia Johnson", Images.Parents[0], "+1 555 201 4411", "amelia.johnson@email.com", "Emma Johnson, Zara Ahmed"),
        P("par-2", "BSA-PAR-121", "Priya Patel", Images.Parents[2], "+1 555 201 4412", "priya.patel@email.com", "Noah Patel"),
        P("par-3", "BSA-PAR-130", "Wei Chen", Images.Parents[3], "+1 555 201 4413", "wei.chen@email.com", "Lily Chen"),
        P("par-4", "BSA-PAR-142", "Marcus Rivera", Images.Parents[1], "+1 555 201 4414", "marcus.rivera@email.com", "Alex Rivera"),
        P("par-5", "BSA-PAR-155", "Layla Hassan", Images.Parents[4], "+1 555 201 4415", "layla.hassan@email.com", "Omar Hassan"),
        P("par-6", "BSA-PAR-160", "Elena Martinez", Images.Parents[0], "+1 555 201 4416", "elena.martinez@email.com", "Sofia Martinez"),
        P("par-7", "BSA-PAR-171", "James Brooks", Images.Parents[5], "+1 555 201 4417", "james.brooks@email.com", "Ethan Brooks"),
        P("par-8", "BSA-PAR-182", "Min-jun Kim", Images.Parents[3], "+1 555 201 4418", "minjun.kim@email.com", "Lucas Kim")
    ];

    public IReadOnlyList<SchoolClass> Classes { get; } =
    [
        new() { Id = "cl-1", Name = "Grade 3-A", Grade = "3", Section = "A", Teacher = "Sarah Wilson", Students = 28, Room = "Sunflower 12", Schedule = "Mon–Fri 8:30–2:30" },
        new() { Id = "cl-2", Name = "Grade 3-B", Grade = "3", Section = "B", Teacher = "Amina Rahman", Students = 26, Room = "Daisy 08", Schedule = "Mon–Fri 8:30–2:30" },
        new() { Id = "cl-3", Name = "Grade 4-A", Grade = "4", Section = "A", Teacher = "David Chen", Students = 30, Room = "Maple 21", Schedule = "Mon–Fri 8:20–2:40" },
        new() { Id = "cl-4", Name = "Grade 4-B", Grade = "4", Section = "B", Teacher = "Sarah Wilson", Students = 27, Room = "Maple 22", Schedule = "Mon–Fri 8:20–2:40" },
        new() { Id = "cl-5", Name = "Grade 5-A", Grade = "5", Section = "A", Teacher = "Sarah Wilson", Students = 29, Room = "Oak 31", Schedule = "Mon–Fri 8:10–2:50" },
        new() { Id = "cl-6", Name = "Grade 5-B", Grade = "5", Section = "B", Teacher = "Michael Torres", Students = 28, Room = "Oak 32", Schedule = "Mon–Fri 8:10–2:50" }
    ];

    public IReadOnlyList<ProgramItem> Programs { get; } =
    [
        new() { Title = "Early Learning", Description = "Building curiosity through play and discovery.", Image = Images.Play, Icon = "blocks", Accent = "yellow" },
        new() { Title = "Primary School", Description = "Strong foundations for lifelong learning.", Image = Images.Classroom, Icon = "book", Accent = "sky" },
        new() { Title = "Middle School", Description = "Developing knowledge, confidence and independence.", Image = Images.Campus, Icon = "cap", Accent = "royal" },
        new() { Title = "Creative Arts", Description = "Helping students express their imagination.", Image = Images.Art, Icon = "palette", Accent = "pink" },
        new() { Title = "Sports", Description = "Building teamwork, confidence and healthy habits.", Image = Images.Sports, Icon = "ball", Accent = "orange" },
        new() { Title = "Science & Technology", Description = "Exploring the world through innovation and discovery.", Image = Images.Science, Icon = "flask", Accent = "mint" }
    ];

    public IReadOnlyList<Facility> Facilities { get; } =
    [
        new() { Id = "lib", Name = "Modern Library", Description = "Books, reading spaces and learning resources.", Image = Images.Library, Icon = "book", Accent = "sky", Featured = true, Size = "lg" },
        new() { Id = "sci", Name = "Science Laboratory", Description = "Hands-on experiments and discovery.", Image = Images.Science, Icon = "flask", Accent = "purple", Size = "md" },
        new() { Id = "comp", Name = "Computer Lab", Description = "Technology and digital learning.", Image = Images.Computers, Icon = "laptop", Accent = "orange", Size = "md" },
        new() { Id = "sport", Name = "Sports Ground", Description = "Outdoor sports and physical activities.", Image = Images.Sports, Icon = "ball", Accent = "grass", Featured = true, Size = "wide" },
        new() { Id = "art", Name = "Art & Creativity Room", Description = "Painting, crafts and creative expression.", Image = Images.Art, Icon = "palette", Accent = "pink", Size = "md" },
        new() { Id = "music", Name = "Music Room", Description = "Music, instruments and performance.", Image = Images.Music, Icon = "music", Accent = "yellow", Size = "md" },
        new() { Id = "play", Name = "Play Area", Description = "Safe and engaging recreational space.", Image = Images.Play, Icon = "blocks", Accent = "coral", Size = "md" },
        new() { Id = "cafe", Name = "Cafeteria", Description = "Comfortable student dining area.", Image = Images.Cafeteria, Icon = "apple", Accent = "orange", Size = "md" },
        new() { Id = "med", Name = "Medical / First Aid Room", Description = "Student health and first-aid support.", Image = Images.Medical, Icon = "heart", Accent = "pink", Size = "sm" },
        new() { Id = "bus", Name = "School Transport", Description = "Safe transportation facilities.", Image = Images.Bus, Icon = "bus", Accent = "royal", Size = "sm" },
        new() { Id = "smart", Name = "Smart Classrooms", Description = "Interactive learning environments.", Image = Images.Classroom, Icon = "board", Accent = "sky", Size = "sm" },
        new() { Id = "safe", Name = "Safe & Secure Campus", Description = "Modern campus safety systems.", Image = Images.Security, Icon = "shield", Accent = "grass", Size = "sm" }
    ];

    public IReadOnlyList<FeatureItem> Features { get; } =
    [
        new() { Title = "Qualified Teachers", Description = "Warm, trained educators who know every child by name.", Icon = "apple", Accent = "coral" },
        new() { Title = "Safe Environment", Description = "Secure campus, caring staff and clear routines.", Icon = "shield", Accent = "sky" },
        new() { Title = "Creative Learning", Description = "Projects, stories and play that make ideas stick.", Icon = "palette", Accent = "pink" },
        new() { Title = "Modern Classrooms", Description = "Bright rooms, smart boards and space to move.", Icon = "board", Accent = "royal" },
        new() { Title = "Sports & Activities", Description = "Fields, clubs and festivals for every interest.", Icon = "ball", Accent = "orange" },
        new() { Title = "Parent Engagement", Description = "Clear updates, meetings and a portal that stays in sync.", Icon = "family", Accent = "mint" }
    ];

    public IReadOnlyList<EventItem> Events { get; } =
    [
        E("ev-1", "Annual Sports Day", "Races, relays and house cheers on the sunlit field.", Images.SportsDay, new(2026, 9, 18), "Sports Ground", "8:30 AM"),
        E("ev-2", "Science Exhibition", "Student inventions, volcanoes and curious questions.", Images.ScienceFair, new(2026, 9, 25), "Science Wing", "10:00 AM"),
        E("ev-3", "Parent Teacher Meeting", "A warm conversation about progress and next steps.", Images.Meeting, new(2026, 9, 12), "Main Hall", "2:00 PM"),
        E("ev-4", "Art & Creativity Week", "Galleries, workshops and a splash of colour.", Images.Art, new(2026, 10, 6), "Art Studio", "All week"),
        E("ev-5", "Book Fair", "New stories, favourite authors and reading corners.", Images.BookFair, new(2026, 10, 14), "Library", "9:00 AM"),
        E("ev-6", "School Picnic", "Games, packed lunches and a day outdoors.", Images.Picnic, new(2026, 10, 22), "Riverside Park", "9:30 AM")
    ];

    public IReadOnlyList<Notice> Notices { get; } =
    [
        new() { Id = "n1", Title = "Parent Teacher Meeting — September 12", Body = "Please book a 15-minute slot through the parent portal by Friday.", Category = "Important", Date = new(2026, 8, 20) },
        new() { Id = "n2", Title = "Science Exhibition registrations are now open.", Body = "Grade 4 and 5 students can register projects until September 10.", Category = "Event", Date = new(2026, 8, 22) },
        new() { Id = "n3", Title = "School will remain closed on Friday.", Body = "Campus closed for staff development. Regular classes resume Monday.", Category = "Holiday", Date = new(2026, 8, 24) },
        new() { Id = "n4", Title = "Library reading challenge starts next week.", Body = "Collect a bookmark from Ms. Amina and log 20 minutes a day.", Category = "General", Date = new(2026, 8, 25) },
        new() { Id = "n5", Title = "Sports Day house lists published.", Body = "Check the notice board or your class page for house assignments.", Category = "Event", Date = new(2026, 8, 26) }
    ];

    public IReadOnlyList<Assignment> Assignments { get; } =
    [
        new() { Id = "a1", Title = "Fractions in the Wild", Subject = "Mathematics", ClassName = "Grade 5-A", DueDate = new(2026, 9, 4), Description = "Find fractions in recipes, sports scores or nature and present three examples.", SubmissionPercent = 78, Status = "Published" },
        new() { Id = "a2", Title = "Plant Journal", Subject = "Science", ClassName = "Grade 4-A", DueDate = new(2026, 9, 8), Description = "Observe a plant for a week and sketch one change each day.", SubmissionPercent = 54, Status = "Published" },
        new() { Id = "a3", Title = "My Neighbourhood Story", Subject = "English", ClassName = "Grade 3-A", DueDate = new(2026, 9, 2), Description = "Write a one-page story about a kind person on your street.", SubmissionPercent = 91, Status = "Published" },
        new() { Id = "a4", Title = "Scratch Mini Game", Subject = "Computer", ClassName = "Grade 5-B", DueDate = new(2026, 9, 15), Description = "Build a 30-second game with a start screen and a score.", SubmissionPercent = 22, Status = "Draft" },
        new() { Id = "a5", Title = "Urdu Vocabulary Cards", Subject = "Urdu", ClassName = "Grade 4-B", DueDate = new(2026, 9, 6), Description = "Create 10 illustrated cards for new words this unit.", SubmissionPercent = 67, Status = "Published" }
    ];

    public IReadOnlyList<AttendanceRow> Attendance { get; }

    public IReadOnlyList<ResultItem> Results { get; } =
    [
        new() { Subject = "Mathematics", Marks = 92, Grade = "A", Performance = "Excellent" },
        new() { Subject = "English", Marks = 88, Grade = "A", Performance = "Strong" },
        new() { Subject = "Science", Marks = 95, Grade = "A+", Performance = "Outstanding" },
        new() { Subject = "Computer", Marks = 90, Grade = "A", Performance = "Excellent" },
        new() { Subject = "Urdu", Marks = 84, Grade = "B+", Performance = "Good" }
    ];

    public IReadOnlyList<MessageThread> Threads { get; } =
    [
        new()
        {
            Id = "m1", Name = "Amelia Johnson", Avatar = Images.Parents[0], Preview = "Thank you for the reading list!", Time = "9:12 AM", Unread = 1,
            Messages =
            [
                new() { From = "Amelia", Text = "Good morning — Emma loved the science project.", Time = "8:40 AM" },
                new() { From = "You", Text = "That’s wonderful to hear. She presented it with real confidence.", Time = "8:52 AM", Mine = true },
                new() { From = "Amelia", Text = "Thank you for the reading list!", Time = "9:12 AM" }
            ]
        },
        new()
        {
            Id = "m2", Name = "David Chen", Avatar = Images.Teachers[4], Preview = "Can we share the lab on Thursday?", Time = "Yesterday", Unread = 0,
            Messages =
            [
                new() { From = "David", Text = "Can we share the lab on Thursday?", Time = "Yesterday" },
                new() { From = "You", Text = "Yes — 11:00 works for Grade 5-A.", Time = "Yesterday", Mine = true }
            ]
        },
        new()
        {
            Id = "m3", Name = "Office Desk", Avatar = Images.School, Preview = "Sports Day volunteer slots are live.", Time = "Mon", Unread = 2,
            Messages =
            [
                new() { From = "Office", Text = "Sports Day volunteer slots are live.", Time = "Mon" }
            ]
        }
    ];

    public IReadOnlyList<GalleryItem> Gallery { get; } =
    [
        new() { Image = Images.Classroom, Caption = "Morning in a bright classroom", Category = "Classrooms" },
        new() { Image = Images.KidsRead, Caption = "Students diving into a new story", Category = "Students" },
        new() { Image = Images.Teachers[0], Caption = "Ms. Wilson with Grade 5", Category = "Teachers" },
        new() { Image = Images.Sports, Caption = "House races on Sports Day", Category = "Sports" },
        new() { Image = Images.Art, Caption = "Colour and clay in the studio", Category = "Art" },
        new() { Image = Images.Science, Caption = "Lab coats and big questions", Category = "Science" },
        new() { Image = Images.Annual, Caption = "Annual function lights up", Category = "Events" },
        new() { Image = Images.School, Caption = "BrightSteps campus at sunrise", Category = "School building" },
        new() { Image = Images.Library, Caption = "Quiet corners of the library", Category = "Library" },
        new() { Image = Images.Play, Caption = "Laughter on the playground", Category = "Playground" },
        new() { Image = Images.Music, Caption = "Choir practice after lunch", Category = "Events" },
        new() { Image = Images.FieldTrip, Caption = "A day of discovery outdoors", Category = "Events" }
    ];

    public IReadOnlyList<ActivityItem> Activities { get; } =
    [
        new() { Title = "Art Class", Image = Images.Art, Description = "Brushes, collage and colour mixing every Wednesday." },
        new() { Title = "Science Fair", Image = Images.ScienceFair, Description = "Experiments, posters and proud inventors." },
        new() { Title = "Sports Day", Image = Images.SportsDay, Description = "Races, relays and plenty of orange slices." },
        new() { Title = "Field Trip", Image = Images.FieldTrip, Description = "Learning beyond the classroom walls." },
        new() { Title = "Reading Week", Image = Images.Reading, Description = "Blankets, book nooks and favourite characters." },
        new() { Title = "Annual Function", Image = Images.Annual, Description = "Music, drama and a stage full of sparkle." }
    ];

    public IReadOnlyList<NotificationItem> Notifications { get; } =
    [
        new() { Title = "New assignment posted.", Body = "Fractions in the Wild is due September 4.", Time = "12 min ago", Type = "assignment" },
        new() { Title = "Parent Teacher Meeting tomorrow.", Body = "Slots start at 2:00 PM in the Main Hall.", Time = "1 hr ago", Type = "event" },
        new() { Title = "Your attendance has been updated.", Body = "Grade 5-A is at 96% this week.", Time = "3 hr ago", Type = "attendance" },
        new() { Title = "New school announcement.", Body = "Library reading challenge starts next week.", Time = "Yesterday", Type = "notice" }
    ];

    public IReadOnlyList<ActivityLog> RecentActivity { get; } =
    [
        new() { Text = "New administrator created", Time = "10 min ago", Accent = "royal" },
        new() { Text = "Teacher account assigned", Time = "42 min ago", Accent = "mint" },
        new() { Text = "New school added", Time = "2 hr ago", Accent = "orange" },
        new() { Text = "Student registered", Time = "Yesterday", Accent = "pink" }
    ];

    public IReadOnlyList<TimelineItem> TodaySchedule { get; } =
    [
        new() { Time = "08:30", Title = "Morning assembly", Meta = "Main courtyard", Accent = "yellow" },
        new() { Time = "09:00", Title = "Mathematics · Grade 5-A", Meta = "Oak 31", Accent = "royal" },
        new() { Time = "10:00", Title = "Science lab", Meta = "Discovery Wing", Accent = "mint" },
        new() { Time = "11:20", Title = "English workshop", Meta = "Library loft", Accent = "coral" },
        new() { Time = "13:10", Title = "PE outdoors", Meta = "Sports Ground", Accent = "orange" },
        new() { Time = "14:20", Title = "Art studio", Meta = "Creativity Room", Accent = "pink" }
    ];

    public IReadOnlyList<TimetableSlot> Timetable { get; } =
    [
        Slot("08:30", "Assembly", "Assembly", "Assembly", "Assembly", "Assembly"),
        Slot("09:00", "Mathematics", "English", "Science", "Mathematics", "Urdu"),
        Slot("10:00", "Science", "Mathematics", "English", "Computer", "Science"),
        Slot("11:20", "English", "Art", "Mathematics", "PE", "English"),
        Slot("12:10", "Lunch", "Lunch", "Lunch", "Lunch", "Lunch"),
        Slot("13:10", "PE", "Computer", "Urdu", "Art", "Music"),
        Slot("14:10", "Urdu", "Science", "PE", "English", "Library")
    ];

    public IReadOnlyList<string> Subjects { get; } = ["Mathematics", "English", "Science", "Computer", "Urdu", "Art", "PE", "Music"];

    public MockSchoolData()
    {
        Attendance = Students.Select((s, i) => new AttendanceRow
        {
            StudentId = s.StudentId,
            StudentName = s.Name,
            Photo = s.Photo,
            Mark = i % 7 == 0 ? "Late" : i % 11 == 0 ? "Absent" : "Present"
        }).ToList();
    }

    public DashboardProfile ProfileFor(string role) => role switch
    {
        "SuperAdmin" => new()
        {
            Role = "Super Admin", DisplayName = "Nora Patel", FirstName = "Nora",
            Avatar = Images.Teachers[1], UserId = "BSA-SA-001", Email = "nora.patel@brightsteps.academy",
            Phone = "+1 555 214 0100", Greeting = "Good Morning, Super Admin! 👋",
            Subtitle = "Here's what's happening across your school system."
        },
        "Admin" => new()
        {
            Role = "Admin", DisplayName = "Daniel Reeves", FirstName = "Daniel",
            Avatar = Images.Teachers[2], UserId = "BSA-ADM-012", Email = "daniel.reeves@brightsteps.academy",
            Phone = "+1 555 214 0120", Greeting = "Welcome Back, Admin! 🏫",
            Subtitle = "Scuola Materna is humming along today."
        },
        "Headmaster" => new()
        {
            Role = "Headmaster", DisplayName = "Grace Okonkwo", FirstName = "Grace",
            Avatar = Images.Teachers[5], UserId = "BSA-HM-003", Email = "grace.okonkwo@brightsteps.academy",
            Phone = "+1 555 214 0130", Greeting = "Good Morning, Headmaster! 🎓",
            Subtitle = "Your school command center for the day."
        },
        "Teacher" => new()
        {
            Role = "Teacher", DisplayName = "Sarah Wilson", FirstName = "Sarah",
            Avatar = Images.Teachers[0], UserId = "BSA-TCH-041", Email = "sarah.wilson@brightsteps.academy",
            Phone = "+1 555 214 0141", Greeting = "Good Morning, Ms. Sarah! 👩‍🏫",
            Subtitle = "Three classes, one lovely Thursday."
        },
        "Parent" => new()
        {
            Role = "Parent", DisplayName = "Amelia Johnson", FirstName = "Amelia",
            Avatar = Images.Parents[0], UserId = "BSA-PAR-118", Email = "amelia.johnson@email.com",
            Phone = "+1 555 201 4411", Greeting = "Welcome Back! 👨‍👩‍👧",
            Subtitle = "Emma and Zara are having a bright week."
        },
        _ => new()
        {
            Role = "Student", DisplayName = "Alex Rivera", FirstName = "Alex",
            Avatar = Images.Students[0], UserId = "BSA-STU-204", Email = "alex.rivera@student.brightsteps.academy",
            Phone = "+1 555 201 2040", Greeting = "Hey Alex! Ready to learn today? 🚀",
            Subtitle = "Homework hero energy — let's keep it going."
        }
    };

    private static UserAccount U(string id, string userId, string name, string email, string role, string school, string avatar, string subject = "") =>
        new() { Id = id, UserId = userId, FullName = name, Email = email, Phone = "+1 555 214 8800", Role = role, School = school, Avatar = avatar, Subject = subject, Status = "Active" };

    private static Student S(string id, string sid, string name, string photo, string cls, string sec, string parent, string pid, int att) =>
        new() { Id = id, StudentId = sid, Name = name, Photo = photo, ClassName = cls, Section = sec, ParentName = parent, ParentId = pid, Attendance = att, Email = sid.ToLower() + "@student.brightsteps.academy", Age = 8 + int.Parse(cls.Split(' ')[1]) };

    private static Teacher T(string id, string tid, string name, string photo, string subject, string classes, int years, string bio) =>
        new() { Id = id, TeacherId = tid, Name = name, Photo = photo, Subject = subject, Classes = classes, ExperienceYears = years, Bio = bio, Email = name.Split(' ')[0].ToLower() + "@brightsteps.academy", Phone = "+1 555 214 0100", Status = "Active" };

    private static Parent P(string id, string pid, string name, string photo, string phone, string email, string children) =>
        new() { Id = id, ParentId = pid, Name = name, Photo = photo, Phone = phone, Email = email, Children = children, Status = "Active" };

    private static EventItem E(string id, string title, string desc, string image, DateTime date, string loc, string time) =>
        new() { Id = id, Title = title, Description = desc, Image = image, Date = date, Location = loc, Time = time };

    private static TimetableSlot Slot(string time, string mo, string tu, string we, string th, string fr) =>
        new() { Time = time, Monday = mo, Tuesday = tu, Wednesday = we, Thursday = th, Friday = fr };
}
