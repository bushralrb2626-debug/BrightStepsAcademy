/**
 * Scuola Materna — EN / IT language switcher
 * Default: Italian. Choice saved in localStorage.
 */
(function () {
  "use strict";

  var STORAGE_KEY = "scuola-materna-lang";

  // [English, Italian] — longer phrases first
  var PAIRS = [
    ["Learn. Explore. Grow.", "Impara. Esplora. Cresci."],
    ["Welcome to Scuola Materna", "Benvenuti alla Scuola Materna"],
    ["WHERE LITTLE MINDS GROW INTO BIG DREAMS", "DOVE LE PICCOLE MENTI CRESCONO IN GRANDI SOGNI"],
    ["WHERE LITTLE MINDS", "DOVE LE PICCOLE MENTI"],
    ["GROW INTO BIG DREAMS", "CRESCONO IN GRANDI SOGNI"],
    [
      "A colorful campus where curiosity blooms, creativity shines, and every child gets to learn, explore and dream",
      "Un campus colorato dove fiorisce la curiosità, brilla la creatività e ogni bambino può imparare, esplorare e sognare",
    ],
    ["every single day.", "ogni singolo giorno."],
    ["Explore Our School", "Esplora la nostra scuola"],
    ["Student Portal", "Portale studenti"],
    ["Our Learning World", "Il nostro mondo dell’apprendimento"],
    ["A story of bright beginnings", "Una storia di inizi luminosi"],
    [
      "Not just a campus — a colorful journey families love to join.",
      "Non solo un campus — un percorso colorato che le famiglie amano condividere.",
    ],
    ["Where learning begins", "Dove inizia l’apprendimento"],
    ["Our mission", "La nostra missione"],
    ["Our vision", "La nostra visione"],
    ["Why families choose us", "Perché le famiglie ci scelgono"],
    [
      "To nurture curious, kind and confident learners through joyful teaching and meaningful experiences.",
      "Coltivare apprendisti curiosi, gentili e sicuri di sé attraverso un insegnamento gioioso ed esperienze significative.",
    ],
    [
      "A world where every child feels seen, challenged and inspired to grow — one bright step at a time.",
      "Un mondo in cui ogni bambino si sente visto, stimolato e ispirato a crescere — un passo luminoso alla volta.",
    ],
    [
      "Safe spaces, creative learning, strong academics, and a portal that keeps parents close to every milestone.",
      "Spazi sicuri, apprendimento creativo, solida didattica e un portale che tiene i genitori vicini a ogni traguardo.",
    ],
    [
      "Sunlit classrooms, curious questions, and teachers who know every child by name — that’s the Scuola Materna way.",
      "Aule illuminate, domande curiose e insegnanti che conoscono ogni bambino per nome — questo è lo stile Scuola Materna.",
    ],
    [
      "Sunlit classrooms, curious questions, and teachers who know every child by name — that’s the BrightSteps way.",
      "Aule illuminate, domande curiose e insegnanti che conoscono ogni bambino per nome — questo è lo stile Scuola Materna.",
    ],
    ["Early Learning", "Prima infanzia"],
    ["Primary School", "Scuola primaria"],
    ["Middle School", "Scuola media"],
    ["Creative Arts", "Arti creative"],
    ["Science & Technology", "Scienza e tecnologia"],
    ["Building curiosity through play and discovery.", "Coltivare la curiosità attraverso il gioco e la scoperta."],
    ["Strong foundations for lifelong learning.", "Basi solide per un apprendimento che dura tutta la vita."],
    ["Developing knowledge, confidence and independence.", "Sviluppare conoscenze, fiducia e indipendenza."],
    ["Helping students express their imagination.", "Aiutare gli studenti a esprimere la loro immaginazione."],
    ["Building teamwork, confidence and healthy habits.", "Costruire collaborazione, fiducia e abitudini sane."],
    ["Exploring the world through innovation and discovery.", "Esplorare il mondo attraverso innovazione e scoperta."],
    ["Modern Library", "Biblioteca moderna"],
    ["Science Laboratory", "Laboratorio di scienze"],
    ["Computer Lab", "Laboratorio di informatica"],
    ["Sports Ground", "Campo sportivo"],
    ["Art & Creativity Room", "Sala arte e creatività"],
    ["Music Room", "Sala musica"],
    ["Play Area", "Area giochi"],
    ["Books, reading spaces and learning resources.", "Libri, spazi di lettura e risorse didattiche."],
    ["Hands-on experiments and discovery.", "Esperimenti pratici e scoperta."],
    ["Technology and digital learning.", "Tecnologia e apprendimento digitale."],
    ["Outdoor sports and physical activities.", "Sport all’aperto e attività motorie."],
    ["Painting, crafts and creative expression.", "Pittura, laboratori e espressione creativa."],
    ["Music, instruments and performance.", "Musica, strumenti e performance."],
    ["Safe and engaging recreational space.", "Spazio ricreativo sicuro e coinvolgente."],
    ["Qualified Teachers", "Insegnanti qualificati"],
    ["Safe Environment", "Ambiente sicuro"],
    ["Creative Learning", "Apprendimento creativo"],
    ["Modern Classrooms", "Aule moderne"],
    ["Sports & Activities", "Sport e attività"],
    ["Parent Engagement", "Coinvolgimento dei genitori"],
    ["Warm, trained educators who know every child by name.", "Educatori preparati e calorosi che conoscono ogni bambino per nome."],
    ["Secure campus, caring staff and clear routines.", "Campus sicuro, personale attento e routine chiare."],
    ["Projects, stories and play that make ideas stick.", "Progetti, storie e gioco che fanno restare le idee."],
    ["Bright rooms, smart boards and space to move.", "Aule luminose, lavagne interattive e spazio per muoversi."],
    ["Fields, clubs and festivals for every interest.", "Campi, club e feste per ogni interesse."],
    ["Clear updates, meetings and a portal that stays in sync.", "Aggiornamenti chiari, incontri e un portale sempre aggiornato."],
    ["Art Class", "Lezione di arte"],
    ["Science Fair", "Fiera della scienza"],
    ["Sports Day", "Giornata dello sport"],
    ["Field Trip", "Gita scolastica"],
    ["Reading Week", "Settimana della lettura"],
    ["Annual Function", "Saggio di fine anno"],
    ["Brushes, collage and colour mixing every Wednesday.", "Pennelli, collage e mischiare i colori ogni mercoledì."],
    ["Experiments, posters and proud inventors.", "Esperimenti, poster e piccoli inventori orgogliosi."],
    ["Races, relays and plenty of orange slices.", "Gare, staffette e tante fette d’arancia."],
    ["Learning beyond the classroom walls.", "Imparare oltre le mura dell’aula."],
    ["Blankets, book nooks and favourite characters.", "Coperte, angoli lettura e personaggi preferiti."],
    ["Music, drama and a stage full of sparkle.", "Musica, teatro e un palco pieno di luccichio."],
    ["Meet Our Teachers", "Incontra i nostri insegnanti"],
    ["Our Programs", "I nostri programmi"],
    ["Our Facilities", "Le nostre strutture"],
    ["School Activities", "Attività scolastiche"],
    ["Upcoming Events", "Prossimi eventi"],
    ["School Gallery", "Galleria della scuola"],
    ["Visit Us", "Vienici a trovare"],
    ["About Our School", "La nostra scuola"],
    ["Get in touch", "Scrivici"],
    ["Send message", "Invia messaggio"],
    ["Your name", "Il tuo nome"],
    ["Your email", "La tua email"],
    ["Your message", "Il tuo messaggio"],
    ["Office hours", "Orari di ufficio"],
    ["Mon–Fri 8:00 AM – 4:00 PM", "Lun–Ven 8:00 – 16:00"],
    [
      "A joyful place where children learn, explore, and grow — every single day.",
      "Un luogo gioioso dove i bambini imparano, esplorano e crescono — ogni singolo giorno.",
    ],
    ["School Life", "Vita scolastica"],
    ["Explore", "Esplora"],
    ["Visit", "Visita"],
    ["Welcome back", "Bentornati"],
    [
      "Authorized school staff and users only. Your dashboard opens based on your account.",
      "Solo personale e utenti autorizzati. La dashboard si apre in base al tuo account.",
    ],
    ["School Login", "Accesso scuola"],
    [
      "Enter your email and password. Role is determined from your account — not from this form.",
      "Inserisci email e password. Il ruolo dipende dal tuo account — non da questo modulo.",
    ],
    ["Email or Login ID", "Email o ID di accesso"],
    ["Email or login ID", "Email o ID di accesso"],
    ["Remember me", "Ricordami"],
    ["Forgot password?", "Password dimenticata?"],
    ["School portals", "Portali della scuola"],
    ["Choose your portal", "Scegli il tuo portale"],
    ["Teacher", "Insegnante"],
    ["Parent", "Genitore"],
    ["Student", "Studente"],
    ["Headmaster", "Preside"],
    ["Sign in", "Accedi"],
    ["Toggle navigation", "Apri menu"],
    ["Children learning with books", "Bambini che imparano con i libri"],
    ["Classroom moments", "Momenti in aula"],
    ["Story time", "Ora della storia"],
    ["Library quiet hours", "Ore di silenzio in biblioteca"],
    ["Art studio", "Studio d’arte"],
    ["Campus view", "Vista del campus"],
    ["Playground joy", "Gioia nel cortile"],
    ["Annual celebration", "Festa annuale"],
    ["Music class", "Lezione di musica"],
    ["Welcome gate", "Cancello di benvenuto"],
    ["About", "Chi siamo"],
    ["Programs", "Programmi"],
    ["Facilities", "Strutture"],
    ["Teachers", "Insegnanti"],
    ["Insegnantes", "Insegnanti"],
    ["Activities", "Attività"],
    ["Events", "Eventi"],
    ["Gallery", "Galleria"],
    ["Contact", "Contatti"],
    ["Login", "Accedi"],
    ["Sports", "Sport"],
    ["Learning", "Apprendimento"],
    ["Creative", "Creativo"],
  ];

  function currentLang() {
    try {
      var saved = localStorage.getItem(STORAGE_KEY);
      if (saved === "en" || saved === "it") return saved;
    } catch (e) {}
    return "it";
  }

  function setLang(lang) {
    if (lang !== "en" && lang !== "it") lang = "it";
    try {
      localStorage.setItem(STORAGE_KEY, lang);
    } catch (e) {}
    document.documentElement.lang = lang;
    translateDom(lang);
    syncButtons(lang);
  }

  function translateDom(lang) {
    var root = document.body;
    if (!root) return;
    var walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT, null);
    var nodes = [];
    while (walker.nextNode()) nodes.push(walker.currentNode);

    nodes.forEach(function (node) {
      if (!node.parentElement) return;
      var tag = node.parentElement.tagName;
      if (tag === "SCRIPT" || tag === "STYLE" || tag === "NOSCRIPT") return;
      var text = node.nodeValue;
      if (!text || !text.trim()) return;
      var next = text;
      PAIRS.forEach(function (pair) {
        var en = pair[0];
        var it = pair[1];
        if (lang === "it") {
          if (next.indexOf(en) !== -1) next = next.split(en).join(it);
        } else {
          if (next.indexOf(it) !== -1) next = next.split(it).join(en);
        }
      });
      if (next !== text) node.nodeValue = next;
    });

    root.querySelectorAll("[placeholder],[aria-label],[alt],[title]").forEach(function (el) {
      ["placeholder", "aria-label", "alt", "title"].forEach(function (attr) {
        var val = el.getAttribute(attr);
        if (!val) return;
        var next = val;
        PAIRS.forEach(function (pair) {
          var en = pair[0];
          var it = pair[1];
          if (lang === "it") {
            if (next.indexOf(en) !== -1) next = next.split(en).join(it);
          } else if (next.indexOf(it) !== -1) {
            next = next.split(it).join(en);
          }
        });
        if (next !== val) el.setAttribute(attr, next);
      });
    });
  }

  function syncButtons(lang) {
    document.querySelectorAll("[data-set-lang]").forEach(function (btn) {
      var active = btn.getAttribute("data-set-lang") === lang;
      btn.classList.toggle("is-active", active);
      btn.setAttribute("aria-pressed", active ? "true" : "false");
    });
  }

  function ensureSwitcher() {
    if (document.getElementById("langSwitch")) return;
    var nav = document.querySelector(".site-nav__menu");
    if (!nav) return;
    var wrap = document.createElement("div");
    wrap.id = "langSwitch";
    wrap.className = "lang-switch";
    wrap.setAttribute("role", "group");
    wrap.setAttribute("aria-label", "Language");
    wrap.innerHTML =
      '<button type="button" class="lang-switch__btn" data-set-lang="en" aria-pressed="false">EN</button>' +
      '<button type="button" class="lang-switch__btn" data-set-lang="it" aria-pressed="false">IT</button>';
    nav.appendChild(wrap);
    wrap.addEventListener("click", function (e) {
      var btn = e.target.closest("[data-set-lang]");
      if (!btn) return;
      setLang(btn.getAttribute("data-set-lang"));
    });
  }

  function boot() {
    ensureSwitcher();
    setLang(currentLang());
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", boot);
  } else {
    boot();
  }

  window.ScuolaLang = { set: setLang, get: currentLang };
})();
