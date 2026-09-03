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

  var FAQS = [
    { keys: ["hello", "hi", "hey", "ciao", "buongiorno", "salve"], en: "Hi! I’m the campus desk. Ask about hours, programs, fees, uniform, buses — or say you want to book an appointment.", it: "Ciao! Sono lo sportello del campus. Chiedi orari, programmi, rette, divisa, pullman — o di’ che vuoi prenotare un appuntamento." },
    { keys: ["thank", "grazie"], en: "You’re welcome. Anything else about the school?", it: "Prego. Altro sulla scuola?" },
    { keys: ["who are you", "your name", "chi sei"], en: "I’m the Scuola Materna campus assistant. I answer school questions and can book a visit.", it: "Sono l’assistente della Scuola Materna. Rispondo alle domande e posso prenotare una visita." },
    { keys: ["help", "aiuto", "what can you"], en: "Ask me about hours, programs, admissions, fees, uniform, lunch, buses, teachers, class size, holidays, the parent portal, or booking a visit.", it: "Chiedimi orari, programmi, iscrizioni, rette, divisa, mensa, pullman, insegnanti, numero in classe, vacanze, portale genitori o una visita." },
    { keys: ["weekend", "saturday", "sunday", "sabato", "domenica"], en: "The office is closed Saturday and Sunday. We open again Monday at 8:00 AM.", it: "La segreteria è chiusa sabato e domenica. Riapriamo lunedì alle 8:00." },
    { keys: ["hour", "open", "orari", "apert", "closing"], en: "Office hours: Monday–Friday, 8:00 AM – 4:00 PM at 42 Maple Grove, Riverside. Phone +1 (555) 214-8800.", it: "Orari: lunedì–venerdì 8:00–16:00, 42 Maple Grove, Riverside. Tel. +1 (555) 214-8800." },
    { keys: ["early learning", "nursery", "infanzia", "preschool", "kindergarten"], en: "Early Learning is play-based discovery for our youngest children — curiosity, stories and gentle routines.", it: "Prima infanzia è un percorso per i più piccoli: gioco, storie e routine delicate." },
    { keys: ["primary", "elementar", "primaria"], en: "Primary School builds strong reading, maths and kindness — the foundations for lifelong learning.", it: "La scuola primaria costruisce lettura, matematica e gentilezza — basi per tutta la vita." },
    { keys: ["middle school", "scuola media"], en: "Middle School grows independence, confidence and deeper subject knowledge.", it: "La scuola media sviluppa indipendenza, fiducia e materie più approfondite." },
    { keys: ["program", "curric", "pathway", "percors", "materie"], en: "Pathways: Early Learning, Primary, Middle School, Creative Arts, Sports, and Science & Technology.", it: "Percorsi: Prima infanzia, Scuola primaria, Scuola media, Arti creative, Sport, Scienza e tecnologia." },
    { keys: ["art", "arte", "music", "musica"], en: "Art and music studios run all week. Children paint, sing, and perform at the annual show.", it: "Sale arte e musica tutta la settimana. I bambini dipingono, cantano e si esibiscono al saggio." },
    { keys: ["sport", "pe", "educazione fisica", "football", "soccer"], en: "Sports include PE, house races and Sports Day on the field. We mix teamwork with lots of orange slices.", it: "Sport: educazione fisica, gare delle case e Giornata dello sport. Tanto gioco di squadra." },
    { keys: ["stem", "science", "scienz", "computer", "coding", "lab"], en: "Science and computer labs are hands-on: experiments, coding as a creative language, and curious questions.", it: "Laboratori di scienze e informatica: esperimenti, coding creativo e tante domande." },
    { keys: ["admiss", "enroll", "iscriz", "apply", "application"], en: "Admissions start with a campus visit. Say “book an appointment” and I’ll take your name, email, day/time and your child’s age.", it: "Le iscrizioni iniziano con una visita. Di’ “prenota un appuntamento” e chiederò nome, email, giorno/orario ed età." },
    { keys: ["fee", "tuition", "cost", "price", "retta", "quanto costa"], en: "Tuition is explained in person on a visit so we can match the right pathway. Book an appointment and the office will walk you through fees.", it: "Le rette si spiegano in visita, in base al percorso. Prenota un appuntamento e la segreteria ti guida." },
    { keys: ["how old", "age", "eta", "età", "years old"], en: "We welcome children from early years through middle school. Tell me your child’s age when you book a visit.", it: "Accogliamo dalla prima infanzia alla scuola media. Dimmi l’età del bambino quando prenoti." },
    { keys: ["uniform", "divisa"], en: "Yes — a simple school uniform. The office shares the list at your visit.", it: "Sì, c’è una divisa semplice. La lista si riceve in visita." },
    { keys: ["lunch", "canteen", "mensa", "food", "cafeteria"], en: "The cafeteria is a comfortable dining space. Packed lunches are welcome too.", it: "La mensa è un spazio comodo. Si può portare anche il pranzo da casa." },
    { keys: ["bus", "transport", "pullman", "scuolabus"], en: "Safe school transport is available. Ask the office during your visit for routes.", it: "C’è un trasporto scolastico sicuro. I percorsi si vedono in segreteria." },
    { keys: ["address", "where", "map", "indirizzo", "location", "dove siete"], en: "42 Maple Grove, Riverside. Email hello@brightsteps.academy or call +1 (555) 214-8800.", it: "42 Maple Grove, Riverside. Email hello@brightsteps.academy — tel. +1 (555) 214-8800." },
    { keys: ["phone", "call", "telefon", "number"], en: "Call the office on +1 (555) 214-8800, Monday–Friday 8:00 AM – 4:00 PM.", it: "Tel. +1 (555) 214-8800, lunedì–venerdì 8:00–16:00." },
    { keys: ["email", "write", "scriv"], en: "Email hello@brightsteps.academy — or use the contact form on this site.", it: "Email hello@brightsteps.academy — oppure il modulo Contatti sul sito." },
    { keys: ["teacher", "insegn", "staff", "faculty"], en: "Warm, qualified teachers who know every child by name — maths, science, English, art, music, PE and computing.", it: "Insegnanti preparati che conoscono ogni bambino per nome — matematica, scienze, inglese, arte, musica, motoria e informatica." },
    { keys: ["class size", "how many children", "quanti bambini", "ratio"], en: "Classes stay small enough that teachers know every child. Ask on your visit for the current year-group sizes.", it: "Classi contenute: gli insegnanti conoscono ogni bambino. I numeri precisi si vedono in visita." },
    { keys: ["language", "english", "italian", "italiano", "inglese", "bilingual"], en: "The public site switches English and Italian with EN | IT. Teaching mixes both so families feel at home.", it: "Il sito passa da inglese a italiano con EN | IT. La didattica accoglie entrambe le lingue." },
    { keys: ["portal", "login", "acced", "password", "demo"], en: "Parents, teachers and students sign in from Accedi. Demo: student_demo, parent_demo, teacher_demo — password Demo@12345. You can also register.", it: "Si accede da Accedi. Demo: student_demo, parent_demo, teacher_demo — password Demo@12345. Puoi anche registrarti." },
    { keys: ["parent portal", "portale genitori"], en: "The parent portal shows diary, attendance and announcements for your child.", it: "Il portale genitori mostra diario, presenze e avvisi." },
    { keys: ["student portal", "portale student"], en: "The student portal has timetable, assignments and marks.", it: "Il portale studenti ha orario, compiti e voti." },
    { keys: ["homework", "compiti", "assignment"], en: "Homework is short and meaningful. Parents see it in the diary on the portal.", it: "I compiti sono brevi e utili. I genitori li vedono nel diario del portale." },
    { keys: ["holiday", "vacation", "vacanz", "break", "closed"], en: "Term dates and holidays are posted on the events page and in the parent portal.", it: "Calendario e vacanze sono in Eventi e nel portale genitori." },
    { keys: ["sick", "ill", "malat", "fever", "assent"], en: "If a child is unwell, keep them home and message the office. We log attendance on the portal.", it: "Se il bambino sta male, resti a casa e avvisa la segreteria. Le assenze sono nel portale." },
    { keys: ["safe", "security", "sicur", "bully"], en: "The campus is secure, with caring staff and clear routines. Kindness is part of the code.", it: "Campus sicuro, personale attento e routine chiare. La gentilezza fa parte delle regole." },
    { keys: ["playground", "play area", "cortile", "giochi"], en: "A safe play area sits beside the fields — space to run, climb and make friends.", it: "Area giochi sicura accanto ai campi — si corre, si gioca, si fanno amici." },
    { keys: ["library", "bibliotec", "books"], en: "The modern library has books, quiet corners and reading week every year.", it: "Biblioteca moderna: libri, angoli silenziosi e la settimana della lettura." },
    { keys: ["after school", "aftercare", "doposcuola", "club"], en: "Clubs and after-school fun run beyond the bell — art, sport, reading and science.", it: "Doposcuola e club oltre l’orario: arte, sport, lettura e scienze." },
    { keys: ["event", "calendar", "calendario"], en: "Sports days, exhibitions, parent meetings and the annual show are on the Events page.", it: "Giornate sportive, mostre, colloqui e saggio annuale sono in Eventi." },
    { keys: ["sports day", "giornata dello sport"], en: "Sports Day is on the field — races, relays and house cheers. Next one is marked on the events posters.", it: "La Giornata dello sport è in campo: gare, staffette e tifo. La data è sui manifesti Eventi." },
    { keys: ["park", "parking"], en: "Short-stay parking is on Maple Grove in front of the gate. Please keep the bus lane clear.", it: "Parcheggio breve in Maple Grove davanti al cancello. Lascia libera la corsia del pullman." },
    { keys: ["special need", "sen", "inclus", "disability", "bes"], en: "We plan support with families. Mention any needs when you book a visit so the right teacher can join.", it: "Progettiamo il supporto con le famiglie. Segnala le esigenze in visita così c’è l’insegnante giusto." },
    { keys: ["vaccine", "vaccin", "health", "nurse", "infermer"], en: "A nurse / first-aid room supports student health. Immunisation records are requested at admission.", it: "C’è un’infermeria. In iscrizione chiediamo i certificati vaccinali." },
    { keys: ["start", "term", "when does school", "inizio"], en: "The school year follows the published calendar. A visit is the easiest way to lock a start date.", it: "L’anno segue il calendario pubblicato. Una visita è il modo più semplice per fissare l’inizio." },
    { keys: ["supply", "stationery", "material", "quadern"], en: "A simple supplies list is given at enrolment. The school provides a lot of shared classroom material.", it: "Una lista semplice si riceve all’iscrizione. Molto materiale è condiviso in classe." },
    { keys: ["photo", "gallery", "galleria"], en: "The gallery page is a scrapbook of campus life — click a photo to open it.", it: "La galleria è uno scrapbook del campus — clicca una foto per aprirla." },
    { keys: ["contact form", "message", "modulo"], en: "The contact form on this page reaches the office. For a timed visit, say you want to book an appointment here in chat.", it: "Il modulo Contatti arriva in segreteria. Per una visita ad orario, prenota qui in chat." },
    { keys: ["headmaster", "principal", "preside", "head teacher"], en: "Our headmaster, Grace Okonkwo, oversees campus life. You can meet leadership on a booked visit.", it: "La preside Grace Okonkwo guida il campus. Si può incontrare la dirigenza in visita." },
    { keys: ["birthday", "compleanno"], en: "Class birthdays are low-key and kind. Tell the teacher a week ahead if you’d like to share a treat.", it: "I compleanni in classe sono semplici. Avvisa l’insegnante una settimana prima se porti qualcosa." },
    { keys: ["weather", "snow", "close", "chiusura"], en: "Weather closures are posted on the parent portal and by email/SMS from the office.", it: "Le chiusure per maltempo arrivano sul portale e via email/SMS dalla segreteria." },
    { keys: ["pta", "parent teacher", "colloqui"], en: "Parent-teacher meetings are on the calendar. You can also request a conversation any week through the office.", it: "I colloqui sono in calendario. Si può anche chiedere un incontro in qualsiasi settimana." },
    { keys: ["religion", "faith", "relig"], en: "We welcome every family. Celebrations are cultural and kind, never exclusive.", it: "Ogni famiglia è la benvenuta. Le feste sono culturali e inclusive." },
    { keys: ["waitlist", "waiting list", "lista d'attesa"], en: "If a year group is full we keep a waitlist. Book a visit anyway so we know your child’s age.", it: "Se la classe è piena teniamo una lista d’attesa. Prenota comunque una visita e indica l’età." },
    { keys: ["tour", "open day"], en: "Campus tours are by appointment. Say “I want to book an appointment” and I’ll collect your details.", it: "Le visite sono su appuntamento. Di’ “voglio prenotare” e raccolgo i dati." },
  ];

  function wantsBook(s) {
    return hasAny(s, [
      "book an appointment",
      "book appointment",
      "book a visit",
      "wanna book",
      "want to book",
      "want a visit",
      "schedule a visit",
      "make an appointment",
      "appointment",
      "appuntamento",
      "prenot",
      "open day",
      "come see the school",
    ]);
  }

  function intentReply(text) {
    var s = norm(text);
    if (wantsBook(s)) return null;
    var code = lang();
    var i;
    for (i = 0; i < FAQS.length; i++) {
      if (hasAny(s, FAQS[i].keys)) return FAQS[i][code] || FAQS[i].en;
    }
    return t().fallback;
  }

  var booking = { step: "", name: "", email: "", when: "", age: "" };
  var rec = null;
  var voiceTurn = false;

  function hushVoice() {
    if (window.speechSynthesis) {
      try {
        window.speechSynthesis.cancel();
      } catch (e) {}
    }
  }

  function speak(text) {
    if (!voiceTurn || !window.speechSynthesis) return;
    try {
      window.speechSynthesis.cancel();
      var u = new SpeechSynthesisUtterance(text);
      u.lang = lang() === "en" ? "en-US" : "it-IT";
      u.rate = 1;
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
      voiceTurn = false;
      hushVoice();
      botSay(t().greet);
    }
    document.getElementById("campusBotInput").focus();
  }

  function closePanel() {
    document.getElementById("campusBot").setAttribute("data-open", "false");
    voiceTurn = false;
    stopMic();
    hushVoice();
  }

  function addBubble(who, text) {
    var log = document.getElementById("campusBotLog");
    var b = document.createElement("div");
    b.className = "campus-bot__bubble campus-bot__bubble--" + who;
    b.textContent = text;
    log.appendChild(b);
    log.scrollTop = log.scrollHeight;
  }

  function botSay(text) {
    addBubble("bot", text);
    if (voiceTurn) speak(text);
  }

  function beginTurn(fromVoice) {
    voiceTurn = !!fromVoice;
    if (!voiceTurn) hushVoice();
  }

  function handleUser(text, fromVoice, forced) {
    beginTurn(fromVoice);
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
      botSay(forced);
      return;
    }
    var reply = intentReply(text);
    if (reply == null) {
      startBooking();
      return;
    }
    botSay(reply);
  }

  function startBooking() {
    booking = { step: "name", name: "", email: "", when: "", age: "" };
    botSay(t().askName);
  }

  function stepBooking(raw) {
    var s = raw.trim();
    var n = norm(s);
    if (booking.step !== "name" && isNo(n)) {
      booking.step = "";
      botSay(t().cancel);
      return;
    }
    if (booking.step === "name") {
      booking.name = s;
      booking.step = "email";
      botSay(fill(t().askEmail, { name: booking.name }));
      return;
    }
    if (booking.step === "email") {
      if (!isEmail(s)) {
        botSay(t().badEmail);
        return;
      }
      booking.email = s;
      booking.step = "when";
      botSay(t().askWhen);
      return;
    }
    if (booking.step === "when") {
      booking.when = s;
      booking.step = "age";
      botSay(t().askAge);
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
        })
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
        botSay(t().cancel);
        return;
      }
      botSay(t().yesNeed);
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
        botSay(t().booked);
      })
      .catch(function () {
        booking.step = "";
        botSay(t().bookFail);
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
