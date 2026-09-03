/**
 * BrightSteps static demo — role-based dashboard UI.
 */
(function () {
  "use strict";

  var auth = window.BrightStepsDemoAuth;
  if (!auth) return;

  var NAV = {
    student: [
      { icon: "🏠", label: "Dashboard", id: "home" },
      { icon: "📅", label: "Timetable", id: "timetable" },
      { icon: "📚", label: "Assignments", id: "assignments" },
      { icon: "📊", label: "Marks", id: "marks" },
    ],
    parent: [
      { icon: "🏠", label: "Dashboard", id: "home" },
      { icon: "📖", label: "Diary", id: "diary" },
      { icon: "✅", label: "Attendance", id: "attendance" },
      { icon: "📣", label: "Announcements", id: "announcements" },
    ],
    teacher: [
      { icon: "🏠", label: "Dashboard", id: "home" },
      { icon: "👥", label: "My class", id: "class" },
      { icon: "📝", label: "Assignments", id: "assignments" },
      { icon: "📋", label: "Attendance", id: "attendance" },
      { icon: "📊", label: "Results", id: "results" },
    ],
    headmaster: [
      { icon: "🏠", label: "Overview", id: "home" },
      { icon: "👩‍🏫", label: "Staff", id: "staff" },
      { icon: "🧒", label: "Students", id: "students" },
      { icon: "📈", label: "Results", id: "reports" },
    ],
    admin: [
      { icon: "🏠", label: "Dashboard", id: "home" },
      { icon: "📅", label: "Meetings", id: "meetings" },
      { icon: "👩‍🏫", label: "Teachers", id: "staff" },
      { icon: "🧒", label: "Students", id: "students" },
      { icon: "📊", label: "Results", id: "results" },
      { icon: "⚙️", label: "Settings", id: "settings" },
    ],
    superadmin: [
      { icon: "🏠", label: "Platform", id: "home" },
      { icon: "📅", label: "Meetings", id: "meetings" },
      { icon: "🏫", label: "Schools", id: "schools" },
      { icon: "👩‍🏫", label: "Teachers", id: "teachers" },
      { icon: "🧒", label: "Students", id: "students" },
      { icon: "📊", label: "Results", id: "results" },
      { icon: "🛡️", label: "Admins", id: "admins" },
    ],
  };

  var SCHOOLS = [
    { name: "Scuola Materna", city: "Riverside", students: 412, teachers: 38, attendance: "94%" },
    { name: "BrightFuture Academy", city: "Harbor", students: 286, teachers: 24, attendance: "91%" },
    { name: "Maple Grove Primary", city: "Riverside", students: 198, teachers: 16, attendance: "96%" },
  ];
  var TEACHERS = [
    { name: "Sarah Wilson", school: "Scuola Materna", subject: "Mathematics", className: "Maple · 4A" },
    { name: "David Chen", school: "Scuola Materna", subject: "Science", className: "Lab · 4B" },
    { name: "Amina Rahman", school: "Scuola Materna", subject: "English", className: "4A / 5A" },
    { name: "Priya Sharma", school: "BrightFuture Academy", subject: "Art", className: "Studio" },
    { name: "James Okonkwo", school: "Maple Grove Primary", subject: "PE", className: "All years" },
  ];
  var STUDENTS = [
    { name: "Alex Rivera", school: "Scuola Materna", year: "Grade 4", avg: "88%" },
    { name: "Mia Chen", school: "Scuola Materna", year: "Grade 4", avg: "91%" },
    { name: "Noah Patel", school: "Scuola Materna", year: "Grade 5", avg: "84%" },
    { name: "Sofia Rossi", school: "BrightFuture Academy", year: "Grade 3", avg: "90%" },
    { name: "Leo Mensah", school: "Maple Grove Primary", year: "Grade 2", avg: "87%" },
  ];
  var KIDS_KEY = "brightsteps-demo-kids";

  function loadKids() {
    try {
      var raw = localStorage.getItem(KIDS_KEY);
      var list = raw ? JSON.parse(raw) : [];
      return Array.isArray(list) ? list : [];
    } catch (e) {
      return [];
    }
  }

  function saveKids(list) {
    localStorage.setItem(KIDS_KEY, JSON.stringify(list.slice(0, 200)));
  }

  function allStudents() {
    return STUDENTS.concat(loadKids());
  }

  function addKidForm(schoolDefault) {
    var school = escapeHtml(schoolDefault || "BrightFuture Academy");
    return (
      '<form class="form-bsa" id="addKidForm" style="margin-bottom:1.25rem">' +
      "<p><strong>Add a child</strong> — they get a student login and appear on this list.</p>" +
      '<div class="form-row">' +
      '<label>Full name<input name="name" required maxlength="80" placeholder="e.g. Ayaan Khan" /></label>' +
      '<label>Year / class<input name="year" required maxlength="40" placeholder="e.g. Grade 1" value="Grade 1" /></label>' +
      "</div>" +
      '<div class="form-row">' +
      '<label>School<input name="school" required maxlength="80" value="' +
      school +
      '" /></label>' +
      '<label>Student email (login)<input name="email" type="email" required placeholder="child@email.com" /></label>' +
      "</div>" +
      '<label>Temporary password<input name="password" value="Demo@12345" minlength="6" /></label>' +
      '<button type="submit" class="btn-bsa btn-bsa-primary">Add child</button>' +
      "</form>"
    );
  }

  function studentsPanel(session) {
    var rows = allStudents().map(function (s) {
      return [escapeHtml(s.name), escapeHtml(s.school), escapeHtml(s.year), escapeHtml(s.avg || "—")];
    });
    return (
      addKidForm(session && session.className ? session.className : "BrightFuture Academy") +
      panel("Students", table(["Name", "School", "Year", "Average"], rows))
    );
  }
  var RESULTS = [
    { student: "Alex Rivera", school: "Scuola Materna", subject: "Math", mark: "88%" },
    { student: "Alex Rivera", school: "Scuola Materna", subject: "Science", mark: "92%" },
    { student: "Mia Chen", school: "Scuola Materna", subject: "English", mark: "91%" },
    { student: "Sofia Rossi", school: "BrightFuture Academy", subject: "Art", mark: "94%" },
    { student: "Leo Mensah", school: "Maple Grove Primary", subject: "PE", mark: "87%" },
  ];

  function loadVisits() {
    try {
      var raw = localStorage.getItem("brightsteps-demo-visits");
      var list = raw ? JSON.parse(raw) : [];
      return Array.isArray(list) ? list : [];
    } catch (e) {
      return [];
    }
  }

  function escapeHtml(value) {
    return String(value == null ? "" : value)
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;");
  }

  function meetingsPanel() {
    var visits = loadVisits();
    if (!visits.length) {
      return panel(
        "Campus visit meetings",
        "<p>No visit bookings yet. When a signed-in parent books a visit in the campus chat, it appears here.</p>"
      );
    }
    return panel(
      "Campus visit meetings",
      table(
        ["When requested", "Name", "Email", "Visit time", "Child / year"],
        visits.map(function (v) {
          return [
            escapeHtml(v.createdAt ? String(v.createdAt).replace("T", " ").slice(0, 16) : ""),
            escapeHtml(v.name),
            escapeHtml(v.email),
            escapeHtml(v.when),
            escapeHtml(v.age),
          ];
        })
      )
    );
  }

  function greeting() {
    var hour = new Date().getHours();
    if (hour < 12) return "Good morning";
    if (hour < 17) return "Good afternoon";
    return "Good evening";
  }

  function kpis(items) {
    return (
      '<div class="grid-4 mb-2">' +
      items
        .map(function (item) {
          return (
            '<article class="kpi-card ' +
            item.accent +
            '"><p class="kpi-label">' +
            item.label +
            '</p><h3 class="mb-0">' +
            item.value +
            "</h3></article>"
          );
        })
        .join("") +
      "</div>"
    );
  }

  function panel(title, body) {
    return '<div class="dash-panel"><div class="dash-panel__head"><h3>' + title + "</h3></div>" + body + "</div>";
  }

  function table(headers, rows) {
    var head = headers.map(function (h) { return "<th>" + h + "</th>"; }).join("");
    var body = rows
      .map(function (row) {
        return "<tr>" + row.map(function (cell) { return "<td>" + cell + "</td>"; }).join("") + "</tr>";
      })
      .join("");
    return (
      '<div class="table-responsive"><table class="dash-table"><thead><tr>' +
      head +
      "</tr></thead><tbody>" +
      body +
      "</tbody></table></div>"
    );
  }

  function timeline(items) {
    return (
      '<div class="timeline">' +
      items
        .map(function (item) {
          return (
            '<div class="timeline-item"><div class="time">' +
            item.time +
            "</div><div><strong>" +
            item.title +
            "</strong><span>" +
            item.detail +
            "</span></div></div>"
          );
        })
        .join("") +
      "</div>"
    );
  }

  function welcome(session, subtitle) {
    return (
      '<div class="welcome-banner"><h2>' +
      greeting() +
      ", " +
      session.name.split(" ")[0] +
      '</h2><p>' +
      (subtitle || session.className || "") +
      "</p></div>"
    );
  }

  function contentFor(session, section) {
    var role = session.role;
    section = section || "home";

    if (role === "student") {
      if (section === "timetable") {
        return panel(
          "Today's timetable",
          timeline([
            { time: "8:30", title: "Mathematics", detail: "Room 12 · Ms. Wilson" },
            { time: "10:00", title: "Science", detail: "Lab 2 · Mr. Chen" },
            { time: "11:30", title: "English", detail: "Room 8 · Ms. Rahman" },
            { time: "13:30", title: "Art", detail: "Studio · Ms. Sharma" },
          ])
        );
      }
      if (section === "assignments") {
        return panel(
          "Upcoming assignments",
          "<p><strong>Fractions worksheet</strong> — due Friday</p><p><strong>Plant diary</strong> — due next Monday</p><p><strong>Reading log</strong> — due Wednesday</p>"
        );
      }
      if (section === "marks") {
        return panel(
          "Recent marks",
          table(["Subject", "Mark", "Teacher"], [
            ["Mathematics", "88%", "Sarah Wilson"],
            ["Science", "92%", "David Chen"],
            ["English", "85%", "Amina Rahman"],
          ])
        );
      }
      return (
        welcome(session) +
        kpis([
          { label: "Attendance", value: "96%", accent: "accent-mint" },
          { label: "Pending work", value: "2", accent: "accent-sky" },
          { label: "Upcoming exams", value: "1", accent: "accent-royal" },
          { label: "Recent average", value: "88%", accent: "accent-coral" },
        ]) +
        '<div class="grid-2">' +
        panel(
          "Today's classes",
          timeline([
            { time: "8:30", title: "Mathematics", detail: "Room 12" },
            { time: "10:00", title: "Science", detail: "Lab 2" },
          ])
        ) +
        panel("Today's diary", "<p><strong>Science</strong> — Plant growth observation</p><p class='text-muted small'>Homework: draw and label the stem.</p>") +
        "</div>"
      );
    }

    if (role === "parent") {
      if (section === "diary") {
        return panel(
          "Class diary",
          "<p><strong>Science</strong> — Plant growth topic introduced.</p><p><strong>English</strong> — Creative writing: my weekend.</p><p><strong>Math</strong> — Fractions with counters.</p>"
        );
      }
      if (section === "attendance") {
        return panel("Attendance", "<p>Alex Rivera — <strong>96%</strong> this term</p><p class='text-muted small'>2 late arrivals · 1 excused absence</p>");
      }
      if (section === "announcements") {
        return panel("Announcements", "<p>📣 Spring sports day — Friday 14 March</p><p>📣 Parent evening bookings open</p><p>📣 Library book fair next week</p>");
      }
      return (
        welcome(session, "Diary, attendance and announcements for your linked children.") +
        kpis([
          { label: "Alex's attendance", value: "96%", accent: "accent-mint" },
          { label: "Unread notices", value: "2", accent: "accent-sky" },
          { label: "Average", value: "88%", accent: "accent-royal" },
          { label: "Events", value: "1", accent: "accent-coral" },
        ]) +
        panel(
          "Alex Rivera · Grade 4",
          "<p>Maple Class · STU-1042</p><p><button type='button' class='btn-bsa btn-bsa-sm btn-bsa-soft' data-section='diary'>Diary</button> <button type='button' class='btn-bsa btn-bsa-sm btn-bsa-soft' data-section='attendance'>Attendance</button></p>"
        )
      );
    }

    if (role === "teacher") {
      if (section === "class") {
        return panel(
          "Maple Class · 24 students",
          table(["Student", "Attendance", "Last mark"], [
            ["Alex Rivera", "96%", "88%"],
            ["Mia Chen", "98%", "91%"],
            ["Noah Patel", "90%", "84%"],
            ["Emma Brooks", "97%", "89%"],
          ])
        );
      }
      if (section === "assignments") {
        return panel(
          "Assignments",
          "<p><strong>Fractions worksheet</strong> — published · 18 / 24 submitted</p><p><strong>Plant diary</strong> — draft</p><p><button type='button' class='btn-bsa btn-bsa-sm btn-bsa-primary' data-demo-action='publish'>Publish new (demo)</button></p>"
        );
      }
      if (section === "attendance") {
        return panel(
          "Today's attendance",
          table(["Student", "Status"], [
            ["Alex Rivera", "Present"],
            ["Mia Chen", "Present"],
            ["Noah Patel", "Late"],
            ["Emma Brooks", "Absent"],
          ]) + "<p class='mt-2'>22 present · 1 late · 1 absent</p>"
        );
      }
      if (section === "results") {
        return panel(
          "Class results",
          table(["Student", "Math", "Science", "English"], [
            ["Alex Rivera", "88%", "92%", "85%"],
            ["Mia Chen", "91%", "90%", "93%"],
            ["Noah Patel", "84%", "86%", "80%"],
          ])
        );
      }
      return (
        welcome(session) +
        kpis([
          { label: "My students", value: "24", accent: "accent-sky" },
          { label: "Lessons today", value: "4", accent: "accent-mint" },
          { label: "To mark", value: "6", accent: "accent-coral" },
          { label: "Messages", value: "3", accent: "accent-royal" },
        ]) +
        '<div class="grid-2">' +
        panel(
          "Today",
          timeline([
            { time: "8:30", title: "Mathematics 4A", detail: "Room 12" },
            { time: "10:00", title: "Mathematics 5B", detail: "Room 12" },
            { time: "13:30", title: "Planning", detail: "Staff room" },
          ])
        ) +
        panel(
          "Quick actions",
          '<p><button type="button" class="btn-bsa btn-bsa-soft" data-section="attendance">Take attendance</button></p><p><button type="button" class="btn-bsa btn-bsa-soft" data-section="class">Open class list</button></p><p><button type="button" class="btn-bsa btn-bsa-soft" data-demo-action="publish">Post assignment</button></p>'
        ) +
        "</div>"
      );
    }

    if (role === "headmaster") {
      if (section === "staff") {
        return panel(
          "Staff",
          table(["Name", "Role", "Subject"], TEACHERS.filter(function (t) { return t.school === "Scuola Materna"; }).map(function (t) {
            return [t.name, "Teacher", t.subject];
          }))
        );
      }
      if (section === "students") {
        return panel(
          "Students",
          table(["Name", "Year", "Average"], STUDENTS.filter(function (s) { return s.school === "Scuola Materna"; }).map(function (s) {
            return [s.name, s.year, s.avg];
          }))
        );
      }
      if (section === "reports") {
        return panel(
          "Results",
          table(["Student", "Subject", "Mark"], RESULTS.filter(function (r) { return r.school === "Scuola Materna"; }).map(function (r) {
            return [r.student, r.subject, r.mark];
          }))
        );
      }
      return (
        '<div class="welcome-banner"><h2>School overview</h2><p>' +
        session.className +
        "</p></div>" +
        kpis([
          { label: "Students", value: "412", accent: "accent-mint" },
          { label: "Staff", value: "38", accent: "accent-sky" },
          { label: "Attendance", value: "94%", accent: "accent-royal" },
          { label: "Open tasks", value: "5", accent: "accent-coral" },
        ]) +
        panel("This week", "<p>Staff meeting — Wednesday</p><p>Inspection prep — ongoing</p><p>Sports day — Friday</p>")
      );
    }

    if (role === "admin") {
      if (section === "meetings") return meetingsPanel();
      if (section === "staff") {
        return panel("Teachers", table(["Name", "School", "Subject", "Class"], TEACHERS.map(function (t) {
          return [t.name, t.school, t.subject, t.className];
        })));
      }
      if (section === "students") {
        return studentsPanel(session);
      }
      if (section === "results") {
        return panel("Results", table(["Student", "School", "Subject", "Mark"], RESULTS.map(function (r) {
          return [r.student, r.school, r.subject, r.mark];
        })));
      }
      if (section === "settings") {
        return panel("School settings", "<p>Website banner, term dates and admissions notices (demo).</p><p><button type='button' class='btn-bsa btn-bsa-primary' data-demo-action='save'>Save (demo)</button></p>");
      }
      return (
        '<div class="welcome-banner"><h2>School admin</h2><p>' +
        session.className +
        "</p></div>" +
        kpis([
          { label: "Active staff", value: "38", accent: "accent-sky" },
          { label: "Students", value: String(allStudents().length), accent: "accent-mint" },
          { label: "Visit requests", value: String(loadVisits().length), accent: "accent-royal" },
          { label: "Pending invites", value: "2", accent: "accent-coral" },
        ]) +
        meetingsPanel() +
        panel("Recent activity", "<p>New teacher account created</p><p>Website banner updated</p><p>Admissions visits appear under Meetings</p>")
      );
    }

    if (section === "meetings") return meetingsPanel();
    if (section === "schools") {
      return panel("All schools", table(["School", "City", "Students", "Teachers", "Attendance"], SCHOOLS.map(function (s) {
        return [s.name, s.city, String(s.students), String(s.teachers), s.attendance];
      })));
    }
    if (section === "teachers") {
      return panel("Teachers across schools", table(["Name", "School", "Subject", "Class"], TEACHERS.map(function (t) {
        return [t.name, t.school, t.subject, t.className];
      })));
    }
    if (section === "students") {
      return studentsPanel(session);
    }
    if (section === "results") {
      return panel("Results across schools", table(["Student", "School", "Subject", "Mark"], RESULTS.map(function (r) {
        return [r.student, r.school, r.subject, r.mark];
      })));
    }
    if (section === "admins") {
      return panel(
        "School admins",
        table(["Name", "School", "Email"], [
          ["School Administrator", "BrightFuture Academy", "admin@gmail.com"],
          ["Grace Okonkwo", "Scuola Materna", "grace.okonkwo@brightsteps.academy"],
        ])
      );
    }
    return (
      '<div class="welcome-banner"><h2>Platform control</h2><p>All schools, teachers, students and results on one desk.</p></div>' +
      kpis([
        { label: "Schools", value: String(SCHOOLS.length), accent: "accent-royal" },
        { label: "Teachers", value: String(TEACHERS.length), accent: "accent-sky" },
        { label: "Students", value: String(allStudents().length), accent: "accent-mint" },
        { label: "Results on file", value: String(RESULTS.length), accent: "accent-coral" },
      ]) +
      meetingsPanel() +
      panel("Schools", table(["School", "Students", "Teachers"], SCHOOLS.map(function (s) {
        return [s.name, String(s.students), String(s.teachers)];
      })))
    );
  }

  function render(session, section) {
    var navItems = NAV[session.role] || NAV.student;
    var navHtml = navItems
      .map(function (item) {
        var active = item.id === section ? " active" : "";
        return (
          '<li><a href="#" class="' +
          active +
          '" data-section="' +
          item.id +
          '"><span aria-hidden="true">' +
          item.icon +
          "</span> " +
          item.label +
          "</a></li>"
        );
      })
      .join("");

    var badge = document.getElementById("dashRoleBadge");
    var nameEl = document.getElementById("dashUserName");
    var navEl = document.getElementById("dashNav");
    var content = document.getElementById("dashContent");
    if (!content || !navEl) return;
    if (badge) badge.textContent = session.roleLabel;
    if (nameEl) nameEl.textContent = session.name;
    navEl.innerHTML = navHtml;
    content.innerHTML = contentFor(session, section);
    document.title = session.roleLabel + " · BrightSteps Academy";
  }

  function boot() {
    var session = auth.requireAuth();
    if (!session) return;

    var section = "home";
    render(session, section);

    var logout = document.getElementById("dashLogout");
    if (logout) {
      logout.addEventListener("click", function (e) {
        e.preventDefault();
        auth.logout();
      });
    }

    document.addEventListener("click", function (e) {
      var navLink = e.target.closest("[data-section]");
      if (navLink && (navLink.closest("#dashNav") || navLink.closest("#dashContent"))) {
        e.preventDefault();
        section = navLink.getAttribute("data-section");
        render(session, section);
        return;
      }
      var demoBtn = e.target.closest("[data-demo-action]");
      if (demoBtn && window.showToast) {
        window.showToast("Saved (demo) — no server connected.", "success");
      }
    });

    document.addEventListener("submit", function (e) {
      var form = e.target.closest("#addKidForm");
      if (!form) return;
      e.preventDefault();
      if (session.role !== "admin" && session.role !== "superadmin") return;
      var name = (form.querySelector('[name="name"]') || {}).value || "";
      var year = (form.querySelector('[name="year"]') || {}).value || "";
      var school = (form.querySelector('[name="school"]') || {}).value || session.className || "School";
      var email = (form.querySelector('[name="email"]') || {}).value || "";
      var password = (form.querySelector('[name="password"]') || {}).value || "Demo@12345";
      var created = auth.addStudentAccount
        ? auth.addStudentAccount({ name: name, year: year, email: email, password: password })
        : { ok: false, message: "Cannot add students." };
      if (!created.ok) {
        if (window.showToast) window.showToast(created.message, "error");
        return;
      }
      var kids = loadKids();
      kids.unshift({
        name: name.trim(),
        school: String(school).trim(),
        year: String(year).trim(),
        avg: "—",
        email: created.email,
      });
      saveKids(kids);
      if (window.showToast) {
        window.showToast("Added " + name.trim() + ". Login: " + created.email + " / " + created.password, "success");
      }
      render(session, "students");
    });
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", boot);
  } else {
    boot();
  }
})();
