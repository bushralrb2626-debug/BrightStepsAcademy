/**
 * BrightSteps static demo — client-side login / registration (no backend).
 */
(function () {
  "use strict";

  var STORAGE_KEY = "brightsteps-demo-session";
  var USERS_KEY = "brightsteps-demo-users";
  var LOGIN_PATH = "/demos/brightsteps/login.html";
  var REGISTER_PATH = "/demos/brightsteps/register.html";
  var DASHBOARD_PATH = "/demos/brightsteps/dashboard.html";
  var DEMO_PASSWORD = "Demo@12345";

  var BUILTIN = {
    student_demo: {
      password: DEMO_PASSWORD,
      role: "student",
      name: "Alex Rivera",
      roleLabel: "Student",
      className: "Grade 4 · Maple Class",
    },
    "alex.rivera@student.brightsteps.academy": null,
    parent_demo: {
      password: DEMO_PASSWORD,
      role: "parent",
      name: "Amelia Johnson",
      roleLabel: "Parent / Guardian",
      className: "Linked child: Alex Rivera",
    },
    "amelia.johnson@email.com": null,
    teacher_demo: {
      password: DEMO_PASSWORD,
      role: "teacher",
      name: "Sarah Wilson",
      roleLabel: "Teacher",
      className: "Maple Class · Homeroom",
    },
    "sarah.wilson@brightsteps.academy": null,
    "grace.okonkwo@brightsteps.academy": {
      password: DEMO_PASSWORD,
      role: "headmaster",
      name: "Grace Okonkwo",
      roleLabel: "Headmaster",
      className: "BrightSteps Academy",
    },
    "admin@gmail.com": {
      password: "123456",
      role: "admin",
      name: "School Administrator",
      roleLabel: "School Admin",
      className: "BrightFuture Academy",
    },
    "superadmin@gmail.com": {
      password: "12345",
      role: "superadmin",
      name: "Platform Super Admin",
      roleLabel: "Super Admin",
      className: "All schools",
    },
  };

  BUILTIN["alex.rivera@student.brightsteps.academy"] = BUILTIN.student_demo;
  BUILTIN["amelia.johnson@email.com"] = BUILTIN.parent_demo;
  BUILTIN["sarah.wilson@brightsteps.academy"] = BUILTIN.teacher_demo;
  BUILTIN["admin@brightfuture.academy"] = BUILTIN["admin@gmail.com"];
  BUILTIN["superadmin@platform.com"] = BUILTIN["superadmin@gmail.com"];

  function normalizeLogin(value) {
    return String(value || "").trim().toLowerCase();
  }

  function extraUsers() {
    try {
      var raw = localStorage.getItem(USERS_KEY);
      var parsed = raw ? JSON.parse(raw) : {};
      return parsed && typeof parsed === "object" ? parsed : {};
    } catch (e) {
      return {};
    }
  }

  function saveExtra(map) {
    localStorage.setItem(USERS_KEY, JSON.stringify(map));
  }

  function lookup(loginId) {
    var key = normalizeLogin(loginId);
    if (BUILTIN[key]) return { key: key, user: BUILTIN[key] };
    var extra = extraUsers();
    if (extra[key]) return { key: key, user: extra[key] };
    return null;
  }

  function isPublicDemoAccount(loginId) {
    var found = lookup(loginId);
    if (!found || !found.user) return false;
    return found.user.role === "student" || found.user.role === "parent" || found.user.role === "teacher";
  }

  function readSession() {
    try {
      var raw = sessionStorage.getItem(STORAGE_KEY) || localStorage.getItem(STORAGE_KEY);
      return raw ? JSON.parse(raw) : null;
    } catch (e) {
      return null;
    }
  }

  function writeSession(session, remember) {
    var payload = JSON.stringify(session);
    sessionStorage.setItem(STORAGE_KEY, payload);
    if (remember) localStorage.setItem(STORAGE_KEY, payload);
    else localStorage.removeItem(STORAGE_KEY);
  }

  function clearSession() {
    sessionStorage.removeItem(STORAGE_KEY);
    localStorage.removeItem(STORAGE_KEY);
  }

  function toSession(key, user) {
    return {
      login: key,
      role: user.role,
      name: user.name,
      roleLabel: user.roleLabel,
      className: user.className,
      loggedInAt: Date.now(),
    };
  }

  function login(loginId, password, remember) {
    var found = lookup(loginId);
    if (!found || found.user.password !== password) {
      return { ok: false, message: "Invalid login ID or password." };
    }
    var session = toSession(found.key, found.user);
    writeSession(session, !!remember);
    return { ok: true, session: session };
  }

  function register(fields) {
    var email = normalizeLogin(fields.email);
    var name = String(fields.name || "").trim();
    var password = String(fields.password || "");
    var role = String(fields.role || "parent");
    var allowed = { student: 1, parent: 1, teacher: 1 };
    if (!allowed[role]) role = "parent";
    if (!name || name.length > 80) return { ok: false, message: "Please enter your name." };
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) return { ok: false, message: "Please enter a valid email." };
    if (password.length < 6) return { ok: false, message: "Password must be at least 6 characters." };
    if (lookup(email)) return { ok: false, message: "That email is already registered. Sign in instead." };

    var labels = { student: "Student", parent: "Parent / Guardian", teacher: "Teacher" };
    var classes = {
      student: "Grade 4 · Maple Class",
      parent: "Linked child pending",
      teacher: "Maple Class · Homeroom",
    };
    var extra = extraUsers();
    extra[email] = {
      password: password,
      role: role,
      name: name,
      roleLabel: labels[role],
      className: classes[role],
    };
    saveExtra(extra);
    var session = toSession(email, extra[email]);
    writeSession(session, true);
    return { ok: true, session: session };
  }

  function addStudentAccount(fields) {
    var email = normalizeLogin(fields.email);
    var name = String(fields.name || "").trim();
    var year = String(fields.year || "Grade 1").trim();
    var password = String(fields.password || DEMO_PASSWORD);
    if (!name || name.length > 80) return { ok: false, message: "Enter the child's name." };
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) return { ok: false, message: "Enter a valid email for the student login." };
    if (lookup(email)) return { ok: false, message: "That email is already registered." };
    var extra = extraUsers();
    extra[email] = {
      password: password.length >= 6 ? password : DEMO_PASSWORD,
      role: "student",
      name: name,
      roleLabel: "Student",
      className: year,
    };
    saveExtra(extra);
    return { ok: true, email: email, password: extra[email].password };
  }

  function logout() {
    clearSession();
    window.location.href = LOGIN_PATH;
  }

  function requireAuth(allowedRoles) {
    var session = readSession();
    if (!session) {
      window.location.href = LOGIN_PATH;
      return null;
    }
    if (allowedRoles && allowedRoles.length && allowedRoles.indexOf(session.role) === -1) {
      window.location.href = DASHBOARD_PATH;
      return null;
    }
    return session;
  }

  window.BrightStepsDemoAuth = {
    login: login,
    register: register,
    addStudentAccount: addStudentAccount,
    logout: logout,
    getSession: readSession,
    requireAuth: requireAuth,
    isDemoAccount: isPublicDemoAccount,
    isPublicDemoAccount: isPublicDemoAccount,
    demoPassword: DEMO_PASSWORD,
    paths: { login: LOGIN_PATH, register: REGISTER_PATH, dashboard: DASHBOARD_PATH },
  };
})();
