/**
 * Scuola Materna campus assistant — chat, voice, visit booking.
 * Bookings POST to /api/contact as [School visit] messages.
 */
(function () {
  "use strict";

  var T = {
    en: {
      fab: "Ask the school",
      title: "Campus desk",
      sub: "Chat, speak, or book a visit",
      close: "Close chat",
      placeholder: "Ask about hours, programs, or a visit…",
      send: "Send",
      mic: "Speak",
      micOn: "Listening…",
      greet:
        "Hi! I’m the Scuola Materna campus desk. I can tell you about hours, programs, admissions and the parent portal — or book a visit.",
      chipHours: "Hours",
      chipPrograms: "Programs",
      chipVisit: "Book a visit",
      chipPortal: "Portal",
      hours:
        "The office is open Monday–Friday, 8:00 AM – 4:00 PM. Classes run on the same campus at 42 Maple Grove, Riverside. Phone +1 (555) 214-8800.",
      programs:
        "Pathways: Early Learning, Primary, Middle School, Creative Arts, Sports, and Science & Technology. Each mixes curiosity, play and solid teaching.",
      admissions:
        "Admissions start with a campus visit. Tell me you’d like to book, and I’ll take your name, email, a preferred day/time, and your child’s age.",
      portal:
        "Parents, teachers and students sign in from the Student Portal / Accedi button. Demo logins work on this site (student_demo, parent_demo, teacher_demo — password Demo@12345).",
      address:
        "We’re at 42 Maple Grove, Riverside. Email hello@brightsteps.academy or call +1 (555) 214-8800.",
      teachers:
        "Warm, qualified teachers who know every child by name — mathematics, science, English, art, music, PE, computing and more.",
      facilities:
        "Library, science and computer labs, sports ground, art and music rooms, smart classrooms, cafeteria and a safe play area.",
      fallback:
        "I can help with hours, programs, admissions, teachers, facilities, the portal — or booking a visit. What would you like?",
      askName: "Lovely — let’s book a visit. What’s your name?",
      askEmail: "Thanks, {name}. What’s the best email for the office to reach you?",
      askWhen: "What day and time work for a campus visit? (for example: Friday 10:00)",
      askAge: "How old is your child? (or the year group you’re interested in)",
      confirm:
        "Here’s what I have:\n• Name: {name}\n• Email: {email}\n• When: {when}\n• Child: {age}\n\nShall I send this to the school office? Reply yes to confirm.",
      booked:
        "Done — the office has your visit request. You’ll also see it in the site Messages inbox. We look forward to meeting your family!",
      bookFail:
        "I couldn’t reach the office inbox from this page. Please use the contact form, or open the school site from Ali’s Work card so the booking can be saved.",
      badEmail: "That doesn’t look like an email yet. Try again? (name@email.com)",
      cancel: "No problem — booking cancelled. Ask me anything else, or say “book a visit” to start again.",
      yesNeed: "Reply yes to send the request, or no to cancel.",
    },
    it: {
      fab: "Chiedi alla scuola",
      title: "Sportello campus",
      sub: "Scrivi, parla o prenota una visita",
      close: "Chiudi chat",
      placeholder: "Orari, programmi o una visita…",
      send: "Invia",
      mic: "Parla",
      micOn: "Ti ascolto…",
      greet:
        "Ciao! Sono lo sportello della Scuola Materna. Posso parlarti di orari, programmi, iscrizioni e del portale — oppure prenotare una visita.",
      chipHours: "Orari",
      chipPrograms: "Programmi",
      chipVisit: "Prenota visita",
      chipPortal: "Portale",
      hours:
        "La segreteria è aperta lunedì–venerdì, 8:00–16:00. Siamo in 42 Maple Grove, Riverside. Tel. +1 (555) 214-8800.",
      programs:
        "Percorsi: Prima infanzia, Scuola primaria, Scuola media, Arti creative, Sport, Scienza e tecnologia. Ogni percorso unisce curiosità, gioco e solida didattica.",
      admissions:
        "Le iscrizioni iniziano con una visita al campus. Dimmi che vuoi prenotare: chiederò nome, email, giorno/orario e l’età del bambino.",
      portal:
        "Genitori, insegnanti e studenti accedono dal pulsante Accedi / Portale studenti. Su questo sito funzionano gli accessi demo (student_demo, parent_demo, teacher_demo — password Demo@12345).",
      address:
        "Siamo in 42 Maple Grove, Riverside. Email hello@brightsteps.academy oppure tel. +1 (555) 214-8800.",
      teachers:
        "Insegnanti preparati e calorosi che conoscono ogni bambino per nome — matematica, scienze, inglese, arte, musica, educazione fisica, informatica e altro.",
      facilities:
        "Biblioteca, laboratori di scienze e informatica, campo sportivo, sale arte e musica, aule smart, mensa e un’area giochi sicura.",
      fallback:
        "Posso aiutarti con orari, programmi, iscrizioni, insegnanti, strutture, il portale — o prenotare una visita. Cosa ti serve?",
      askName: "Perfetto — prenotiamo una visita. Come ti chiami?",
      askEmail: "Grazie, {name}. Qual è l’email migliore per la segreteria?",
      askWhen: "Che giorno e orario ti vanno per la visita? (es. venerdì 10:00)",
      askAge: "Quanti anni ha tuo figlio? (o la classe che ti interessa)",
      confirm:
        "Ecco i dati:\n• Nome: {name}\n• Email: {email}\n• Quando: {when}\n• Bambino: {age}\n\nInvio la richiesta in segreteria? Rispondi sì per confermare.",
      booked:
        "Fatto — la segreteria ha la richiesta di visita. Compare anche in Messaggi nel pannello admin. Non vediamo l’ora di incontrare la vostra famiglia!",
      bookFail:
        "Da questa pagina non riesco a salvare la richiesta. Usa il modulo Contatti, oppure apri il sito scuola dalla card Work del portfolio.",
      badEmail: "Quella non sembra ancora un’email. Riprova? (nome@email.com)",
      cancel: "Va bene — prenotazione annullata. Chiedimi pure altro, o di’ “prenota una visita” per ricominciare.",
      yesNeed: "Rispondi sì per inviare, o no per annullare.",
    },
  };

  function lang() {
    if (window.ScuolaLang && typeof window.ScuolaLang.get === "function") {
      var l = window.ScuolaLang.get();
      if (l === "en" || l === "it") return l;
    }
    return document.documentElement.lang === "en" ? "en" : "it";
  }

  function t() {
    return T[lang()] || T.it;
  }

  function fill(s, map) {
    return String(s).replace(/\{(\w+)\}/g, function (_, k) {
      return map[k] != null ? map[k] : "";
    });
  }

  function norm(s) {
    return String(s || "")
      .toLowerCase()
      .normalize("NFD")
      .replace(/[\u0300-\u036f]/g, "")
      .replace(/[’']/g, "'")
      .trim();
  }

  function hasAny(s, words) {
    return words.some(function (w) {
      return s.indexOf(w) !== -1;
    });
  }

  function isYes(s) {
    return /^(y|yes|ok|okay|sure|please|si|sì|sì\.|certo|va bene|oké)\b/.test(s) || s === "sì" || s === "si";
  }

  function isNo(s) {
    return /^(n|no|nope|cancel|stop|annulla|basta)\b/.test(s);
  }

  function isEmail(s) {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(s);
  }

  function wantsBook(s) {
    return hasAny(s, [
      "book",
      "visit",
      "tour",
      "appointment",
      "come see",
      "open day",
      "prenot",
      "visita",
      "appuntamento",
      "open day",
      "iscriz",
    ]);
  }

  function intentReply(text) {
    var s = norm(text);
    if (hasAny(s, ["hour", "open", "orari", "apert", "quando siete", "office"])) return t().hours;
    if (hasAny(s, ["program", "curric", "early", "primary", "pathway", "percors", "infanzia", "materie"]))
      return t().programs;
    if (hasAny(s, ["portal", "login", "acced", "password", "demo"])) return t().portal;
    if (hasAny(s, ["address", "where", "map", "indirizzo", "dove", "phone", "telefon", "email"])) return t().address;
    if (hasAny(s, ["teacher", "insegn", "staff", "faculty"])) return t().teachers;
    if (hasAny(s, ["facilit", "library", "lab", "sport", "struttur", "bibliotec", "campus"])) return t().facilities;
    if (hasAny(s, ["admiss", "enroll", "iscriz", "tuition", "fee"])) return t().admissions;
    if (hasAny(s, ["hello", "hi ", "hey", "ciao", "buongiorno", "salve"])) return t().greet;
    if (wantsBook(s)) return null;
    return t().fallback;
  }

  var booking = { step: "", name: "", email: "", when: "", age: "" };
  var rec = null;
  var speaking = false;

  function speak(text) {
    if (!window.speechSynthesis) return;
    try {
      window.speechSynthesis.cancel();
      var u = new SpeechSynthesisUtterance(text);
      u.lang = lang() === "en" ? "en-US" : "it-IT";
      u.rate = 1;
      speaking = true;
      u.onend = function () {
        speaking = false;
      };
      window.speechSynthesis.speak(u);
    } catch (e) {}
  }

  function el(html) {
    var d = document.createElement("div");
    d.innerHTML = html.trim();
    return d.firstChild;
  }

  function mount() {
    if (document.getElementById("campusBot")) return;
    var root = el(
      '<div class="campus-bot" id="campusBot" data-no-translate data-open="false">' +
        '<button type="button" class="campus-bot__fab" id="campusBotFab"></button>' +
        '<div class="campus-bot__panel" role="dialog" aria-labelledby="campusBotTitle">' +
        '<div class="campus-bot__head">' +
        '<span class="campus-bot__avatar" aria-hidden="true">🏫</span>' +
        "<div><h2 id=\"campusBotTitle\"></h2><p id=\"campusBotSub\"></p></div>" +
        '<button type="button" class="campus-bot__x" id="campusBotClose" aria-label="Close">×</button>' +
        "</div>" +
        '<div class="campus-bot__log" id="campusBotLog"></div>' +
        '<div class="campus-bot__chips" id="campusBotChips"></div>' +
        '<form class="campus-bot__form" id="campusBotForm">' +
        '<input id="campusBotInput" autocomplete="off" />' +
        '<button type="button" class="campus-bot__mic" id="campusBotMic"></button>' +
        '<button type="submit" class="campus-bot__send" id="campusBotSend">➤</button>' +
        "</form>" +
        '<div class="campus-bot__status" id="campusBotStatus"></div>' +
        "</div></div>"
    );
    document.body.appendChild(root);
    paintChrome();
    document.getElementById("campusBotFab").addEventListener("click", openPanel);
    document.getElementById("campusBotClose").addEventListener("click", closePanel);
    document.getElementById("campusBotForm").addEventListener("submit", function (e) {
      e.preventDefault();
      var input = document.getElementById("campusBotInput");
      var val = (input.value || "").trim();
      if (!val) return;
      input.value = "";
      handleUser(val, false);
    });
    document.getElementById("campusBotMic").addEventListener("click", toggleMic);
    document.addEventListener("scuola-lang", function () {
      paintChrome();
    });
  }

  function paintChrome() {
    var copy = t();
    var fab = document.getElementById("campusBotFab");
    if (!fab) return;
    fab.innerHTML = '<span class="campus-bot__fab-ico" aria-hidden="true">💬</span><span></span>';
    fab.lastChild.textContent = copy.fab;
    document.getElementById("campusBotTitle").textContent = copy.title;
    document.getElementById("campusBotSub").textContent = copy.sub;
    document.getElementById("campusBotClose").setAttribute("aria-label", copy.close);
    document.getElementById("campusBotInput").setAttribute("placeholder", copy.placeholder);
    document.getElementById("campusBotSend").setAttribute("aria-label", copy.send);
    var mic = document.getElementById("campusBotMic");
    mic.textContent = "🎙️";
    mic.setAttribute("aria-label", copy.mic);
    renderChips();
  }

  function renderChips() {
    var box = document.getElementById("campusBotChips");
    if (!box) return;
    var copy = t();
    box.innerHTML = "";
    [
      [copy.chipHours, copy.hours],
      [copy.chipPrograms, copy.programs],
      [copy.chipVisit, "__book__"],
      [copy.chipPortal, copy.portal],
    ].forEach(function (pair) {
      var b = document.createElement("button");
      b.type = "button";
      b.className = "campus-bot__chip";
      b.textContent = pair[0];
      b.addEventListener("click", function () {
        handleUser(pair[0], false, pair[1]);
      });
      box.appendChild(b);
    });
  }

  function openPanel() {
    document.getElementById("campusBot").setAttribute("data-open", "true");
    var log = document.getElementById("campusBotLog");
    if (log && !log.childElementCount) {
      botSay(t().greet, true);
    }
    document.getElementById("campusBotInput").focus();
  }

  function closePanel() {
    document.getElementById("campusBot").setAttribute("data-open", "false");
    stopMic();
    if (window.speechSynthesis) window.speechSynthesis.cancel();
  }

  function addBubble(who, text) {
    var log = document.getElementById("campusBotLog");
    var b = document.createElement("div");
    b.className = "campus-bot__bubble campus-bot__bubble--" + who;
    b.textContent = text;
    log.appendChild(b);
    log.scrollTop = log.scrollHeight;
  }

  function botSay(text, voice) {
    addBubble("bot", text);
    if (voice !== false) speak(text);
  }

  function handleUser(text, fromVoice, forced) {
    addBubble("user", text);
    if (forced === "__book__" || (forced == null && wantsBook(norm(text)) && !booking.step)) {
      startBooking();
      return;
    }
    if (booking.step) {
      stepBooking(text);
      return;
    }
    if (forced && forced !== "__book__") {
      botSay(forced, true);
      return;
    }
    var reply = intentReply(text);
    if (reply == null) {
      startBooking();
      return;
    }
    botSay(reply, true);
  }

  function startBooking() {
    booking = { step: "name", name: "", email: "", when: "", age: "" };
    botSay(t().askName, true);
  }

  function stepBooking(raw) {
    var s = raw.trim();
    var n = norm(s);
    if (booking.step !== "name" && isNo(n)) {
      booking.step = "";
      botSay(t().cancel, true);
      return;
    }
    if (booking.step === "name") {
      booking.name = s;
      booking.step = "email";
      botSay(fill(t().askEmail, { name: booking.name }), true);
      return;
    }
    if (booking.step === "email") {
      if (!isEmail(s)) {
        botSay(t().badEmail, true);
        return;
      }
      booking.email = s;
      booking.step = "when";
      botSay(t().askWhen, true);
      return;
    }
    if (booking.step === "when") {
      booking.when = s;
      booking.step = "age";
      botSay(t().askAge, true);
      return;
    }
    if (booking.step === "age") {
      booking.age = s;
      booking.step = "confirm";
      botSay(
        fill(t().confirm, {
          name: booking.name,
          email: booking.email,
          when: booking.when,
          age: booking.age,
        }),
        true
      );
      return;
    }
    if (booking.step === "confirm") {
      if (isYes(n)) {
        submitVisit();
        return;
      }
      if (isNo(n)) {
        booking.step = "";
        botSay(t().cancel, true);
        return;
      }
      botSay(t().yesNeed, true);
    }
  }

  function submitVisit() {
    var message =
      "[School visit]\nWhen: " +
      booking.when +
      "\nChild age / year: " +
      booking.age +
      "\nLanguage: " +
      lang();
    fetch("/api/contact", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        name: booking.name.slice(0, 120),
        email: booking.email.slice(0, 200),
        message: message.slice(0, 4000),
      }),
    })
      .then(function (res) {
        booking.step = "";
        if (!res.ok) throw new Error("fail");
        botSay(t().booked, true);
      })
      .catch(function () {
        booking.step = "";
        botSay(t().bookFail, true);
      });
  }

  function SpeechRec() {
    return window.SpeechRecognition || window.webkitSpeechRecognition || null;
  }

  function toggleMic() {
    if (rec) {
      stopMic();
      return;
    }
    var Ctor = SpeechRec();
    var mic = document.getElementById("campusBotMic");
    if (!Ctor) {
      document.getElementById("campusBotStatus").textContent =
        lang() === "en" ? "Voice works in Chrome or Edge." : "La voce funziona in Chrome o Edge.";
      return;
    }
    rec = new Ctor();
    rec.lang = lang() === "en" ? "en-US" : "it-IT";
    rec.interimResults = false;
    rec.onresult = function (ev) {
      var said = ev.results[0][0].transcript;
      stopMic();
      handleUser(said, true);
    };
    rec.onerror = function () {
      stopMic();
    };
    rec.onend = function () {
      if (rec) stopMic();
    };
    rec.start();
    mic.classList.add("is-on");
    mic.setAttribute("aria-label", t().micOn);
    document.getElementById("campusBotStatus").textContent = t().micOn;
  }

  function stopMic() {
    var mic = document.getElementById("campusBotMic");
    if (rec) {
      try {
        rec.stop();
      } catch (e) {}
      rec = null;
    }
    if (mic) {
      mic.classList.remove("is-on");
      mic.setAttribute("aria-label", t().mic);
    }
    var st = document.getElementById("campusBotStatus");
    if (st) st.textContent = "";
  }

  function boot() {
    mount();
  }

  if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", boot);
  else boot();
})();
