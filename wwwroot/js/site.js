/**
 * BrightSteps Academy — site interactions (frontend-only demos)
 * No backend calls. Safe to load on public + dashboard pages.
 */
(function () {
  "use strict";

  /* --------------------------------------------------------------------------
     Toast system
     -------------------------------------------------------------------------- */
  function ensureToastHost() {
    var host = document.getElementById("toastHost");
    if (!host) {
      host = document.createElement("div");
      host.id = "toastHost";
      host.setAttribute("aria-live", "polite");
      host.setAttribute("aria-relevant", "additions");
      document.body.appendChild(host);
    }
    return host;
  }

  window.showToast = function (message, type) {
    var host = ensureToastHost();
    var toast = document.createElement("div");
    var kind = type || "info";
    toast.className = "toast-bsa " + kind;
    toast.setAttribute("role", "status");

    var msg = document.createElement("div");
    msg.className = "toast-msg";
    msg.textContent = message;

    var close = document.createElement("button");
    close.type = "button";
    close.className = "toast-close";
    close.setAttribute("aria-label", "Dismiss");
    close.innerHTML = "&times;";
    close.addEventListener("click", function () {
      toast.remove();
    });

    toast.appendChild(msg);
    toast.appendChild(close);
    host.appendChild(toast);

    window.setTimeout(function () {
      if (toast.parentNode) toast.remove();
    }, 4200);
  };

  /* --------------------------------------------------------------------------
     Modals — data-modal-open / data-modal-close
     -------------------------------------------------------------------------- */
  function openModal(id) {
    var modal = document.getElementById(id);
    if (!modal) return;
    modal.classList.add("open");
    document.body.classList.add("modal-open");
    var focusable = modal.querySelector("button, [href], input, select, textarea");
    if (focusable) focusable.focus();
  }

  function closeModal(modal) {
    if (!modal) return;
    modal.classList.remove("open");
    if (!document.querySelector(".modal-bsa.open")) {
      document.body.classList.remove("modal-open");
    }
  }

  document.addEventListener("click", function (e) {
    var openBtn = e.target.closest("[data-modal-open]");
    if (openBtn) {
      e.preventDefault();
      openModal(openBtn.getAttribute("data-modal-open"));
      return;
    }

    var closeBtn = e.target.closest("[data-modal-close]");
    if (closeBtn) {
      e.preventDefault();
      closeModal(closeBtn.closest(".modal-bsa"));
      return;
    }

    if (e.target.classList.contains("modal-bsa-backdrop")) {
      closeModal(e.target.closest(".modal-bsa"));
    }
  });

  document.addEventListener("keydown", function (e) {
    if (e.key === "Escape") {
      var open = document.querySelector(".modal-bsa.open");
      if (open) closeModal(open);
      var lb = document.getElementById("galleryLightbox");
      if (lb && lb.classList.contains("open")) closeLightbox();
    }
  });

  /* --------------------------------------------------------------------------
     Contact form
     -------------------------------------------------------------------------- */
  document.querySelectorAll("form.js-contact-form, form#contactForm").forEach(function (form) {
    form.addEventListener("submit", function (e) {
      e.preventDefault();
      window.showToast("Thanks! Your message was sent (demo).", "success");
      form.reset();
    });
  });

  /* --------------------------------------------------------------------------
     Gallery lightbox
     -------------------------------------------------------------------------- */
  var galleryItems = [];
  var galleryIndex = 0;

  function ensureLightbox() {
    var lb = document.getElementById("galleryLightbox");
    if (lb) return lb;

    lb = document.createElement("div");
    lb.id = "galleryLightbox";
    lb.className = "lightbox";
    lb.innerHTML =
      '<div class="lightbox-inner">' +
      '<button type="button" class="lightbox-close" aria-label="Close">&times;</button>' +
      '<button type="button" class="lightbox-nav lightbox-prev" aria-label="Previous">‹</button>' +
      '<img src="" alt="" />' +
      '<button type="button" class="lightbox-nav lightbox-next" aria-label="Next">›</button>' +
      "</div>";
    document.body.appendChild(lb);

    lb.querySelector(".lightbox-close").addEventListener("click", closeLightbox);
    lb.addEventListener("click", function (e) {
      if (e.target === lb) closeLightbox();
    });
    lb.querySelector(".lightbox-prev").addEventListener("click", function (e) {
      e.stopPropagation();
      showGallery(galleryIndex - 1);
    });
    lb.querySelector(".lightbox-next").addEventListener("click", function (e) {
      e.stopPropagation();
      showGallery(galleryIndex + 1);
    });
    return lb;
  }

  function showGallery(index) {
    if (!galleryItems.length) return;
    galleryIndex = (index + galleryItems.length) % galleryItems.length;
    var lb = ensureLightbox();
    var img = lb.querySelector("img");
    var src = galleryItems[galleryIndex].getAttribute("data-full") ||
      galleryItems[galleryIndex].querySelector("img")?.src ||
      galleryItems[galleryIndex].src;
    var alt = galleryItems[galleryIndex].querySelector("img")?.alt || "Gallery image";
    img.src = src;
    img.alt = alt;
    lb.classList.add("open");
    document.body.classList.add("modal-open");
  }

  function closeLightbox() {
    var lb = document.getElementById("galleryLightbox");
    if (!lb) return;
    lb.classList.remove("open");
    if (!document.querySelector(".modal-bsa.open")) {
      document.body.classList.remove("modal-open");
    }
  }

  function initGallery() {
    galleryItems = Array.from(document.querySelectorAll(".gallery-item, .gallery-masonry [data-full]"));
    galleryItems.forEach(function (item, i) {
      item.addEventListener("click", function (e) {
        e.preventDefault();
        showGallery(i);
      });
    });
  }

  /* --------------------------------------------------------------------------
     Animated counters
     -------------------------------------------------------------------------- */
  function animateCounter(el) {
    if (el.dataset.counted === "1") return;
    el.dataset.counted = "1";
    var target = parseFloat(el.getAttribute("data-target") || el.textContent.replace(/[^\d.]/g, "")) || 0;
    var suffix = el.getAttribute("data-suffix") || "";
    var prefix = el.getAttribute("data-prefix") || "";
    var duration = parseInt(el.getAttribute("data-duration") || "1200", 10);
    var start = performance.now();
    var isInt = Number.isInteger(target);

    function frame(now) {
      var t = Math.min(1, (now - start) / duration);
      var eased = 1 - Math.pow(1 - t, 3);
      var value = target * eased;
      el.textContent = prefix + (isInt ? Math.round(value) : value.toFixed(1)) + suffix;
      if (t < 1) requestAnimationFrame(frame);
    }
    requestAnimationFrame(frame);
  }

  function initCounters() {
    var counters = document.querySelectorAll(".counter[data-target], [data-counter]");
    if (!counters.length || !("IntersectionObserver" in window)) {
      counters.forEach(animateCounter);
      return;
    }
    var io = new IntersectionObserver(
      function (entries) {
        entries.forEach(function (entry) {
          if (entry.isIntersecting) {
            animateCounter(entry.target);
            io.unobserve(entry.target);
          }
        });
      },
      { threshold: 0.35 }
    );
    counters.forEach(function (c) {
      io.observe(c);
    });
  }

  /* --------------------------------------------------------------------------
     Reveal on scroll
     -------------------------------------------------------------------------- */
  function initReveal() {
    var nodes = document.querySelectorAll(".reveal, .stagger");
    if (!nodes.length) return;

    function show(el) {
      el.classList.add("visible", "is-visible", "in");
      if (el.classList.contains("stagger")) {
        el.querySelectorAll(".reveal").forEach(function (child, i) {
          window.setTimeout(function () {
            child.classList.add("visible", "is-visible", "in");
          }, 70 * i);
        });
      }
    }

    if (!("IntersectionObserver" in window) || window.matchMedia("(prefers-reduced-motion: reduce)").matches) {
      nodes.forEach(show);
      return;
    }

    var io = new IntersectionObserver(
      function (entries) {
        entries.forEach(function (entry) {
          if (entry.isIntersecting) {
            show(entry.target);
            io.unobserve(entry.target);
          }
        });
      },
      { threshold: 0.1, rootMargin: "0px 0px -36px 0px" }
    );

    nodes.forEach(function (n) {
      io.observe(n);
    });
  }

  /* --------------------------------------------------------------------------
     Public nav drawer
     -------------------------------------------------------------------------- */
  function initPublicNav() {
    var toggle = document.getElementById("navToggle") || document.querySelector(".nav-toggle");
    var drawer = document.getElementById("navDrawer") || document.querySelector(".nav-drawer");
    var backdrop = document.getElementById("navDrawerBackdrop") || document.querySelector(".nav-drawer-backdrop");
    if (!toggle || !drawer) return;

    function setOpen(open) {
      drawer.classList.toggle("open", open);
      if (backdrop) backdrop.classList.toggle("open", open);
      document.body.classList.toggle("nav-open", open);
      toggle.setAttribute("aria-expanded", open ? "true" : "false");
    }

    toggle.addEventListener("click", function () {
      setOpen(!drawer.classList.contains("open"));
    });
    if (backdrop) {
      backdrop.addEventListener("click", function () {
        setOpen(false);
      });
    }
    drawer.querySelectorAll("a").forEach(function (a) {
      a.addEventListener("click", function () {
        setOpen(false);
      });
    });
  }

  /* --------------------------------------------------------------------------
     Dashboard sidebar
     -------------------------------------------------------------------------- */
  function initSidebar() {
    var toggle = document.getElementById("sidebarToggle");
    var sidebar = document.querySelector(".dash-sidebar");
    var backdrop = document.querySelector(".sidebar-backdrop");
    if (!sidebar) return;

    function setOpen(open) {
      document.body.classList.toggle("sidebar-open", open);
      sidebar.classList.toggle("open", open);
      if (backdrop) backdrop.classList.toggle("open", open);
      if (toggle) toggle.setAttribute("aria-expanded", open ? "true" : "false");
    }

    document.querySelectorAll("[data-dash-toggle], #sidebarToggle").forEach(function (btn) {
      btn.addEventListener("click", function () {
        setOpen(!document.body.classList.contains("sidebar-open"));
      });
    });
    if (backdrop) {
      backdrop.addEventListener("click", function () {
        setOpen(false);
      });
    }
  }

  /* --------------------------------------------------------------------------
     Notification / message dropdowns
     -------------------------------------------------------------------------- */
  function initDropdowns() {
    document.querySelectorAll("[data-dropdown]").forEach(function (btn) {
      var id = btn.getAttribute("data-dropdown");
      var panel = document.getElementById(id);
      if (!panel) return;

      btn.addEventListener("click", function (e) {
        e.stopPropagation();
        var willOpen = !panel.classList.contains("open");
        document.querySelectorAll(".dropdown-panel.open").forEach(function (p) {
          p.classList.remove("open");
        });
        panel.classList.toggle("open", willOpen);
      });
    });

    document.addEventListener("click", function () {
      document.querySelectorAll(".dropdown-panel.open").forEach(function (p) {
        p.classList.remove("open");
      });
    });
  }

  /* --------------------------------------------------------------------------
     Client-side table filter
     -------------------------------------------------------------------------- */
  function initTableFilters() {
    document.querySelectorAll(".js-filter-table").forEach(function (input) {
      var tableSel = input.getAttribute("data-table") || ".table-bsa";
      var table = document.querySelector(tableSel);
      if (!table) return;
      var tbody = table.tBodies[0];
      if (!tbody) return;

      input.addEventListener("input", function () {
        var q = input.value.trim().toLowerCase();
        Array.from(tbody.rows).forEach(function (row) {
          var text = row.textContent.toLowerCase();
          row.style.display = !q || text.indexOf(q) !== -1 ? "" : "none";
        });
      });
    });

    document.querySelectorAll("select.js-filter-select").forEach(function (sel) {
      var tableSel = sel.getAttribute("data-table") || ".table-bsa";
      var col = parseInt(sel.getAttribute("data-col") || "0", 10);
      var table = document.querySelector(tableSel);
      if (!table || !table.tBodies[0]) return;

      sel.addEventListener("change", function () {
        var val = sel.value.toLowerCase();
        Array.from(table.tBodies[0].rows).forEach(function (row) {
          if (!val) {
            row.style.display = "";
            return;
          }
          var cell = row.cells[col];
          var text = (cell ? cell.textContent : "").toLowerCase();
          row.style.display = text.indexOf(val) !== -1 ? "" : "none";
        });
      });
    });
  }

  /* --------------------------------------------------------------------------
     Attendance mark toggle
     -------------------------------------------------------------------------- */
  function initAttendance() {
    document.querySelectorAll(".attendance-toggle").forEach(function (group) {
      group.querySelectorAll("button[data-status]").forEach(function (btn) {
        btn.addEventListener("click", function () {
          group.querySelectorAll("button").forEach(function (b) {
            b.classList.remove("active");
          });
          btn.classList.add("active");
          var row = group.closest("tr");
          if (row) {
            var badge = row.querySelector(".js-attendance-badge");
            if (badge) {
              badge.textContent = btn.getAttribute("data-status");
              badge.className =
                "badge-soft js-attendance-badge badge-" +
                btn.getAttribute("data-status").toLowerCase();
            }
          }
          window.showToast("Marked " + btn.getAttribute("data-status"), "success");
        });
      });
    });
  }

  /* --------------------------------------------------------------------------
     Assignment publish
     -------------------------------------------------------------------------- */
  function initAssignmentPublish() {
    document.querySelectorAll("[data-publish-assignment]").forEach(function (btn) {
      btn.addEventListener("click", function () {
        window.showToast("Assignment published (demo).", "success");
        var modal = btn.closest(".modal-bsa");
        if (modal) closeModal(modal);
      });
    });
  }

  /* --------------------------------------------------------------------------
     Quick actions
     -------------------------------------------------------------------------- */
  function initQuickActions() {
    document.querySelectorAll("[data-quick-action]").forEach(function (btn) {
      btn.addEventListener("click", function (e) {
        var action = btn.getAttribute("data-quick-action");
        var modalId = btn.getAttribute("data-modal-open");
        if (modalId) return;
        e.preventDefault();
        window.showToast("Quick action: " + (action || "done") + " (demo)", "info");
      });
    });
  }

  /* --------------------------------------------------------------------------
     Pagination demo
     -------------------------------------------------------------------------- */
  function initPagination() {
    document.querySelectorAll(".pagination-bsa").forEach(function (nav) {
      nav.querySelectorAll("button, .page-btn").forEach(function (btn) {
        btn.addEventListener("click", function () {
          if (btn.disabled || btn.classList.contains("disabled")) return;
          nav.querySelectorAll("button, .page-btn").forEach(function (b) {
            b.classList.remove("active");
          });
          if (!btn.classList.contains("page-prev") && !btn.classList.contains("page-next")) {
            btn.classList.add("active");
          } else {
            var pages = Array.from(nav.querySelectorAll("button:not(.page-prev):not(.page-next), .page-btn:not(.page-prev):not(.page-next)"));
            var current = pages.findIndex(function (p) {
              return p.classList.contains("active");
            });
            if (current < 0) current = 0;
            pages.forEach(function (p) {
              p.classList.remove("active");
            });
            var next = btn.classList.contains("page-next")
              ? Math.min(pages.length - 1, current + 1)
              : Math.max(0, current - 1);
            if (pages[next]) pages[next].classList.add("active");
          }
          window.showToast("Page changed (demo)", "info");
        });
      });
    });
  }

  /* --------------------------------------------------------------------------
     Timetable subject color mapping
     -------------------------------------------------------------------------- */
  var SUBJECT_MAP = {
    math: "math",
    mathematics: "math",
    english: "english",
    science: "science",
    art: "art",
    "p.e.": "pe",
    pe: "pe",
    "physical education": "pe",
    music: "music",
    history: "history",
    free: "free",
    break: "free",
  };

  function initTimetable() {
    document.querySelectorAll(".tt-cell[data-subject], .tt-cell").forEach(function (cell) {
      var subject = (cell.getAttribute("data-subject") || cell.textContent || "").trim().toLowerCase();
      var key = SUBJECT_MAP[subject];
      if (key) cell.classList.add(key);
    });
  }

  /* --------------------------------------------------------------------------
     Chat thread switcher
     -------------------------------------------------------------------------- */
  function initChat() {
    var threads = document.querySelectorAll(".msg-thread[data-thread]");
    if (!threads.length) return;

    threads.forEach(function (thread) {
      thread.addEventListener("click", function () {
        threads.forEach(function (t) {
          t.classList.remove("active");
        });
        thread.classList.add("active");

        var id = thread.getAttribute("data-thread");
        document.querySelectorAll(".msg-pane-body[data-thread-panel]").forEach(function (panel) {
          panel.hidden = panel.getAttribute("data-thread-panel") !== id;
        });

        var name = thread.querySelector("strong");
        var headerName = document.querySelector(".msg-pane-header .js-thread-name");
        if (name && headerName) headerName.textContent = name.textContent;
      });
    });

    var compose = document.querySelector(".msg-compose");
    if (compose) {
      var input = compose.querySelector("input");
      var send = compose.querySelector("button");
      function sendMsg() {
        if (!input || !input.value.trim()) return;
        var activePanel = document.querySelector(".msg-pane-body[data-thread-panel]:not([hidden])");
        if (!activePanel) activePanel = document.querySelector(".msg-pane-body");
        if (activePanel) {
          var bubble = document.createElement("div");
          bubble.className = "msg-bubble mine";
          bubble.textContent = input.value.trim();
          activePanel.appendChild(bubble);
          activePanel.scrollTop = activePanel.scrollHeight;
        }
        input.value = "";
      }
      if (send) send.addEventListener("click", sendMsg);
      if (input) {
        input.addEventListener("keydown", function (e) {
          if (e.key === "Enter") {
            e.preventDefault();
            sendMsg();
          }
        });
      }
    }
  }

  /* --------------------------------------------------------------------------
     Charts — Chart.js if present, else CSS bar fallback
     -------------------------------------------------------------------------- */
  var SOFT_COLORS = [
    "rgba(91, 184, 232, 0.85)",
    "rgba(94, 201, 168, 0.85)",
    "rgba(246, 201, 69, 0.85)",
    "rgba(155, 126, 220, 0.85)",
    "rgba(240, 138, 60, 0.85)",
    "rgba(232, 121, 168, 0.85)",
    "rgba(107, 188, 91, 0.85)",
  ];

  function parseChartData(canvas) {
    try {
      var raw = canvas.getAttribute("data-chart");
      if (!raw) return null;
      return JSON.parse(raw);
    } catch (err) {
      return null;
    }
  }

  function renderCssFallback(canvas, data) {
    var wrap = canvas.parentElement;
    if (!wrap) return;
    canvas.style.display = "none";
    var fallback = document.createElement("div");
    fallback.className = "chart-fallback";
    var values = data.datasets && data.datasets[0] ? data.datasets[0].data : [];
    var labels = data.labels || [];
    var max = Math.max.apply(null, values.concat([1]));
    values.forEach(function (v, i) {
      var bar = document.createElement("div");
      bar.className = "bar";
      bar.style.height = Math.max(8, (v / max) * 100) + "%";
      var label = document.createElement("span");
      label.textContent = labels[i] || "";
      bar.appendChild(label);
      fallback.appendChild(bar);
    });
    wrap.appendChild(fallback);
  }

  function initCharts() {
    document.querySelectorAll("canvas[data-chart]").forEach(function (canvas) {
      var data = parseChartData(canvas);
      if (!data) return;

      if (typeof Chart !== "undefined") {
        var type = canvas.getAttribute("data-chart-type") || data.type || "bar";
        var dataset = data.datasets || [
          {
            label: data.label || "Data",
            data: data.data || [],
            backgroundColor: SOFT_COLORS,
            borderColor: SOFT_COLORS.map(function (c) {
              return c.replace("0.85", "1");
            }),
            borderWidth: 1,
            borderRadius: 8,
            tension: 0.35,
          },
        ];

        if (data.datasets) {
          dataset.forEach(function (ds, i) {
            if (!ds.backgroundColor) {
              ds.backgroundColor = type === "line" ? SOFT_COLORS[i % SOFT_COLORS.length] : SOFT_COLORS;
            }
            if (!ds.borderColor) {
              ds.borderColor = SOFT_COLORS[i % SOFT_COLORS.length];
            }
            if (ds.borderRadius == null) ds.borderRadius = 8;
            if (ds.tension == null) ds.tension = 0.35;
          });
        }

        new Chart(canvas.getContext("2d"), {
          type: type,
          data: {
            labels: data.labels || [],
            datasets: dataset,
          },
          options: {
            responsive: true,
            maintainAspectRatio: true,
            plugins: {
              legend: {
                display: type !== "bar" || (dataset.length > 1),
                labels: { color: "#6B728A", font: { family: "Nunito" } },
              },
            },
            scales:
              type === "doughnut" || type === "pie"
                ? {}
                : {
                    x: {
                      grid: { display: false },
                      ticks: { color: "#6B728A" },
                    },
                    y: {
                      beginAtZero: true,
                      grid: { color: "rgba(232, 226, 216, 0.9)" },
                      ticks: { color: "#6B728A" },
                    },
                  },
          },
        });
      } else {
        renderCssFallback(canvas, {
          labels: data.labels || [],
          datasets: data.datasets || [{ data: data.data || [] }],
        });
      }
    });
  }

  /* --------------------------------------------------------------------------
     Boot
     -------------------------------------------------------------------------- */
  function boot() {
    ensureToastHost();
    initPublicNav();
    initGallery();
    initCounters();
    initReveal();
    initSidebar();
    initDropdowns();
    initTableFilters();
    initAttendance();
    initAssignmentPublish();
    initQuickActions();
    initPagination();
    initTimetable();
    initChat();
    initCharts();
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", boot);
  } else {
    boot();
  }
})();
