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
      { icon: "📣", label: "Notices", id: "announcements" },
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
      { icon: "💬", label: "Feedback", id: "feedback" },
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
      { icon: "💵", label: "Fees", id: "fees" },
      { icon: "🏫", label: "Classrooms", id: "classrooms" },
      { icon: "📣", label: "Announce", id: "announce" },
      { icon: "💬", label: "Feedback", id: "feedback" },
      { icon: "📊", label: "Results", id: "results" },
      { icon: "⚙️", label: "Settings", id: "settings" },
    ],
    superadmin: [
      { icon: "🏠", label: "Platform", id: "home" },
      { icon: "📅", label: "Meetings", id: "meetings" },
      { icon: "🏫", label: "Schools", id: "schools" },
      { icon: "👩‍🏫", label: "Teachers", id: "teachers" },
      { icon: "🧒", label: "Students", id: "students" },
      { icon: "💵", label: "Fees", id: "fees" },
      { icon: "🏫", label: "Classrooms", id: "classrooms" },
      { icon: "📣", label: "Announce", id: "announce" },
      { icon: "💬", label: "Feedback", id: "feedback" },
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
    { id: "seed-sarah", name: "Sarah Wilson", school: "Scuola Materna", subject: "Mathematics", className: "Maple · 4A", salary: 85000, email: "sarah.wilson@brightsteps.academy" },
    { id: "seed-david", name: "David Chen", school: "Scuola Materna", subject: "Science", className: "Lab · 4B", salary: 82000 },
    { id: "seed-amina", name: "Amina Rahman", school: "Scuola Materna", subject: "English", className: "4A / 5A", salary: 80000 },
    { id: "seed-priya", name: "Priya Sharma", school: "BrightFuture Academy", subject: "Art", className: "Studio", salary: 78000 },
    { id: "seed-james", name: "James Okonkwo", school: "Maple Grove Primary", subject: "PE", className: "All years", salary: 76000 },
  ];
  var STUDENTS = [
    { id: "seed-alex", name: "Alex Rivera", school: "Scuola Materna", year: "Grade 4", avg: "88%", fee: 12000, email: "alex.rivera@student.brightsteps.academy" },
    { id: "seed-mia", name: "Mia Chen", school: "Scuola Materna", year: "Grade 4", avg: "91%", fee: 12000 },
    { id: "seed-noah", name: "Noah Patel", school: "Scuola Materna", year: "Grade 5", avg: "84%", fee: 13000 },
    { id: "seed-sofia", name: "Sofia Rossi", school: "BrightFuture Academy", year: "Grade 3", avg: "90%", fee: 11000 },
    { id: "seed-leo", name: "Leo Mensah", school: "Maple Grove Primary", year: "Grade 2", avg: "87%", fee: 10000 },
  ];
  var KIDS_KEY = "brightsteps-demo-kids";
  var STAFF_KEY = "brightsteps-demo-staff";
  var MONEY_KEY = "brightsteps-demo-money";
  var ROOMS_KEY = "brightsteps-demo-rooms";
  var ROOM_MAP_KEY = "brightsteps-demo-room-map";
  var ANNOUNCE_KEY = "brightsteps-demo-announce";
  var HOMEWORK_KEY = "brightsteps-demo-homework";
  var FEEDBACK_KEY = "brightsteps-demo-feedback";

  var DEFAULT_ROOMS = [
    "Grade 1",
    "Grade 2",
    "Grade 3",
    "Grade 4 · Maple",
    "Grade 5",
    "Art Studio",
    "PE All years",
  ];

  var SEED_ROOM_MAP = {
    "seed-alex": "Grade 4 · Maple",
    "seed-mia": "Grade 4 · Maple",
    "seed-noah": "Grade 5",
    "seed-sofia": "Grade 3",
    "seed-leo": "Grade 2",
    "seed-sarah": "Grade 4 · Maple",
    "seed-david": "Grade 5",
    "seed-amina": "Grade 4 · Maple",
    "seed-priya": "Art Studio",
    "seed-james": "PE All years",
  };

  function loadList(key) {
    try {
      var raw = localStorage.getItem(key);
      var list = raw ? JSON.parse(raw) : [];
      return Array.isArray(list) ? list : [];
    } catch (e) {
      return [];
    }
  }

  function saveList(key, list) {
    localStorage.setItem(key, JSON.stringify(list.slice(0, 200)));
  }

  function loadMap(key) {
    try {
      var raw = localStorage.getItem(key);
      var parsed = raw ? JSON.parse(raw) : {};
      return parsed && typeof parsed === "object" ? parsed : {};
    } catch (e) {
      return {};
    }
  }

  function saveMap(key, map) {
    localStorage.setItem(key, JSON.stringify(map));
  }

  function loadKids() {
    return loadList(KIDS_KEY);
  }

  function saveKids(list) {
    saveList(KIDS_KEY, list);
  }

  function loadStaff() {
    return loadList(STAFF_KEY);
  }

  function saveStaff(list) {
    saveList(STAFF_KEY, list);
  }

  function loadMoney() {
    try {
      var raw = localStorage.getItem(MONEY_KEY);
      var parsed = raw ? JSON.parse(raw) : {};
      return parsed && typeof parsed === "object" ? parsed : {};
    } catch (e) {
      return {};
    }
  }

  function saveMoney(map) {
    localStorage.setItem(MONEY_KEY, JSON.stringify(map));
  }

  function ensureRooms() {
    var rooms = loadList(ROOMS_KEY);
    if (!rooms.length) {
      saveList(ROOMS_KEY, DEFAULT_ROOMS.slice());
      return DEFAULT_ROOMS.slice();
    }
    return rooms;
  }

  function loadRooms() {
    return ensureRooms();
  }

  function saveRooms(rooms) {
    saveList(ROOMS_KEY, rooms);
  }

  function ensureRoomMap() {
    var map = loadMap(ROOM_MAP_KEY);
    if (!Object.keys(map).length) {
      map = {};
      Object.keys(SEED_ROOM_MAP).forEach(function (k) {
        map[k] = SEED_ROOM_MAP[k];
      });
      saveMap(ROOM_MAP_KEY, map);
    }
    return map;
  }

  function loadRoomMap() {
    return ensureRoomMap();
  }

  function saveRoomMap(map) {
    saveMap(ROOM_MAP_KEY, map);
  }

  function loadAnnouncements() {
    return loadList(ANNOUNCE_KEY);
  }

  function saveAnnouncements(list) {
    saveList(ANNOUNCE_KEY, list);
  }

  function loadHomework() {
    return loadList(HOMEWORK_KEY);
  }

  function saveHomework(list) {
    saveList(HOMEWORK_KEY, list);
  }

  function loadFeedback() {
    return loadList(FEEDBACK_KEY);
  }

  function saveFeedback(list) {
    saveList(FEEDBACK_KEY, list);
  }

  function parseAmount(value) {
    var n = Number(String(value == null ? "" : value).replace(/[^\d.]/g, ""));
    if (!isFinite(n) || n < 0) return 0;
    return Math.round(n);
  }

  function money(value) {
    return "Rs " + parseAmount(value).toLocaleString();
  }

  function amountFor(id, field, fallback) {
    var map = loadMoney();
    if (map[id] && map[id][field] != null && map[id][field] !== "") return parseAmount(map[id][field]);
    return parseAmount(fallback);
  }

  function setAmount(id, field, value) {
    var map = loadMoney();
    if (!map[id]) map[id] = {};
    map[id][field] = parseAmount(value);
    saveMoney(map);
  }

  function canManageRoster(session) {
    return session && (session.role === "admin" || session.role === "superadmin");
  }

  function isPersonRemoved(person) {
    var id = person.id || person.name;
    if (auth.isRemoved(id)) return true;
    if (person.email && auth.isRemoved(person.email)) return true;
    return false;
  }

  function personKeys(person) {
    var id = person.id || person.name;
    var keys = [id];
    if (person.email) keys.push(person.email);
    if (id === "seed-alex") {
      keys.push("student_demo", "alex.rivera@student.brightsteps.academy");
    }
    if (id === "seed-sarah") {
      keys.push("teacher_demo", "sarah.wilson@brightsteps.academy");
    }
    return keys;
  }

  function isStudentLocked(person) {
    return personKeys(person).some(function (k) {
      return auth.isLocked(k);
    });
  }

  function getClassroomForPerson(personId, fallback) {
    var map = loadRoomMap();
    if (map[personId]) return map[personId];
    return fallback || "";
  }

  function applyClassroom(person) {
    var id = person.id || person.name;
    person.classroom = getClassroomForPerson(id, person.year || person.className || "");
    return person;
  }

  function sessionClassroom(session) {
    if (session.personId) {
      return getClassroomForPerson(session.personId, session.className || "");
    }
    return session.className || "";
  }

  function parentLinkedClassroom() {
    return "Grade 4 · Maple";
  }

  function saveFeeFor(id, value) {
    setAmount(id, "fee", value);
    var kids = loadKids();
    var changed = false;
    kids.forEach(function (k) {
      if ((k.id || k.name) === id) {
        k.fee = parseAmount(value);
        changed = true;
      }
    });
    if (changed) saveKids(kids);
  }

  function saveSalaryFor(id, value) {
    setAmount(id, "salary", value);
    var staff = loadStaff();
    var changed = false;
    staff.forEach(function (t) {
      if ((t.id || t.name) === id) {
        t.salary = parseAmount(value);
        changed = true;
      }
    });
    if (changed) saveStaff(staff);
  }

  function allTeachers() {
    return TEACHERS.concat(loadStaff())
      .filter(function (t) {
        return !isPersonRemoved(t);
      })
      .map(function (t) {
        var copy = {};
        Object.keys(t).forEach(function (k) {
          copy[k] = t[k];
        });
        copy.salary = amountFor(t.id || t.name, "salary", t.salary || 0);
        return applyClassroom(copy);
      });
  }

  function allStudents() {
    return STUDENTS.concat(loadKids())
      .filter(function (s) {
        return !isPersonRemoved(s);
      })
      .map(function (s) {
        var copy = {};
        Object.keys(s).forEach(function (k) {
          copy[k] = s[k];
        });
        copy.fee = amountFor(s.id || s.name, "fee", s.fee || 0);
        return applyClassroom(copy);
      });
  }

  function roomSelectOptions(current) {
    return loadRooms()
      .map(function (room) {
        var selected = room === current ? " selected" : "";
        return (
          '<option value="' + escapeHtml(room) + '"' + selected + ">" + escapeHtml(room) + "</option>"
        );
      })
      .join("");
  }

  function inlineRoomSelect(personId, current) {
    return (
      '<select data-room-select="' +
      escapeHtml(personId) +
      '">' +
      roomSelectOptions(current) +
      '</select> <button type="button" class="btn-bsa btn-bsa-sm btn-bsa-soft" data-save-room="' +
      escapeHtml(personId) +
      '">Save</button>'
    );
  }

  function portalLockCell(person) {
    var locked = isStudentLocked(person);
    if (locked) {
      return (
        '<span class="text-muted small">Locked</span> ' +
        '<button type="button" class="btn-bsa btn-bsa-sm btn-bsa-soft" data-unlock-student="' +
        escapeHtml(person.id || person.name) +
        '" data-email="' +
        escapeHtml(person.email || "") +
        '">Unlock</button>'
      );
    }
    return (
      '<button type="button" class="btn-bsa btn-bsa-sm btn-bsa-primary" data-lock-student="' +
      escapeHtml(person.id || person.name) +
      '" data-email="' +
      escapeHtml(person.email || "") +
      '">Lock</button>'
    );
  }

  function removePersonBtn(kind, person) {
    return (
      '<button type="button" class="btn-bsa btn-bsa-sm btn-bsa-soft" data-remove-person data-kind="' +
      escapeHtml(kind) +
      '" data-id="' +
      escapeHtml(person.id || person.name) +
      '" data-email="' +
      escapeHtml(person.email || "") +
      '">Remove</button>'
    );
  }

  function moneyInput(kind, id, value) {
    return (
      '<div class="dash-money">' +
      '<input type="number" min="0" step="500" data-' +
      kind +
      '="' +
      escapeHtml(id) +
      '" value="' +
      parseAmount(value) +
      '" />' +
      '<button type="button" class="btn-bsa btn-bsa-sm btn-bsa-soft" data-save-' +
      kind +
      '="' +
      escapeHtml(id) +
      '">Save</button>' +
      "</div>"
    );
  }

  function addKidForm(schoolDefault) {
    var school = escapeHtml(schoolDefault || "BrightFuture Academy");
    return (
      '<form class="form-bsa" id="addKidForm" style="margin-bottom:1.25rem">' +
      "<p><strong>Add a child</strong> — set their monthly fee and they get a student login.</p>" +
      '<div class="form-row">' +
      '<label>Full name<input name="name" required maxlength="80" placeholder="e.g. Ayaan Khan" /></label>' +
      '<label>Year / class<input name="year" required maxlength="40" placeholder="e.g. Grade 1" value="Grade 1" /></label>' +
      "</div>" +
      '<div class="form-row">' +
      '<label>School<input name="school" required maxlength="80" value="' +
      school +
      '" /></label>' +
      '<label>Monthly fee (Rs)<input name="fee" type="number" min="0" step="500" value="10000" required /></label>' +
      "</div>" +
      '<div class="form-row">' +
      '<label>Student email (login)<input name="email" type="email" required placeholder="child@email.com" /></label>' +
      '<label>Temporary password<input name="password" value="Demo@12345" minlength="6" /></label>' +
      "</div>" +
      '<button type="submit" class="btn-bsa btn-bsa-primary">Add child</button>' +
      "</form>"
    );
  }

  function addTeacherForm(schoolDefault) {
    var school = escapeHtml(schoolDefault || "BrightFuture Academy");
    return (
      '<form class="form-bsa" id="addTeacherForm" style="margin-bottom:1.25rem">' +
      "<p><strong>Add a teacher</strong> — set their monthly salary and they get a teacher login.</p>" +
      '<div class="form-row">' +
      '<label>Full name<input name="name" required maxlength="80" placeholder="e.g. Sara Malik" /></label>' +
      '<label>Subject<input name="subject" required maxlength="40" placeholder="e.g. Mathematics" /></label>' +
      "</div>" +
      '<div class="form-row">' +
      '<label>School<input name="school" required maxlength="80" value="' +
      school +
      '" /></label>' +
      '<label>Class / role<input name="className" maxlength="40" placeholder="e.g. Grade 3 homeroom" /></label>' +
      "</div>" +
      '<div class="form-row">' +
      '<label>Monthly salary (Rs)<input name="salary" type="number" min="0" step="1000" value="75000" required /></label>' +
      '<label>Teacher email (login)<input name="email" type="email" required placeholder="teacher@email.com" /></label>' +
      "</div>" +
      '<label>Temporary password<input name="password" value="Demo@12345" minlength="6" /></label>' +
      '<button type="submit" class="btn-bsa btn-bsa-primary">Add teacher</button>' +
      "</form>"
    );
  }

  function studentsPanel(session) {
    var rows = allStudents().map(function (s) {
      var id = s.id || s.name;
      return [
        escapeHtml(s.name),
        escapeHtml(s.school),
        escapeHtml(s.year),
        inlineRoomSelect(id, s.classroom),
        moneyInput("fee", id, s.fee),
        portalLockCell(s),
        removePersonBtn("student", s),
      ];
    });
    return (
      addKidForm(session && session.className ? session.className : "BrightFuture Academy") +
      panel(
        "Students and monthly fees",
        table(["Name", "School", "Year", "Classroom", "Monthly fee", "Portal", "Actions"], rows)
      )
    );
  }

  function teachersPanel(session) {
    var rows = allTeachers().map(function (t) {
      var id = t.id || t.name;
      return [
        escapeHtml(t.name),
        escapeHtml(t.school),
        escapeHtml(t.subject),
        inlineRoomSelect(id, t.classroom),
        moneyInput("salary", id, t.salary),
        removePersonBtn("teacher", t),
      ];
    });
    return (
      addTeacherForm(session && session.className ? session.className : "BrightFuture Academy") +
      panel(
        "Teachers and monthly salary",
        table(["Name", "School", "Subject", "Classroom", "Monthly salary", "Actions"], rows)
      )
    );
  }

  function countPeopleInRoom(room, people) {
    return people.filter(function (p) {
      return p.classroom === room;
    }).length;
  }

  function classroomsPanel() {
    var rooms = loadRooms();
    var students = allStudents();
    var teachers = allTeachers();
    var roomRows = rooms.map(function (room) {
      return [
        escapeHtml(room),
        String(countPeopleInRoom(room, students)),
        String(countPeopleInRoom(room, teachers)),
      ];
    });

    var assignRows = students
      .concat(
        teachers.map(function (t) {
          return { id: t.id, name: t.name, classroom: t.classroom, kind: "teacher" };
        })
      )
      .map(function (p) {
        var id = p.id || p.name;
        var kind = p.kind || "student";
        return [
          escapeHtml(p.name),
          kind === "teacher" ? "Teacher" : "Student",
          '<select data-room-select="' +
            escapeHtml(id) +
            '">' +
            roomSelectOptions(p.classroom) +
            '</select> <button type="button" class="btn-bsa btn-bsa-sm btn-bsa-soft" data-assign-room data-person="' +
            escapeHtml(id) +
            '">Save assignment</button>',
        ];
      });

    return (
      '<form class="form-bsa" id="addRoomForm" style="margin-bottom:1.25rem">' +
      "<p><strong>Add a classroom</strong></p>" +
      '<label>New classroom name<input name="name" required maxlength="60" placeholder="e.g. Grade 6" /></label>' +
      '<button type="submit" class="btn-bsa btn-bsa-primary">Add classroom</button>' +
      "</form>" +
      panel("Classrooms overview", table(["Classroom", "Students", "Teachers"], roomRows)) +
      panel("Assign people to classrooms", table(["Name", "Role", "Assignment"], assignRows))
    );
  }

  function formatDate(iso) {
    if (!iso) return "";
    return String(iso).replace("T", " ").slice(0, 16);
  }

  function announceListHtml(items, showDelete) {
    if (!items.length) {
      return "<p class='text-muted'>No announcements yet.</p>";
    }
    return (
      "<ul class='announce-list'>" +
      items
        .map(function (a) {
          var deleteBtn = showDelete
            ? ' <button type="button" class="btn-bsa btn-bsa-sm btn-bsa-soft" data-delete-announce="' +
              escapeHtml(a.id) +
              '">Delete</button>'
            : "";
          return (
            "<li style='margin-bottom:1rem'>" +
            "<strong>" +
            escapeHtml(a.title) +
            "</strong>" +
            " <span class='text-muted small'>· " +
            escapeHtml(a.audience === "all" ? "Whole school" : a.audience) +
            " · " +
            escapeHtml(formatDate(a.createdAt)) +
            "</span>" +
            deleteBtn +
            "<p>" +
            escapeHtml(a.body) +
            "</p>" +
            "<p class='text-muted small'>Posted by " +
            escapeHtml(a.by || "Admin") +
            "</p></li>"
          );
        })
        .join("") +
      "</ul>"
    );
  }

  function filteredAnnouncements(audiences) {
    var set = {};
    audiences.forEach(function (a) {
      set[a] = true;
    });
    return loadAnnouncements()
      .filter(function (a) {
        if (a.audience === "all") return true;
        return !!set[a.audience];
      })
      .sort(function (a, b) {
        return String(b.createdAt || "").localeCompare(String(a.createdAt || ""));
      });
  }

  function announcePanel(session) {
    var rooms = loadRooms();
    var audienceOpts =
      '<option value="all">Whole school</option>' +
      rooms
        .map(function (r) {
          return '<option value="' + escapeHtml(r) + '">' + escapeHtml(r) + "</option>";
        })
        .join("");
    var list = loadAnnouncements().sort(function (a, b) {
      return String(b.createdAt || "").localeCompare(String(a.createdAt || ""));
    });
    return (
      '<form class="form-bsa" id="announceForm" style="margin-bottom:1.25rem">' +
      "<p><strong>Post an announcement</strong></p>" +
      '<label>Title<input name="title" required maxlength="120" placeholder="e.g. Sports day" /></label>' +
      '<label>Message<textarea name="body" required maxlength="800" rows="3" placeholder="Details for parents and students"></textarea></label>' +
      '<label>Audience<select name="audience">' +
      audienceOpts +
      "</select></label>" +
      '<button type="submit" class="btn-bsa btn-bsa-primary">Publish announcement</button>' +
      "</form>" +
      panel("School announcements", announceListHtml(list, true))
    );
  }

  function studentNoticesPanel(session) {
    var classroom = sessionClassroom(session);
    var items = filteredAnnouncements(["all", classroom]);
    return panel(
      "School notices · " + escapeHtml(classroom || "your class"),
      announceListHtml(items, false)
    );
  }

  function parentNoticesPanel() {
    var linked = parentLinkedClassroom();
    var items = filteredAnnouncements(["all", linked]);
    return panel(
      "Announcements for Alex · " + escapeHtml(linked),
      announceListHtml(items, false)
    );
  }

  function recentNoticesSnippet(session, limit) {
    var classroom = sessionClassroom(session);
    var items = filteredAnnouncements(["all", classroom]).slice(0, limit || 2);
    if (!items.length) return "<p class='text-muted small'>No notices yet.</p>";
    return items
      .map(function (a) {
        return (
          "<p><strong>" +
          escapeHtml(a.title) +
          "</strong> — " +
          escapeHtml(a.body).slice(0, 80) +
          (a.body.length > 80 ? "…" : "") +
          "</p>"
        );
      })
      .join("");
  }

  function homeworkListHtml(items, showDelete) {
    if (!items.length) {
      return "<p class='text-muted'>No homework posted yet.</p>";
    }
    return (
      "<ul>" +
      items
        .map(function (h) {
          var deleteBtn = showDelete
            ? ' <button type="button" class="btn-bsa btn-bsa-sm btn-bsa-soft" data-delete-homework="' +
              escapeHtml(h.id) +
              '">Delete</button>'
            : "";
          return (
            "<li style='margin-bottom:1rem'><strong>" +
            escapeHtml(h.title) +
            "</strong> · " +
            escapeHtml(h.classroom) +
            " · due " +
            escapeHtml(h.due || "TBC") +
            deleteBtn +
            "<p>" +
            escapeHtml(h.body) +
            "</p>" +
            "<p class='text-muted small'>By " +
            escapeHtml(h.teacher || "Teacher") +
            " · " +
            escapeHtml(formatDate(h.createdAt)) +
            "</p></li>"
          );
        })
        .join("") +
      "</ul>"
    );
  }

  function homeworkPanel(session) {
    var defaultRoom = sessionClassroom(session) || session.className || loadRooms()[0] || "";
    var rooms = loadRooms();
    var roomOpts = rooms
      .map(function (r) {
        var sel = r === defaultRoom ? " selected" : "";
        return '<option value="' + escapeHtml(r) + '"' + sel + ">" + escapeHtml(r) + "</option>";
      })
      .join("");
    var mine = loadHomework()
      .filter(function (h) {
        return h.teacherLogin === session.login;
      })
      .sort(function (a, b) {
        return String(b.createdAt || "").localeCompare(String(a.createdAt || ""));
      });
    return (
      '<form class="form-bsa" id="homeworkForm" style="margin-bottom:1.25rem">' +
      "<p><strong>Assign homework</strong></p>" +
      '<label>Title<input name="title" required maxlength="120" placeholder="e.g. Fractions worksheet" /></label>' +
      '<label>Instructions<textarea name="body" required maxlength="800" rows="3" placeholder="What students should complete"></textarea></label>' +
      '<div class="form-row">' +
      '<label>Due date<input name="due" maxlength="40" placeholder="e.g. Friday 14 March" /></label>' +
      '<label>Classroom<select name="classroom">' +
      roomOpts +
      "</select></label>" +
      "</div>" +
      '<button type="submit" class="btn-bsa btn-bsa-primary">Post homework</button>' +
      "</form>" +
      panel("Homework you posted", homeworkListHtml(mine, true))
    );
  }

  function studentHomeworkPanel(session) {
    var classroom = sessionClassroom(session);
    var items = loadHomework()
      .filter(function (h) {
        return h.classroom === classroom;
      })
      .sort(function (a, b) {
        return String(b.createdAt || "").localeCompare(String(a.createdAt || ""));
      });
    return panel(
      "Homework · " + escapeHtml(classroom || "your class"),
      homeworkListHtml(items, false)
    );
  }

  function feedbackPanel(session) {
    var isAdmin = session.role === "admin" || session.role === "superadmin";
    var form =
      '<form class="form-bsa" id="feedbackForm" style="margin-bottom:1.25rem">' +
      "<p><strong>Send feedback to the school</strong></p>" +
      '<label>Type<select name="kind"><option value="suggestion">Suggestion</option><option value="complaint">Complaint</option></select></label>' +
      '<label>Message<textarea name="body" required maxlength="800" rows="3" placeholder="Your feedback"></textarea></label>' +
      '<label><input type="checkbox" name="anonymous" /> Send anonymously</label>' +
      '<button type="submit" class="btn-bsa btn-bsa-primary">Submit feedback</button>' +
      "</form>";

    var all = loadFeedback().sort(function (a, b) {
      return String(b.createdAt || "").localeCompare(String(a.createdAt || ""));
    });

    var listTitle = isAdmin ? "All feedback" : "School feedback (transparency demo)";
    var rows = all.map(function (f) {
      return [
        escapeHtml(formatDate(f.createdAt)),
        escapeHtml(f.kind === "complaint" ? "Complaint" : "Suggestion"),
        escapeHtml(f.author || "Anonymous"),
        escapeHtml(f.role || ""),
        escapeHtml(f.body),
      ];
    });

    return form + panel(listTitle, table(["When", "Type", "From", "Role", "Message"], rows));
  }

  function feesPanel() {
    var rows = allStudents().map(function (s) {
      var id = s.id || s.name;
      return [escapeHtml(s.name), escapeHtml(s.year), escapeHtml(s.school), moneyInput("fee", id, s.fee)];
    });
    var total = allStudents().reduce(function (sum, s) {
      return sum + parseAmount(s.fee);
    }, 0);
    return (
      kpis([
        { label: "Students", value: String(allStudents().length), accent: "accent-mint" },
        { label: "Monthly fee total", value: money(total), accent: "accent-royal" },
        {
          label: "Teacher payroll",
          value: money(
            allTeachers().reduce(function (sum, t) {
              return sum + parseAmount(t.salary);
            }, 0)
          ),
          accent: "accent-sky",
        },
        { label: "Open visits", value: String(loadVisits().length), accent: "accent-coral" },
      ]) +
      panel("Fee of each student", table(["Student", "Year", "School", "Monthly fee"], rows))
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
    var head = headers.map(function (h) {
      return "<th>" + h + "</th>";
    }).join("");
    var body = rows
      .map(function (row) {
        return "<tr>" + row.map(function (cell) {
          return "<td>" + cell + "</td>";
        }).join("") + "</tr>";
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
        return studentHomeworkPanel(session);
      }
      if (section === "announcements") {
        return studentNoticesPanel(session);
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
      var pendingHw = loadHomework().filter(function (h) {
        return h.classroom === sessionClassroom(session);
      }).length;
      return (
        welcome(session) +
        kpis([
          { label: "Attendance", value: "96%", accent: "accent-mint" },
          { label: "Pending work", value: String(pendingHw), accent: "accent-sky" },
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
        panel(
          "Latest notices",
          recentNoticesSnippet(session, 3) +
            '<p><button type="button" class="btn-bsa btn-bsa-sm btn-bsa-soft" data-section="announcements">All notices</button></p>'
        ) +
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
        return panel(
          "Attendance",
          "<p>Alex Rivera — <strong>96%</strong> this term</p><p class='text-muted small'>2 late arrivals · 1 excused absence</p>"
        );
      }
      if (section === "announcements") {
        return parentNoticesPanel();
      }
      return (
        welcome(session, "Diary, attendance and announcements for your linked children.") +
        kpis([
          { label: "Alex's attendance", value: "96%", accent: "accent-mint" },
          {
            label: "Unread notices",
            value: String(filteredAnnouncements(["all", parentLinkedClassroom()]).length),
            accent: "accent-sky",
          },
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
        var myRoom = sessionClassroom(session);
        var classStudents = allStudents().filter(function (s) {
          return s.classroom === myRoom;
        });
        var classRows = classStudents.length
          ? classStudents.map(function (s) {
              return [escapeHtml(s.name), "96%", escapeHtml(s.avg || "—")];
            })
          : [["Alex Rivera", "96%", "88%"], ["Mia Chen", "98%", "91%"]];
        return panel(
          escapeHtml(myRoom || "My class") + " · " + classRows.length + " students",
          table(["Student", "Attendance", "Last mark"], classRows)
        );
      }
      if (section === "assignments") {
        return homeworkPanel(session);
      }
      if (section === "feedback") {
        return feedbackPanel(session);
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
          '<p><button type="button" class="btn-bsa btn-bsa-soft" data-section="attendance">Take attendance</button></p><p><button type="button" class="btn-bsa btn-bsa-soft" data-section="class">Open class list</button></p><p><button type="button" class="btn-bsa btn-bsa-soft" data-section="assignments">Post homework</button></p>'
        ) +
        "</div>"
      );
    }

    if (role === "headmaster") {
      if (section === "staff") {
        return panel(
          "Staff",
          table(
            ["Name", "Role", "Subject"],
            TEACHERS.filter(function (t) {
              return t.school === "Scuola Materna";
            }).map(function (t) {
              return [t.name, "Teacher", t.subject];
            })
          )
        );
      }
      if (section === "students") {
        return panel(
          "Students",
          table(
            ["Name", "Year", "Average"],
            STUDENTS.filter(function (s) {
              return s.school === "Scuola Materna";
            }).map(function (s) {
              return [s.name, s.year, s.avg];
            })
          )
        );
      }
      if (section === "reports") {
        return panel(
          "Results",
          table(
            ["Student", "Subject", "Mark"],
            RESULTS.filter(function (r) {
              return r.school === "Scuola Materna";
            }).map(function (r) {
              return [r.student, r.subject, r.mark];
            })
          )
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
      if (section === "staff") return teachersPanel(session);
      if (section === "students") return studentsPanel(session);
      if (section === "fees") return feesPanel();
      if (section === "classrooms") return classroomsPanel();
      if (section === "announce") return announcePanel(session);
      if (section === "feedback") return feedbackPanel(session);
      if (section === "results") {
        return panel(
          "Results",
          table(
            ["Student", "School", "Subject", "Mark"],
            RESULTS.map(function (r) {
              return [r.student, r.school, r.subject, r.mark];
            })
          )
        );
      }
      if (section === "settings") {
        return panel(
          "School settings",
          "<p>Website banner, term dates and admissions notices (demo).</p><p><button type='button' class='btn-bsa btn-bsa-primary' data-demo-action='save'>Save (demo)</button></p>"
        );
      }
      return (
        '<div class="welcome-banner"><h2>School admin</h2><p>' +
        session.className +
        "</p></div>" +
        kpis([
          { label: "Active staff", value: String(allTeachers().length), accent: "accent-sky" },
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
      return panel(
        "All schools",
        table(
          ["School", "City", "Students", "Teachers", "Attendance"],
          SCHOOLS.map(function (s) {
            return [s.name, s.city, String(s.students), String(s.teachers), s.attendance];
          })
        )
      );
    }
    if (section === "teachers") return teachersPanel(session);
    if (section === "students") return studentsPanel(session);
    if (section === "fees") return feesPanel();
    if (section === "classrooms") return classroomsPanel();
    if (section === "announce") return announcePanel(session);
    if (section === "feedback") return feedbackPanel(session);
    if (section === "results") {
      return panel(
        "Results across schools",
        table(
          ["Student", "School", "Subject", "Mark"],
          RESULTS.map(function (r) {
            return [r.student, r.school, r.subject, r.mark];
          })
        )
      );
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
        { label: "Teachers", value: String(allTeachers().length), accent: "accent-sky" },
        { label: "Students", value: String(allStudents().length), accent: "accent-mint" },
        { label: "Results on file", value: String(RESULTS.length), accent: "accent-coral" },
      ]) +
      meetingsPanel() +
      panel(
        "Schools",
        table(
          ["School", "Students", "Teachers"],
          SCHOOLS.map(function (s) {
            return [s.name, String(s.students), String(s.teachers)];
          })
        )
      )
    );
  }

  function render(session, section) {
    ensureRooms();
    ensureRoomMap();
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

  function findPersonById(id) {
    var student = STUDENTS.concat(loadKids()).find(function (s) {
      return (s.id || s.name) === id;
    });
    if (student) return student;
    return TEACHERS.concat(loadStaff()).find(function (t) {
      return (t.id || t.name) === id;
    });
  }

  function saveRoomForPerson(personId, room) {
    var map = loadRoomMap();
    map[personId] = room;
    saveRoomMap(map);
  }

  function removePerson(kind, id, email) {
    if (!window.confirm("Remove this " + kind + " from the school portal? They will not be able to sign in.")) {
      return;
    }
    var person = findPersonById(id) || { id: id, email: email };
    auth.markRemoved(personKeys(person));
    if (email) auth.deleteExtraUser(email);
    if (kind === "student") {
      var kids = loadKids().filter(function (k) {
        return (k.id || k.name) !== id && k.email !== email;
      });
      saveKids(kids);
    }
    if (kind === "teacher") {
      var staff = loadStaff().filter(function (t) {
        return (t.id || t.name) !== id && t.email !== email;
      });
      saveStaff(staff);
    }
    var map = loadRoomMap();
    delete map[id];
    saveRoomMap(map);
    if (window.showToast) window.showToast("Removed from portal.", "success");
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

      var lockBtn = e.target.closest("[data-lock-student]");
      if (lockBtn) {
        e.preventDefault();
        if (!canManageRoster(session)) return;
        var lockId = lockBtn.getAttribute("data-lock-student");
        var lockPerson = findPersonById(lockId) || { id: lockId, email: lockBtn.getAttribute("data-email") };
        auth.setLocked(personKeys(lockPerson), true);
        if (window.showToast) window.showToast("Student portal locked.", "success");
        render(session, section);
        return;
      }

      var unlockBtn = e.target.closest("[data-unlock-student]");
      if (unlockBtn) {
        e.preventDefault();
        if (!canManageRoster(session)) return;
        var unlockId = unlockBtn.getAttribute("data-unlock-student");
        var unlockPerson = findPersonById(unlockId) || { id: unlockId, email: unlockBtn.getAttribute("data-email") };
        auth.setLocked(personKeys(unlockPerson), false);
        if (window.showToast) window.showToast("Student portal unlocked.", "success");
        render(session, section);
        return;
      }

      var removeBtn = e.target.closest("[data-remove-person]");
      if (removeBtn) {
        e.preventDefault();
        if (!canManageRoster(session)) return;
        removePerson(
          removeBtn.getAttribute("data-kind"),
          removeBtn.getAttribute("data-id"),
          removeBtn.getAttribute("data-email")
        );
        render(session, section);
        return;
      }

      var saveRoomBtn = e.target.closest("[data-save-room]");
      if (saveRoomBtn) {
        e.preventDefault();
        if (!canManageRoster(session)) return;
        var personId = saveRoomBtn.getAttribute("data-save-room");
        var row = saveRoomBtn.closest("tr");
        var select = row && row.querySelector('[data-room-select="' + personId + '"]');
        if (select) {
          saveRoomForPerson(personId, select.value);
          if (window.showToast) window.showToast("Classroom saved.", "success");
          render(session, section);
        }
        return;
      }

      var assignRoomBtn = e.target.closest("[data-assign-room]");
      if (assignRoomBtn) {
        e.preventDefault();
        if (!canManageRoster(session)) return;
        var assignId = assignRoomBtn.getAttribute("data-person");
        var assignRow = assignRoomBtn.closest("tr");
        var assignSelect = assignRow && assignRow.querySelector('[data-room-select="' + assignId + '"]');
        if (assignSelect) {
          saveRoomForPerson(assignId, assignSelect.value);
          if (window.showToast) window.showToast("Assignment saved.", "success");
          render(session, section);
        }
        return;
      }

      var deleteAnnounceBtn = e.target.closest("[data-delete-announce]");
      if (deleteAnnounceBtn) {
        e.preventDefault();
        if (!canManageRoster(session)) return;
        var announceId = deleteAnnounceBtn.getAttribute("data-delete-announce");
        saveAnnouncements(
          loadAnnouncements().filter(function (a) {
            return a.id !== announceId;
          })
        );
        if (window.showToast) window.showToast("Announcement deleted.", "success");
        render(session, section);
        return;
      }

      var deleteHomeworkBtn = e.target.closest("[data-delete-homework]");
      if (deleteHomeworkBtn) {
        e.preventDefault();
        var hwId = deleteHomeworkBtn.getAttribute("data-delete-homework");
        saveHomework(
          loadHomework().filter(function (h) {
            return h.id !== hwId;
          })
        );
        if (window.showToast) window.showToast("Homework deleted.", "success");
        render(session, section);
        return;
      }

      var saveFeeBtn = e.target.closest("[data-save-fee]");
      if (saveFeeBtn) {
        e.preventDefault();
        if (!canManageRoster(session)) return;
        var feeRow = saveFeeBtn.closest("tr");
        var feeInput = feeRow && feeRow.querySelector("[data-fee]");
        var feeId = saveFeeBtn.getAttribute("data-save-fee");
        saveFeeFor(feeId, feeInput ? feeInput.value : 0);
        if (window.showToast) window.showToast("Monthly fee saved: " + money(feeInput && feeInput.value), "success");
        render(session, section);
        return;
      }
      var saveSalaryBtn = e.target.closest("[data-save-salary]");
      if (saveSalaryBtn) {
        e.preventDefault();
        if (!canManageRoster(session)) return;
        var salaryRow = saveSalaryBtn.closest("tr");
        var salaryInput = salaryRow && salaryRow.querySelector("[data-salary]");
        var salaryId = saveSalaryBtn.getAttribute("data-save-salary");
        saveSalaryFor(salaryId, salaryInput ? salaryInput.value : 0);
        if (window.showToast) window.showToast("Monthly salary saved: " + money(salaryInput && salaryInput.value), "success");
        render(session, section);
        return;
      }
      var demoBtn = e.target.closest("[data-demo-action]");
      if (demoBtn && window.showToast) {
        window.showToast("Saved (demo) — no server connected.", "success");
      }
    });

    document.addEventListener("submit", function (e) {
      var roomForm = e.target.closest("#addRoomForm");
      if (roomForm) {
        e.preventDefault();
        if (!canManageRoster(session)) return;
        var roomName = String((roomForm.querySelector('[name="name"]') || {}).value || "").trim();
        if (!roomName) return;
        var rooms = loadRooms();
        if (rooms.indexOf(roomName) === -1) {
          rooms.push(roomName);
          saveRooms(rooms);
          if (window.showToast) window.showToast("Classroom added.", "success");
        }
        render(session, "classrooms");
        return;
      }

      var announceForm = e.target.closest("#announceForm");
      if (announceForm) {
        e.preventDefault();
        if (!canManageRoster(session)) return;
        var aTitle = String((announceForm.querySelector('[name="title"]') || {}).value || "").trim();
        var aBody = String((announceForm.querySelector('[name="body"]') || {}).value || "").trim();
        var aAudience = (announceForm.querySelector('[name="audience"]') || {}).value || "all";
        if (!aTitle || !aBody) return;
        var announcements = loadAnnouncements();
        announcements.unshift({
          id: "ann-" + Date.now(),
          title: aTitle,
          body: aBody,
          audience: aAudience,
          by: session.name,
          createdAt: new Date().toISOString(),
        });
        saveAnnouncements(announcements);
        if (window.showToast) window.showToast("Announcement published.", "success");
        render(session, "announce");
        return;
      }

      var homeworkForm = e.target.closest("#homeworkForm");
      if (homeworkForm) {
        e.preventDefault();
        var hTitle = String((homeworkForm.querySelector('[name="title"]') || {}).value || "").trim();
        var hBody = String((homeworkForm.querySelector('[name="body"]') || {}).value || "").trim();
        var hDue = String((homeworkForm.querySelector('[name="due"]') || {}).value || "").trim();
        var hClassroom = (homeworkForm.querySelector('[name="classroom"]') || {}).value || sessionClassroom(session);
        if (!hTitle || !hBody) return;
        var homework = loadHomework();
        homework.unshift({
          id: "hw-" + Date.now(),
          title: hTitle,
          body: hBody,
          classroom: hClassroom,
          due: hDue,
          teacher: session.name,
          teacherLogin: session.login,
          createdAt: new Date().toISOString(),
        });
        saveHomework(homework);
        if (window.showToast) window.showToast("Homework posted.", "success");
        render(session, "assignments");
        return;
      }

      var feedbackForm = e.target.closest("#feedbackForm");
      if (feedbackForm) {
        e.preventDefault();
        var fKind = (feedbackForm.querySelector('[name="kind"]') || {}).value || "suggestion";
        var fBody = String((feedbackForm.querySelector('[name="body"]') || {}).value || "").trim();
        var fAnon = !!(feedbackForm.querySelector('[name="anonymous"]') || {}).checked;
        if (!fBody) return;
        var feedback = loadFeedback();
        feedback.unshift({
          id: "fb-" + Date.now(),
          kind: fKind === "complaint" ? "complaint" : "suggestion",
          body: fBody,
          anonymous: fAnon,
          author: fAnon ? "Anonymous" : session.name,
          login: fAnon ? "" : session.login,
          role: session.roleLabel || session.role,
          createdAt: new Date().toISOString(),
        });
        saveFeedback(feedback);
        if (window.showToast) window.showToast("Feedback submitted.", "success");
        render(session, "feedback");
        return;
      }

      var teacherForm = e.target.closest("#addTeacherForm");
      if (teacherForm) {
        e.preventDefault();
        if (!canManageRoster(session)) return;
        var tName = (teacherForm.querySelector('[name="name"]') || {}).value || "";
        var tSubject = (teacherForm.querySelector('[name="subject"]') || {}).value || "";
        var tSchool = (teacherForm.querySelector('[name="school"]') || {}).value || session.className || "School";
        var tClass = (teacherForm.querySelector('[name="className"]') || {}).value || tSubject;
        var tSalary = parseAmount((teacherForm.querySelector('[name="salary"]') || {}).value);
        var tEmail = (teacherForm.querySelector('[name="email"]') || {}).value || "";
        var tPassword = (teacherForm.querySelector('[name="password"]') || {}).value || "Demo@12345";
        var tCreated = auth.addTeacherAccount
          ? auth.addTeacherAccount({ name: tName, subject: tSubject, className: tClass, email: tEmail, password: tPassword })
          : { ok: false, message: "Cannot add teachers." };
        if (!tCreated.ok) {
          if (window.showToast) window.showToast(tCreated.message, "error");
          return;
        }
        var staff = loadStaff();
        var teacherId = "t-" + Date.now();
        staff.unshift({
          id: teacherId,
          name: tName.trim(),
          school: String(tSchool).trim(),
          subject: String(tSubject).trim(),
          className: String(tClass).trim() || String(tSubject).trim(),
          salary: tSalary,
          email: tCreated.email,
        });
        saveStaff(staff);
        saveSalaryFor(teacherId, tSalary);
        if (window.showToast) {
          window.showToast("Added " + tName.trim() + ". Login: " + tCreated.email + " / " + tCreated.password, "success");
        }
        render(session, session.role === "superadmin" ? "teachers" : "staff");
        return;
      }

      var form = e.target.closest("#addKidForm");
      if (!form) return;
      e.preventDefault();
      if (!canManageRoster(session)) return;
      var name = (form.querySelector('[name="name"]') || {}).value || "";
      var year = (form.querySelector('[name="year"]') || {}).value || "";
      var school = (form.querySelector('[name="school"]') || {}).value || session.className || "School";
      var fee = parseAmount((form.querySelector('[name="fee"]') || {}).value);
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
      var kidId = "s-" + Date.now();
      kids.unshift({
        id: kidId,
        name: name.trim(),
        school: String(school).trim(),
        year: String(year).trim(),
        avg: "—",
        fee: fee,
        email: created.email,
      });
      saveKids(kids);
      saveFeeFor(kidId, fee);
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
