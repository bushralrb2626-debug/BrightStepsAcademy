/**
 * Scuola Materna campus assistant — chat, voice, visit booking.
 * Bookings POST to /api/contact as [School visit] messages.
 */
(function () {
  "use strict";

  var T = {
    en: {
      fab: "Ask about school",
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
      chipLogin: "Login",
      chipRegister: "Register",
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
        "Done — your visit is booked. School admin and super admin can see it on their Meetings page. We look forward to meeting your family!",
      bookFail:
        "I couldn’t save the visit. Please sign in again and try once more, or use the contact form.",
      needLogin:
        "I can only book a visit after you sign in. Please login or register first — then come back and tap Book a visit.",
      askWhenLoggedIn:
        "You’re signed in as {name}. What day and time work for a campus visit? (for example: Friday 10:00)",
      timeTaken:
        "Sorry — a meeting is already booked around that time ({when}). Please choose another day or time.",
      badEmail: "That doesn’t look like an email yet. Try again? (name@email.com)",
      cancel: "No problem — booking cancelled. Ask me anything else, or say “book a visit” to start again.",
      yesNeed: "Reply yes to send the request, or no to cancel.",
    },
    it: {
      fab: "Info sulla scuola",
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
      chipLogin: "Accedi",
      chipRegister: "Registrati",
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
        "Fatto — la visita è prenotata. Admin e super admin la vedono in Incontri. Non vediamo l’ora di incontrare la vostra famiglia!",
      bookFail:
        "Non riesco a salvare la visita. Accedi di nuovo e riprova, oppure usa il modulo Contatti.",
      needLogin:
        "Posso prenotare solo se hai fatto l’accesso. Accedi o registrati, poi tocca Prenota visita.",
      askWhenLoggedIn:
        "Sei connesso come {name}. Che giorno e orario ti vanno per la visita? (es. venerdì 10:00)",
      timeTaken:
        "Spiacente — c’è già un incontro intorno a quell’orario ({when}). Scegli un altro giorno o orario.",
      badEmail: "Quella non sembra ancora un’email. Riprova? (nome@email.com)",
      cancel: "Va bene — prenotazione annullata. Chiedimi pure altro, o di’ “prenota una visita” per ricominciare.",
      yesNeed: "Rispondi sì per inviare, o no per annullare.",
    },
    ur: {
      fab: "اسکول کے بارے میں پوچھیں",
      title: "کیمپس ڈیسک",
      sub: "لکھو یا بولو — اسی زبان میں جواب",
      close: "چیٹ بند کریں",
      placeholder: "اوقات، پروگرامز، یا وزٹ کے بارے میں پوچھیں…",
      send: "بھیجیں",
      mic: "بولیں",
      micOn: "سن رہا ہوں…",
      greet: "السلام علیکم! میں Scuola Materna کا کیمپس ڈیسک ہوں۔ اوقات، پروگرامز، داخلہ اور والدین پورٹل بتا سکتا ہوں — یا وزٹ بک کروا سکتا ہوں۔",
      chipHours: "اوقات",
      chipPrograms: "پروگرامز",
      chipVisit: "وزٹ بک کریں",
      chipPortal: "پورٹل",
      chipLogin: "لاگ اِن",
      chipRegister: "رجسٹر",
      hours: "دفتر پیر تا جمعہ صبح 8:00 سے شام 4:00 تک کھلا ہے۔ پتہ: 42 Maple Grove, Riverside۔ فون +1 (555) 214-8800۔",
      programs: "راستے: Early Learning، پرائمری، مڈل اسکول، آرٹس، اسپورٹس، اور سائنس و ٹیکنالوجی۔ ہر راستے میں کھیل، تجسس اور مضبوط تعلیم ہے۔",
      admissions: "داخلہ کیمپس وزٹ سے شروع ہوتا ہے۔ کہیں کہ وزٹ بک کرنی ہے — نام، ای میل، دن/وقت اور بچے کی عمر پوچھوں گا۔",
      portal: "والدین، اساتذہ اور طلبہ Student Portal / Accedi سے لاگ اِن کریں۔ ڈیمو: student_demo، parent_demo، teacher_demo — پاس ورڈ Demo@12345۔",
      address: "پتہ: 42 Maple Grove, Riverside۔ ای میل hello@brightsteps.academy یا فون +1 (555) 214-8800۔",
      teachers: "محبت کرنے والے، اہل اساتذہ جو ہر بچے کا نام جانتے ہیں — ریاضی، سائنس، انگریزی، آرٹ، موسیقی، کھیلیں اور کمپیوٹر۔",
      facilities: "لائبریری، سائنس و کمپیوٹر لیبز، کھیلوں کا میدان، آرٹ و موسیقی کے کمرے، سمارٹ کلاسز، کیفے اور محفوظ کھیل کا علاقہ۔",
      fallback: "میں اوقات، پروگرامز، داخلہ، اساتذہ، سہولیات، پورٹل — یا وزٹ بکنگ میں مدد کر سکتا ہوں۔ کیا پوچھنا ہے؟",
      askName: "زبردست — وزٹ بک کرتے ہیں۔ آپ کا نام کیا ہے؟",
      askEmail: "شکریہ، {name}۔ دفتر کے لیے بہترین ای میل کیا ہے؟",
      askWhen: "وزٹ کے لیے کون سا دن اور وقت ٹھیک ہے؟ (جیسے: جمعہ 10:00)",
      askAge: "بچے کی عمر کیا ہے؟ (یا کون سی جماعت چاہیے)",
      confirm:
        "یہ تفصیل ہے:\n• نام: {name}\n• ای میل: {email}\n• وقت: {when}\n• بچہ: {age}\n\nدفتر کو بھیج دوں؟ تصدیق کے لیے ہاں لکھیں۔",
      booked: "ہو گیا — وزٹ بک ہو گئی۔ ایڈمن اور سپر ایڈمن Meetings میں دیکھیں گے۔ خاندان سے ملنے کا انتظار ہے!",
      bookFail: "وزٹ محفوظ نہیں ہو سکی۔ دوبارہ لاگ اِن کر کے کوشش کریں، یا رابطہ فارم استعمال کریں۔",
      needLogin: "وزٹ تبھی بک ہوتی ہے جب آپ لاگ اِن ہوں۔ پہلے لاگ اِن یا رجسٹر کریں، پھر وزٹ بک کریں دبائیں۔",
      askWhenLoggedIn: "آپ {name} کے طور پر سائن اِن ہیں۔ وزٹ کے لیے کون سا دن اور وقت ٹھیک ہے؟ (جیسے: جمعہ 10:00)",
      timeTaken: "معذرت — اس وقت ({when}) پہلے سے ملاقات بک ہے۔ براہِ کرم دوسرا دن یا وقت چنیں۔",
      badEmail: "یہ ای میل درست نہیں لگتی۔ دوبارہ کوشش؟ (name@email.com)",
      cancel: "ٹھیک ہے — بکنگ منسوخ۔ کچھ اور پوچھیں، یا پھر سے وزٹ بک کریں۔",
      yesNeed: "بھیجنے کے لیے ہاں لکھیں، یا منسوخ کے لیے نہیں۔",
    },
    pa: {
      fab: "سکول بارے پُچھو",
      title: "کیمپس ڈیسک",
      sub: "لکھو یا بولو — اوسے زبان وچ جواب",
      close: "چیٹ بند کرو",
      placeholder: "اواریں، پروگرام، یا وزٹ بارے پُچھو…",
      send: "بھیجو",
      mic: "بولو",
      micOn: "سُن رہا واں…",
      greet: "ست سری اکال / السلام علیکم! میں Scuola Materna دا کیمپس ڈیسک واں۔ اواریں، پروگرام، داخلہ تے پورٹل دس سکدا واں — یا وزٹ بک کروا سکدا واں۔",
      chipHours: "اواریں",
      chipPrograms: "پروگرام",
      chipVisit: "وزٹ بک کرو",
      chipPortal: "پورٹل",
      chipLogin: "لاگ اِن",
      chipRegister: "رجسٹر",
      hours: "دفتر پیر توں جمعہ صبح 8:00 توں شام 4:00 کھلا۔ پتہ: 42 Maple Grove, Riverside۔ فون +1 (555) 214-8800۔",
      programs: "راستے: Early Learning، پرائمری، مڈل سکول، آرٹس، کھیڈاں، سائنس تے ٹیکنالوجی۔ ہر پاسے کھیل، تجسس تے مضبوط سکھیا۔",
      admissions: "داخلہ کیمپس وزٹ نال شروع ہوندا اے۔ کہو وزٹ بک کرنی اے — ناں، ای میل، دن/وقت تے بچے دی عمر پُچھاں گا۔",
      portal: "ماپے، استاد تے طالب علم Accedi / Student Portal توں لاگ اِن کرن۔ ڈیمو: student_demo، parent_demo، teacher_demo — پاس ورڈ Demo@12345۔",
      address: "پتہ: 42 Maple Grove, Riverside۔ ای میل hello@brightsteps.academy یا فون +1 (555) 214-8800۔",
      teachers: "پیار والے استاد جو ہر بچے دا ناں جاندے نے — ریاضی، سائنس، انگریزی، آرٹ، موسیقی، کھیڈاں تے کمپیوٹر۔",
      facilities: "لائبریری، سائنس تے کمپیوٹر لیبز، کھیڈاں دا میدان، آرٹ تے موسیقی دے کمرے، سمارٹ کلاس، کیفے تے محفوظ کھیڈ دا تھاں۔",
      fallback: "میں اواریں، پروگرام، داخلہ، استاد، سہولیات، پورٹل — یا وزٹ بکنگ وچ مدد کر سکدا واں۔ کی پُچھنا اے؟",
      askName: "چنگا — وزٹ بک کردے آں۔ تہاڈا ناں کی اے؟",
      askEmail: "شکریہ، {name}۔ دفتر لئی سب توں ودھیا ای میل کیہڑی اے؟",
      askWhen: "وزٹ لئی کیہڑا دن تے وقت ٹھیک اے؟ (جداں: جمعہ 10:00)",
      askAge: "بچے دی عمر کی اے؟ (یا کیہڑی جماعت چاہی دی اے)",
      confirm:
        "ایہ تفصیل اے:\n• ناں: {name}\n• ای میل: {email}\n• وقت: {when}\n• بچہ: {age}\n\nدفتر نوں بھیج دیاں؟ تصدیق لئی ہاں لکھو۔",
      booked: "ہو گیا — وزٹ بک ہو گئی۔ ایڈمن تے سپر ایڈمن Meetings وچ ویکھن گے۔ ٹبر نال ملن دا انتظار اے!",
      bookFail: "وزٹ محفوظ نہیں ہو سکی۔ فیر لاگ اِن کرکے کوشش کرو، یا رابطہ فارم ورتو۔",
      needLogin: "وزٹ تبھی بک ہوندی اے جدوں تسیں لاگ اِن ہوو۔ پہلے لاگ اِن یا رجسٹر کرو، فیر وزٹ بک کرو دباؤ۔",
      askWhenLoggedIn: "تسیں {name} وجوں سائن اِن او۔ وزٹ لئی کیہڑا دن تے وقت ٹھیک اے؟ (جداں: جمعہ 10:00)",
      timeTaken: "معذرت — اس ویلے ({when}) پہلے توں ملاقات بک اے۔ براہ کرم دوجا دن یا وقت چُنو۔",
      badEmail: "ایہ ای میل ٹھیک نہیں لگدی۔ فیر کوشش؟ (name@email.com)",
      cancel: "ٹھیک اے — بکنگ منسوخ۔ ہور پُچھو، یا فیر توں وزٹ بک کرو۔",
      yesNeed: "بھیجن لئی ہاں لکھو، یا منسوخ لئی نہیں۔",
    },
  };

  var chatLang = null;

  function siteLang() {
    if (window.ScuolaLang && typeof window.ScuolaLang.get === "function") {
      var l = window.ScuolaLang.get();
      if (l === "en" || l === "it") return l;
    }
    return document.documentElement.lang === "en" ? "en" : "it";
  }

  function activeLang() {
    return chatLang || siteLang();
  }

  function lang() {
    return activeLang();
  }

  function t() {
    return T[activeLang()] || T.en;
  }


  function detectMsgLang(raw) {
    var s = String(raw || "");
    if (/[\u0A00-\u0A7F]/.test(s)) return "pa";
    if (/[\u0600-\u06FF]/.test(s)) {
      if (/تسی|تسیں|اسیں|کیہ حال|کی حال اے|دسو|کیہڑا|کیہڑی|ویں/.test(s)) return "pa";
      return "ur";
    }
    var n = foldText(s);
    if (!n) return null;

    if (
      hasAny(n, [
        "tusi",
        "tussi",
        "assi ",
        "ki haal",
        "kihal",
        "punjabi",
        "panjabi",
        "menu daso",
        "daso ji",
        "ki daso",
        "ki ae",
        "fer ki",
        "oho ",
        "kimme",
        "kinne",
      ])
    )
      return "pa";

    if (
      hasAny(n, [
        "assalam",
        "salaam",
        "salam",
        "shukriya",
        "shukria",
        "mehrbani",
        "meherbani",
        "kya haal",
        "kitni",
        "kitna",
        "kitne",
        "kaise",
        "kaisi",
        "kahan",
        "kidhar",
        "batao",
        "bataye",
        "batana",
        "btana",
        "fees kitni",
        "fee kitni",
        "kitni fee",
        "kitni fees",
        "admission kaise",
        "dakhil",
        "dakhla",
        "mulakat",
        "mulaqat",
        "urdu",
        "kharcha",
        "khulta",
        "khulti",
        "khulty",
        "chahiye",
        "karna hai",
        "karni hai",
        "kya time",
        "school kab",
        "school ki ",
        "school ke ",
        "fees kya",
        "fee kya",
        "hai na",
        "yaar ",
      ])
    )
      return "ur";

    if (hasAny(n, ["ciao", "grazie", "prenot", "orari", "scuola", "bambino", "visita", "per favore"]))
      return "it";

    // Plain Latin (English questions, names with letters) → English so lang can switch back.
    if (/[a-z]/.test(n)) return "en";
    return null;
  }

  function noteUserLang(raw) {
    var d = detectMsgLang(raw);
    if (d) chatLang = d;
  }

  function fill(s, map) {
    return String(s).replace(/\{(\w+)\}/g, function (_, k) {
      return map[k] != null ? map[k] : "";
    });
  }

  /** Fold Latin + Urdu/Arabic letter variants so keys match real typing. */
  function foldText(s) {
    return String(s || "")
      .replace(/[\u064B-\u065F\u0670\u0640\u200C\u200D\uFEFF]/g, "")
      .replace(/[أإآٱ]/g, "ا")
      .replace(/[يىئ]/g, "ی")
      .replace(/ك/g, "ک")
      .replace(/[ۀہھ]/g, "ہ")
      .replace(/ة/g, "ہ")
      .replace(/ؤ/g, "و")
      .toLowerCase()
      .normalize("NFD")
      .replace(/[\u0300-\u036f]/g, "")
      .replace(/[’']/g, "'")
      .replace(/[؟?]/g, " ")
      .replace(/[،,!.:;]/g, " ")
      .replace(/\s+/g, " ")
      .trim();
  }

  function norm(s) {
    return foldText(s);
  }

  function hasAny(s, words) {
    return words.some(function (w) {
      var needle = foldText(w);
      return needle && s.indexOf(needle) !== -1;
    });
  }

  function isYes(s) {
    var n = foldText(s);
    return (
      /^(y|yes|ok|okay|sure|please|si|sì|certo|va bene|haan|ji|han|aaho|aho)\b/.test(n) ||
      n === "جی" ||
      n === "ہاں" ||
      n === "ہن" ||
      n.indexOf("ہاں") === 0 ||
      n.indexOf("جی ہاں") === 0 ||
      n.indexOf("جی") === 0
    );
  }

  function isNo(s) {
    var n = foldText(s);
    return (
      /^(n|no|nope|cancel|stop|annulla|basta|nahi|nahin|na)\b/.test(n) ||
      n.indexOf("نہیں") === 0 ||
      n.indexOf("نئیں") === 0 ||
      n === "نہیں"
    );
  }

  function isEmail(s) {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(s);
  }

  var FAQS = [
    { keys: ["hello","hi","hey","ciao","buongiorno","salve","سلام","السلام","آداب","کی حال","کیا حال","assalam","salam","kya haal","ki haal","السلام علیکم","سلام علیکم","اداب","sat sri","sasriakal","hello ji","ہیلو","هيلو","ہائے","ہائ","هاي","aoa","asalam","aslamo","hy","helo"], en: "Hi! I’m the campus desk. Ask about hours, programs, fees, uniform, buses — or say you want to book an appointment.", it: "Ciao! Sono lo sportello del campus. Chiedi orari, programmi, rette, divisa, pullman — o di’ che vuoi prenotare un appuntamento.", ur: "السلام علیکم! میں کیمپس ڈیسک ہوں۔ اوقات، پروگرامز، فیس، یونیفارم، بس — یا ملاقات بک کرنے کو کہیں۔", pa: "ست سری اکال! میں کیمپس ڈیسک واں۔ اواریں، پروگرام، فیس، یونیفارم، بس — یا ملاقات بک کرن لئی کہو۔" },
    { keys: ["thank","grazie","شکریہ","مہربانی","shukriya","mehrbani","shukria","dhanyavad","بہت شکریہ","thank you","thanks ji"], en: "You’re welcome. Anything else about the school?", it: "Prego. Altro sulla scuola?", ur: "خوشی ہوئی۔ اسکول کے بارے میں اور کچھ؟", pa: "کوئی گل نہیں۔ سکول بارے ہور کی؟" },
    { keys: ["who are you","your name","chi sei","آپ کون","تسی کون","تون کون","aap kaun","tusi kaun","تم کون ہو","آپ کون ہو","یہ بوٹ"], en: "I’m the Scuola Materna campus assistant. I answer school questions and can book a visit.", it: "Sono l’assistente della Scuola Materna. Rispondo alle domande e posso prenotare una visita.", ur: "میں Scuola Materna کا اسسٹنٹ ہوں۔ اسکول کے سوالات کے جواب دیتا ہوں اور وزٹ بک کروا سکتا ہوں۔", pa: "میں Scuola Materna دا اسسٹنٹ واں۔ سکول دے سوالاں دے جواب دیندا واں تے وزٹ بک کروا سکدا واں۔" },
    { keys: ["help","aiuto","what can you","مدد","کیا کر سکتے","کی کر سکدے","madad","help urdu","کیا مدد","madad chahiye","help chahiye"], en: "Ask me about hours, programs, admissions, fees, uniform, lunch, buses, teachers, class size, holidays, the parent portal, or booking a visit.", it: "Chiedimi orari, programmi, iscrizioni, rette, divisa, mensa, pullman, insegnanti, numero in classe, vacanze, portale genitori o una visita.", ur: "اوقات، پروگرامز، داخلہ، فیس، یونیفارم، لنچ، بس، اساتذہ، کلاس سائز، چھٹیاں، والدین پورٹل، یا وزٹ بکنگ پوچھیں۔", pa: "اواریں، پروگرام، داخلہ، فیس، یونیفارم، لنچ، بس، استاد، کلاس سائز، چھٹیاں، والدین پورٹل، یا وزٹ بکنگ پُچھو۔" },
    { keys: ["weekend","saturday","sunday","sabato","domenica","ہفتہ","اتوار","weekend urdu","hafta","itwar","ہفتے کو","اتوار کو","weekend band"], en: "The office is closed Saturday and Sunday. We open again Monday at 8:00 AM.", it: "La segreteria è chiusa sabato e domenica. Riapriamo lunedì alle 8:00.", ur: "دفتر ہفتہ اور اتوار بند رہتا ہے۔ پیر صبح 8:00 بجے کھلتا ہے۔", pa: "دفتر ہفتہ تے اتوار بند رہندا اے۔ پیر صبح 8:00 کھلدا اے۔" },
    { keys: ["hour","open","orari","apert","closing","اوقات","کھلا","کب کھلتا","اواریں","kitne baje","timing","timings","kab khulta","اوقات کیا","وقت کیا","کب کھلتی","کب کھلتے","کھلتا ہے","کھلتی ہے","ٹائمنگ","ٹائمنگز","وقت بتاؤ","اوقات بتاؤ","اوقات بتائیں","school timing","school timings","timing batao","timings batao","kab khulti","kab khulte","khulta hai","khulti hai","office hours","school hours","kya time","kitnay baje"], en: "Office hours: Monday–Friday, 8:00 AM – 4:00 PM at 42 Maple Grove, Riverside. Phone +1 (555) 214-8800.", it: "Orari: lunedì–venerdì 8:00–16:00, 42 Maple Grove, Riverside. Tel. +1 (555) 214-8800.", ur: "دفتر کے اوقات: پیر تا جمعہ، 8:00 صبح – 4:00 شام، 42 Maple Grove, Riverside۔ فون +1 (555) 214-8800۔", pa: "دفتر دیاں اواریں: پیر توں جمعہ، 8:00 صبح – 4:00 شام، 42 Maple Grove, Riverside۔ فون +1 (555) 214-8800۔" },
    { keys: ["early learning","nursery","infanzia","preschool","kindergarten","نرسری","پری اسکول","ابتدائی","preschool urdu","ابتدائی تعلیم","nursery class"], en: "Early Learning is play-based discovery for our youngest children — curiosity, stories and gentle routines.", it: "Prima infanzia è un percorso per i più piccoli: gioco, storie e routine delicate.", ur: "Early Learning چھوٹے بچوں کے لیے کھیل پر مبنی سیکھنا ہے — کہانیاں اور نرم روٹین۔", pa: "Early Learning ننھے بچیاں لئی کھیڈ نال سکھنا اے — کہانیاں تے نرم روٹین۔" },
    { keys: ["primary","elementar","primaria","پرائمری","پرائمری اسکول","primary urdu","پرائمری کلاس","primary class"], en: "Primary School builds strong reading, maths and kindness — the foundations for lifelong learning.", it: "La scuola primaria costruisce lettura, matematica e gentilezza — basi per tutta la vita.", ur: "پرائمری میں مضبوط پڑھائی، ریاضی اور مہربانی سکھائی جاتی ہے — زندگی بھر کی بنیاد۔", pa: "پرائمری وچ مضبوط پڑھائی، ریاضی تے مہربانی سکھائی جاندی اے — زندگی بھر دی بنیاد۔" },
    { keys: ["middle school","scuola media","مڈل","مڈل اسکول","middle urdu","مڈل کلاس"], en: "Middle School grows independence, confidence and deeper subject knowledge.", it: "La scuola media sviluppa indipendenza, fiducia e materie più approfondite.", ur: "مڈل اسکول میں آزادی، اعتماد اور گہری مضامین کی سمجھ بڑھتی ہے۔", pa: "مڈل سکول وچ آزادی، اعتماد تے مضامین دی گہری سمجھ ودھدی اے۔" },
    { keys: ["program","curric","pathway","percors","materie","پروگرام","نصاب","راستہ","program urdu","nisab","کون سے پروگرام","کیا پڑھاتے","subjects","کورس"], en: "Pathways: Early Learning, Primary, Middle School, Creative Arts, Sports, and Science & Technology.", it: "Percorsi: Prima infanzia, Scuola primaria, Scuola media, Arti creative, Sport, Scienza e tecnologia.", ur: "راستے: Early Learning، پرائمری، مڈل اسکول، آرٹس، اسپورٹس، سائنس و ٹیکنالوجی۔", pa: "راستے: Early Learning، پرائمری، مڈل سکول، آرٹس، کھیڈاں، سائنس تے ٹیکنالوجی۔" },
    { keys: ["art","arte","music","musica","آرٹ","موسیقی","فنون","آرٹ کلاس","ڈرائنگ"], en: "Art and music studios run all week. Children paint, sing, and perform at the annual show.", it: "Sale arte e musica tutta la settimana. I bambini dipingono, cantano e si esibiscono al saggio.", ur: "آرٹ اور موسیقی پورے ہفتے چلتے ہیں۔ بچے پینٹ کرتے، گاتے اور سالانہ شو میں پرفارم کرتے ہیں۔", pa: "آرٹ تے موسیقی سارے ہفتے چلدے نے۔ بچے پینٹ کردے، گاندے تے سالانہ شو وچ پرفارم کردے نے۔" },
    { keys: ["sport","pe","educazione fisica","football","soccer","کھیل","کھیڈ","اسپورٹ","football urdu","کھیلیں","sports day"], en: "Sports include PE, house races and Sports Day on the field. We mix teamwork with lots of orange slices.", it: "Sport: educazione fisica, gare delle case e Giornata dello sport. Tanto gioco di squadra.", ur: "کھیل: پی ای، ہاؤس ریسز اور اسپورٹس ڈے میدان پر۔ ٹیم ورک اور مزہ۔", pa: "کھیڈاں: پی ای، ہاؤس ریس تے اسپورٹس ڈے میدان تے۔ ٹیم ورک تے مزہ۔" },
    { keys: ["stem","science","scienz","computer","coding","lab","سائنس","کمپیوٹر","کوڈنگ","لیب","سائنس لیب","کمپیوٹر لیب"], en: "Science and computer labs are hands-on: experiments, coding as a creative language, and curious questions.", it: "Laboratori di scienze e informatica: esperimenti, coding creativo e tante domande.", ur: "سائنس اور کمپیوٹر لیبز عملی ہیں: تجربے، کوڈنگ اور تجسس۔", pa: "سائنس تے کمپیوٹر لیبز عملی نے: تجربے، کوڈنگ تے تجسس۔" },
    { keys: ["admiss","enroll","iscriz","apply","application","داخلہ","داخلے","داخلہ کیسے","admission","dakhil","dakhla","enroll urdu","داخلہ کب","داخلہ کرنا","داخل کرانا","ایڈمیشن","ایڈمیشن کیسے","admission kaise","admission kab","naya bacha","form kab"], en: "Admissions start with a campus visit. Say “book an appointment” and I’ll take your name, email, day/time and your child’s age.", it: "Le iscrizioni iniziano con una visita. Di’ “prenota un appuntamento” e chiederò nome, email, giorno/orario ed età.", ur: "داخلہ کیمپس وزٹ سے شروع ہوتا ہے۔ کہیں “ملاقات بک کریں” — نام، ای میل، دن/وقت اور عمر لوں گا۔", pa: "داخلہ کیمپس وزٹ نال شروع ہوندا اے۔ کہو “ملاقات بک کرو” — ناں، ای میل، دن/وقت تے عمر لاں گا۔" },
    { keys: ["fee","tuition","cost","price","retta","quanto costa","فیس","کھرچہ","کتنی فیس","kitni fee","fees kitni","tuition urdu","فیس کتنی","فیس کیا","ماہانہ فیس","ٹیوشن","خرچہ","کیا فیس","fee kitni","kitni fees","fees kya","fee kya","monthly fee","tuition fee","fees batao","fee batao","kharcha"], en: "Tuition is explained in person on a visit so we can match the right pathway. Book an appointment and the office will walk you through fees.", it: "Le rette si spiegano in visita, in base al percorso. Prenota un appuntamento e la segreteria ti guida.", ur: "فیس وزٹ پر ذاتی طور پر بتائی جاتی ہے تاکہ درست راستہ ملے۔ ملاقات بک کریں، دفتر فیس سمجھا دے گا۔", pa: "فیس وزٹ تے ذاتی طور تے دسدی اے تاکہ درست راستہ ملے۔ ملاقات بک کرو، دفتر فیس سمجھا دیوے گا۔" },
    { keys: ["how old","age","eta","età","years old","عمر","سال کا","کتنے سال","umr","kitne saal","عمر کیا","کتنے سال کا","age limit"], en: "We welcome children from early years through middle school. Tell me your child’s age when you book a visit.", it: "Accogliamo dalla prima infanzia alla scuola media. Dimmi l’età del bambino quando prenoti.", ur: "ہم ابتدائی سالوں سے مڈل اسکول تک بچوں کا استقبال کرتے ہیں۔ وزٹ بک کرتے وقت عمر بتائیں۔", pa: "اسیں ابتدائی سالاں توں مڈل سکول تک بچیاں دا استقبال کردے آں۔ وزٹ بک کردیاں عمر دسو۔" },
    { keys: ["uniform","divisa","یونیفارم","ڈریس","یونفورم","uniform urdu","یونیفارم ہے","ڈریس کوڈ","uniform hai","dress code"], en: "Yes — a simple school uniform. The office shares the list at your visit.", it: "Sì, c’è una divisa semplice. La lista si riceve in visita.", ur: "جی ہاں — سادہ اسکول یونیفارم۔ فہرست وزٹ پر ملتی ہے۔", pa: "ہاں — سادہ سکول یونیفارم۔ لسٹ وزٹ تے ملدی اے۔" },
    { keys: ["lunch","canteen","mensa","food","cafeteria","لنچ","کھانا","کیفے","khana","کھانا ملتا","لنچ ہوتا","tiffin"], en: "The cafeteria is a comfortable dining space. Packed lunches are welcome too.", it: "La mensa è un spazio comodo. Si può portare anche il pranzo da casa.", ur: "کیفے آرام دہ ہے۔ گھر سے لنچ بھی لا سکتے ہیں۔", pa: "کیفے آرام دہ اے۔ گھر توں لنچ وی لیا جا سکدا اے۔" },
    { keys: ["bus","transport","pullman","scuolabus","بس","ٹرانسپورٹ","سکول بس","bus urdu","بس ملتی","pick and drop","school van"], en: "Safe school transport is available. Ask the office during your visit for routes.", it: "C’è un trasporto scolastico sicuro. I percorsi si vedono in segreteria.", ur: "محفوظ اسکول ٹرانسپورٹ دستیاب ہے۔ راستے وزٹ پر دفتر سے پوچھیں۔", pa: "محفوظ سکول ٹرانسپورٹ دستیاب اے۔ رستے وزٹ تے دفتر توں پُچھو۔" },
    { keys: ["address","where","map","indirizzo","location","dove siete","پتہ","کہاں","لوکیشن","address urdu","kahan","kidhar","پتہ کیا","کہاں واقع","لوکیشن کیا","اسکول کہاں","address kya","school kahan","kahan hai","location kya"], en: "42 Maple Grove, Riverside. Email hello@brightsteps.academy or call +1 (555) 214-8800.", it: "42 Maple Grove, Riverside. Email hello@brightsteps.academy — tel. +1 (555) 214-8800.", ur: "42 Maple Grove, Riverside۔ ای میل hello@brightsteps.academy یا فون +1 (555) 214-8800۔", pa: "42 Maple Grove, Riverside۔ ای میل hello@brightsteps.academy یا فون +1 (555) 214-8800۔" },
    { keys: ["phone","call","telefon","number","فون","نمبر","کال","phone urdu","فون نمبر","رابطہ نمبر","contact number","phone number","call karo"], en: "Call the office on +1 (555) 214-8800, Monday–Friday 8:00 AM – 4:00 PM.", it: "Tel. +1 (555) 214-8800, lunedì–venerdì 8:00–16:00.", ur: "دفتر کو کال کریں: +1 (555) 214-8800، پیر تا جمعہ 8:00–4:00۔", pa: "دفتر نوں کال کرو: +1 (555) 214-8800، پیر توں جمعہ 8:00–4:00۔" },
    { keys: ["email","write","scriv","ای میل","لکھیں","email urdu","ای میل کیا","email kya"], en: "Email hello@brightsteps.academy — or use the contact form on this site.", it: "Email hello@brightsteps.academy — oppure il modulo Contatti sul sito.", ur: "ای میل hello@brightsteps.academy — یا اس سائٹ کا رابطہ فارم۔", pa: "ای میل hello@brightsteps.academy — یا اس سائٹ دا رابطہ فارم۔" },
    { keys: ["teacher","insegn","staff","faculty","استاد","استانی","ٹیچر","ustad","teacher urdu","استاد کون","ٹیچرز","teachers kaise"], en: "Warm, qualified teachers who know every child by name — maths, science, English, art, music, PE and computing.", it: "Insegnanti preparati che conoscono ogni bambino per nome — matematica, scienze, inglese, arte, musica, motoria e informatica.", ur: "محبت کرنے والے، اہل اساتذہ جو ہر بچے کا نام جانتے ہیں — ریاضی، سائنس، انگریزی، آرٹ، موسیقی، کھیلیں، کمپیوٹر۔", pa: "پیار والے استاد جو ہر بچے دا ناں جاندے نے — ریاضی، سائنس، انگریزی، آرٹ، موسیقی، کھیڈاں، کمپیوٹر۔" },
    { keys: ["class size","how many children","quanti bambini","ratio","کلاس سائز","کتنے بچے","kitne bachay","کلاس میں کتنے","کتنے بچے کلاس"], en: "Classes stay small enough that teachers know every child. Ask on your visit for the current year-group sizes.", it: "Classi contenute: gli insegnanti conoscono ogni bambino. I numeri precisi si vedono in visita.", ur: "کلاسز اتنی چھوٹی رہتی ہیں کہ استاد ہر بچے کو جانتے ہیں۔ موجودہ سائز وزٹ پر پوچھیں۔", pa: "کلاسز اتنی چھوٹیاں رہندیاں نے کہ استاد ہر بچے نوں جاندے نے۔ موجودہ سائز وزٹ تے پُچھو۔" },
    { keys: ["language","english","italian","italiano","inglese","bilingual","اردو","پنجابی","urdu","punjabi","panjabi","اردو","پنجابی","زبان","urdu","punjabi","panjabi","zaban","اردو بولتے","پنجابی بولتے","urdu mein","punjabi mein"], en: "The site switches English and Italian with EN | IT. In this chat I also understand and reply in Urdu and Punjabi — just write or speak.", it: "Il sito passa da inglese a italiano con EN | IT. In chat capisco e rispondo anche in urdu e punjabi — scrivi o parla pure.", ur: "سائٹ EN | IT سے انگریزی/اطالوی بدل سکتی ہے۔ چیٹ میں اردو اور پنجابی بھی سمجھتا اور جواب دیتا ہوں۔", pa: "سائٹ EN | IT نال انگریزی/اطالوی بدل سکدی اے۔ چیٹ وچ اردو تے پنجابی وی سمجھدا تے جواب دیندا واں۔" },
    { keys: ["portal","login","acced","password","demo","پورٹل","لاگ ان","پاس ورڈ","login urdu","پورٹل کیسے","لاگ ان کیسے","login kaise"], en: "Parents, teachers and students sign in from Accedi. Demo: student_demo, parent_demo, teacher_demo — password Demo@12345. You can also register.", it: "Si accede da Accedi. Demo: student_demo, parent_demo, teacher_demo — password Demo@12345. Puoi anche registrarti.", ur: "Accedi سے لاگ اِن۔ ڈیمو: student_demo، parent_demo، teacher_demo — پاس ورڈ Demo@12345۔ رجسٹر بھی کر سکتے ہیں۔", pa: "Accedi توں لاگ اِن۔ ڈیمو: student_demo، parent_demo، teacher_demo — پاس ورڈ Demo@12345۔ رجسٹر وی کر سکدے او۔" },
    { keys: ["parent portal","portale genitori","والدین پورٹل","پیرنٹ پورٹل","پیرنٹس پورٹل"], en: "The parent portal shows diary, attendance and announcements for your child.", it: "Il portale genitori mostra diario, presenze e avvisi.", ur: "والدین پورٹل میں ڈائری، حاضری اور اعلانات ملتے ہیں۔", pa: "والدین پورٹل وچ ڈائری، حاضری تے اعلانات ملدے نے۔" },
    { keys: ["student portal","portale student","طالب علم پورٹل","سٹوڈنٹ پورٹل","سٹوڈنٹس پورٹل"], en: "The student portal has timetable, assignments and marks.", it: "Il portale studenti ha orario, compiti e voti.", ur: "طالب علم پورٹل میں ٹائم ٹیبل، ہوم ورک اور نمبر ہیں۔", pa: "طالب علم پورٹل وچ ٹائم ٹیبل، ہوم ورک تے نمبر نے۔" },
    { keys: ["homework","compiti","assignment","ہوم ورک","کام","اسائنمنٹ","homework urdu","ہوم ورک ہوتا","homework hota"], en: "Homework is short and meaningful. Parents see it in the diary on the portal.", it: "I compiti sono brevi e utili. I genitori li vedono nel diario del portale.", ur: "ہوم ورک مختصر اور مفید ہے۔ والدین اسے پورٹل ڈائری میں دیکھتے ہیں۔", pa: "ہوم ورک مختصر تے فائدے مند اے۔ ماپے اسنوں پورٹل ڈائری وچ ویکھدے نے۔" },
    { keys: ["holiday","vacation","vacanz","break","closed","چھٹی","تعطیل","چھٹیاں","chhutti","holiday urdu","چھٹیاں کب","chuttiyan","holiday kab"], en: "Term dates and holidays are posted on the events page and in the parent portal.", it: "Calendario e vacanze sono in Eventi e nel portale genitori.", ur: "ٹرم اور چھٹیاں ایونٹس صفحے اور والدین پورٹل پر ہیں۔", pa: "ٹرم تے چھٹیاں ایونٹس صفحے تے والدین پورٹل تے۔" },
    { keys: ["sick","ill","malat","fever","assent","بیمار","بخار","غیر حاضر","bimar","bukhar","بیمار ہو","بخار ہو"], en: "If a child is unwell, keep them home and message the office. We log attendance on the portal.", it: "Se il bambino sta male, resti a casa e avvisa la segreteria. Le assenze sono nel portale.", ur: "اگر بچہ بیمار ہو تو گھر رکھیں اور دفتر کو پیغام دیں۔ حاضری پورٹل پر لکھی جاتی ہے۔", pa: "جے بچہ بیمار ہووے تاں گھر رکھو تے دفتر نوں پیغام دیو۔ حاضری پورٹل تے لکھی جاندی اے۔" },
    { keys: ["safe","security","sicur","bully","محفوظ","سیکیورٹی","محفوظی","safe urdu","سیکیورٹی کیسی","محفوظ ہے","safety"], en: "The campus is secure, with caring staff and clear routines. Kindness is part of the code.", it: "Campus sicuro, personale attento e routine chiare. La gentilezza fa parte delle regole.", ur: "کیمپس محفوظ ہے، عملہ خیال رکھتا ہے اور روٹین واضح ہیں۔ مہربانی ہمارا اصول ہے۔", pa: "کیمپس محفوظ اے، عملہ خیال رکھدا اے تے روٹین واضح اے۔ مہربانی ساڈا اصول اے۔" },
    { keys: ["playground","play area","cortile","giochi","پلے گراؤنڈ","کھیل کا میدان","کھیڈ دا تھاں","کھیلنے کی جگہ"], en: "A safe play area sits beside the fields — space to run, climb and make friends.", it: "Area giochi sicura accanto ai campi — si corre, si gioca, si fanno amici.", ur: "محفوظ کھیل کا علاقہ میدان کے پاس ہے — دوڑ، کھیل اور دوستی۔", pa: "محفوظ کھیڈ دا تھاں میدان کول اے — دوڑ، کھیڈ تے دوستی۔" },
    { keys: ["library","bibliotec","books","لائبریری","کتابیں","کتاباں","لائبریری ہے","کتب خانہ"], en: "The modern library has books, quiet corners and reading week every year.", it: "Biblioteca moderna: libri, angoli silenziosi e la settimana della lettura.", ur: "جدید لائبریری: کتابیں، خاموش کونے اور ہر سال ریڈنگ ویک۔", pa: "جدید لائبریری: کتاباں، خاموش کونے تے ہر سال ریڈنگ ویک۔" },
    { keys: ["after school","aftercare","doposcuola","club","افٹر سکول","کلب","ڈوپو","اسکول کے بعد"], en: "Clubs and after-school fun run beyond the bell — art, sport, reading and science.", it: "Doposcuola e club oltre l’orario: arte, sport, lettura e scienze.", ur: "اسکول کے بعد کلب: آرٹ، کھیل، پڑھائی اور سائنس۔", pa: "سکول بعد کلب: آرٹ، کھیڈ، پڑھائی تے سائنس۔" },
    { keys: ["event","calendar","calendario","ایونٹ","کیلنڈر","تقریب","ایونٹس","تقریبات"], en: "Sports days, exhibitions, parent meetings and the annual show are on the Events page.", it: "Giornate sportive, mostre, colloqui e saggio annuale sono in Eventi.", ur: "اسپورٹس ڈے، نمائشیں، والدین ملاقاتیں اور سالانہ شو ایونٹس صفحے پر ہیں۔", pa: "اسپورٹس ڈے، نمائشاں، والدین ملاقاتاں تے سالانہ شو ایونٹس صفحے تے۔" },
    { keys: ["sports day","giornata dello sport","اسپورٹس ڈے","کھیلوں کا دن","کھیڈاں دا دن"], en: "Sports Day is on the field — races, relays and house cheers. Next one is marked on the events posters.", it: "La Giornata dello sport è in campo: gare, staffette e tifo. La data è sui manifesti Eventi.", ur: "اسپورٹس ڈے میدان پر — دوڑیں، ریلے اور ہاؤس کا جوش۔ تاریخ ایونٹس پوسٹرز پر ہے۔", pa: "اسپورٹس ڈے میدان تے — دوڑاں، ریلے تے ہاؤس دا جوش۔ تاریخ ایونٹس پوسٹرز تے۔" },
    { keys: ["park","parking","پارکنگ","گاڑی","پارکنگ ہے"], en: "Short-stay parking is on Maple Grove in front of the gate. Please keep the bus lane clear.", it: "Parcheggio breve in Maple Grove davanti al cancello. Lascia libera la corsia del pullman.", ur: "مختصر پارکنگ Maple Grove پر گیٹ کے سامنے۔ بس لین خالی رکھیں۔", pa: "مختصر پارکنگ Maple Grove تے گیٹ دے ساہمنے۔ بس لین خالی رکھو۔" },
    { keys: ["special need","sen","inclus","disability","bes","خصوصی ضرورت","شمولیت","special need urdu","خصوصی بچے"], en: "We plan support with families. Mention any needs when you book a visit so the right teacher can join.", it: "Progettiamo il supporto con le famiglie. Segnala le esigenze in visita così c’è l’insegnante giusto.", ur: "ہم خاندانوں کے ساتھ سپورٹ پلان کرتے ہیں۔ وزٹ بک کرتے وقت ضروریات بتائیں تاکہ درست استاد شامل ہو۔", pa: "اسیں ٹبراں نال سپورٹ پلان کردے آں۔ وزٹ بک کردیاں لوڑاں دسو تاکہ درست استاد شامل ہووے۔" },
    { keys: ["vaccine","vaccin","health","nurse","infermer","ویکسین","نرس","صحت"], en: "A nurse / first-aid room supports student health. Immunisation records are requested at admission.", it: "C’è un’infermeria. In iscrizione chiediamo i certificati vaccinali.", ur: "نرس / فرسٹ ایڈ روم صحت کی دیکھ بھال کرتا ہے۔ داخلے پر ویکسینیشن ریکارڈ مانگا جاتا ہے۔", pa: "نرس / فرسٹ ایڈ روم صحت دی دیکھ بھال کردا اے۔ داخلے تے ویکسینیشن ریکارڈ منگیا جاندا اے۔" },
    { keys: ["start","term","when does school","inizio","شروع","ٹرم","سکول کب","سکول کب شروع","session kab"], en: "The school year follows the published calendar. A visit is the easiest way to lock a start date.", it: "L’anno segue il calendario pubblicato. Una visita è il modo più semplice per fissare l’inizio.", ur: "سکول سال شائع شدہ کیلنڈر پر چلتا ہے۔ آغاز کی تاریخ کے لیے وزٹ سب سے آسان ہے۔", pa: "سکول سال شائع شدہ کیلنڈر تے چلدا اے۔ شروعات دی تاریخ لئی وزٹ سب توں آسان اے۔" },
    { keys: ["supply","stationery","material","quadern","سامان","اسٹیشنری","کاپی","کتابیں کیا","book list"], en: "A simple supplies list is given at enrolment. The school provides a lot of shared classroom material.", it: "Una lista semplice si riceve all’iscrizione. Molto materiale è condiviso in classe.", ur: "سادہ سامان کی فہرست داخلے پر ملتی ہے۔ بہت سا شیئرڈ کلاس میٹریل اسکول دیتا ہے۔", pa: "سادہ سامان دی لسٹ داخلے تے ملدی اے۔ بہت سارا شیئرڈ کلاس میٹریل سکول دیندا اے۔" },
    { keys: ["photo","gallery","galleria","تصویر","گیلری","فوٹو","تصاویر"], en: "The gallery page is a scrapbook of campus life — click a photo to open it.", it: "La galleria è uno scrapbook del campus — clicca una foto per aprirla.", ur: "گیلری صفحہ کیمپس کی زندگی کا البم ہے — تصویر کھولنے کے لیے کلک کریں۔", pa: "گیلری صفحہ کیمپس دی زندگی دا البم اے — تصویر کھولن لئی کلک کرو۔" },
    { keys: ["contact form","message","modulo","رابطہ فارم","میسج","پیغام","رابطہ کریں","میسج بھیجیں"], en: "The contact form on this page reaches the office. For a timed visit, say you want to book an appointment here in chat.", it: "Il modulo Contatti arriva in segreteria. Per una visita ad orario, prenota qui in chat.", ur: "اس صفحے کا رابطہ فارم دفتر تک پہنچتا ہے۔ وقت والی وزٹ کے لیے یہاں چیٹ میں ملاقات بک کریں۔", pa: "اس صفحے دا رابطہ فارم دفتر تک پہنچدا اے۔ وقت والی وزٹ لئی ایتھے چیٹ وچ ملاقات بک کرو۔" },
    { keys: ["headmaster","principal","preside","head teacher","پرنسپل","ہیڈ مسٹر","ہیڈ مسٹریس","پرنسپل کون","principal kaun"], en: "Our headmaster, Grace Okonkwo, oversees campus life. You can meet leadership on a booked visit.", it: "La preside Grace Okonkwo guida il campus. Si può incontrare la dirigenza in visita.", ur: "ہماری ہیڈ مسٹریس Grace Okonkwo کیمپس چلاتی ہیں۔ قیادت سے ملاقات بک شدہ وزٹ پر ہو سکتی ہے۔", pa: "ساڈی ہیڈ مسٹریس Grace Okonkwo کیمپس چلاندی اے۔ قیادت نال ملاقات بک شدہ وزٹ تے ہو سکدی اے۔" },
    { keys: ["birthday","compleanno","سالگرہ","جنم دن","birthday urdu","سالگرہ مناتے"], en: "Class birthdays are low-key and kind. Tell the teacher a week ahead if you’d like to share a treat.", it: "I compleanni in classe sono semplici. Avvisa l’insegnante una settimana prima se porti qualcosa.", ur: "کلاس میں جنم دن سادہ اور پیارے ہوتے ہیں۔ اگر ٹریٹ لانا ہو تو ایک ہفتہ پہلے استاد کو بتائیں۔", pa: "کلاس وچ جنم دن سادے تے پیارے ہوندے نے۔ جے ٹریٹ لیانا ہووے تاں اک ہفتہ پہلے استاد نوں دسو۔" },
    { keys: ["weather","snow","close","chiusura","موسم","برف","بند","موسم خراب","سکول بند"], en: "Weather closures are posted on the parent portal and by email/SMS from the office.", it: "Le chiusure per maltempo arrivano sul portale e via email/SMS dalla segreteria.", ur: "موسم کی بندش والدین پورٹل اور دفتر کے ای میل/SMS پر آتی ہے۔", pa: "موسم دی بندش والدین پورٹل تے دفتر دے ای میل/SMS تے آوندی اے۔" },
    { keys: ["pta","parent teacher","colloqui","والدین ملاقات","پی ٹی اے","colloqui urdu","میٹنگ کب","parents meeting"], en: "Parent-teacher meetings are on the calendar. You can also request a conversation any week through the office.", it: "I colloqui sono in calendario. Si può anche chiedere un incontro in qualsiasi settimana.", ur: "والدین-استاد ملاقاتیں کیلنڈر پر ہیں۔ کسی بھی ہفتے دفتر سے بات بھی مانگ سکتے ہیں۔", pa: "ماپے-استاد ملاقاتاں کیلنڈر تے۔ کسے وی ہفتے دفتر توں گل وی منگ سکدے او۔" },
    { keys: ["religion","faith","relig","مذہب","دین","عقیدہ","مذہبی"], en: "We welcome every family. Celebrations are cultural and kind, never exclusive.", it: "Ogni famiglia è la benvenuta. Le feste sono culturali e inclusive.", ur: "ہر خاندان کا خیرمقدم ہے۔ تقریبات ثقافتی اور شامل ہیں، کبھی الگ تھلگ نہیں۔", pa: "ہر ٹبر دا خیرمقدم اے۔ تقریبات ثقافتی تے شامل نے، کدی الگ تھلگ نہیں۔" },
    { keys: ["waitlist","waiting list","lista d'attesa","ویٹ لسٹ","انتظار کی فہرست","سیٹ خالی","waiting"], en: "If a year group is full we keep a waitlist. Book a visit anyway so we know your child’s age.", it: "Se la classe è piena teniamo una lista d’attesa. Prenota comunque una visita e indica l’età.", ur: "اگر جماعت بھری ہو تو ویٹ لسٹ رکھتے ہیں۔ پھر بھی وزٹ بک کریں اور عمر بتائیں۔", pa: "جے جماعت بھری ہووے تاں ویٹ لسٹ رکھدے آں۔ فیر وی وزٹ بک کرو تے عمر دسو۔" },
    { keys: ["tour","open day","ٹور","اوپن ڈے","کیمپس دیکھیں","campus dekho","کیمپس دیکھنا","ٹور کرنا","school dekhna"], en: "Campus tours are by appointment. Say “I want to book an appointment” and I’ll collect your details.", it: "Le visite sono su appuntamento. Di’ “voglio prenotare” e raccolgo i dati.", ur: "کیمپس ٹور اپائنٹمنٹ پر۔ کہیں “ملاقات بک کرنی ہے” اور تفصیل لوں گا۔", pa: "کیمپس ٹور اپائنٹمنٹ تے۔ کہو “ملاقات بک کرنی اے” تے تفصیل لاں گا۔" },
  ];


  function wantsBook(s) {
    var n = foldText(s);
    return hasAny(n, [
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
      "ملاقات",
      "وزٹ بک",
      "وزٹ کرنی",
      "وزٹ کرنا",
      "اپائنٹمنٹ",
      "ملاقات بک",
      "ملاقات کرنی",
      "ملاقات کرنا",
      "ملنا ہے",
      "ملنا اے",
      "وزٹ بک کرو",
      "وزٹ بک کریں",
      "mulakat",
      "mulaqat",
      "visit book",
      "book visit",
      "appointment book",
      "visit karni",
      "visit karna",
      "milna hai",
      "dekhna hai",
      "campus visit",
    ]);
  }

  var SOFT_INTENTS = {
    hello: true,
    thank: true,
    help: true,
  };

  function softIntent(faq) {
    var k0 = (faq.keys && faq.keys[0]) || "";
    return !!SOFT_INTENTS[k0];
  }

  function scoreFaq(faq, folded) {
    var best = 0;
    var i;
    for (i = 0; i < faq.keys.length; i++) {
      var key = foldText(faq.keys[i]);
      if (!key || key.length < 2) continue;
      if (/^[a-z]{1,2}$/.test(key)) {
        var re = new RegExp("(?:^|\\s)" + key + "(?:$|\\s|[!?.,])");
        if (!re.test(folded)) continue;
        if (softIntent(faq) && folded.length > 18) continue;
      }
      if (folded.indexOf(key) === -1) continue;
      var score = key.length * 4;
      if (key.indexOf(" ") !== -1) score += 10;
      if (/[\u0600-\u06FF]/.test(key)) score += 8;
      if (softIntent(faq)) {
        if (folded.length > 22) score = Math.min(score, 6);
        else score += 12;
      } else {
        score += 15;
      }
      if (score > best) best = score;
    }
    return best;
  }

  function intentReply(text) {
    noteUserLang(text);
    var folded = foldText(text);
    if (wantsBook(folded)) return null;
    var code = activeLang();

    // Short hellos like "ہیلو" / "hello" must hit greeting, not the fallback.
    var greetKeys = ["hello", "hi", "hey", "ہیلو", "هيلو", "ہائے", "سلام", "السلام علیکم", "ciao", "salam", "hy", "helo"];
    if (folded.length <= 18 && hasAny(folded, greetKeys)) {
      return FAQS[0][code] || FAQS[0].en;
    }

    var bestFaq = null;
    var bestScore = 0;
    var i;
    for (i = 0; i < FAQS.length; i++) {
      var score = scoreFaq(FAQS[i], folded);
      if (score > bestScore) {
        bestScore = score;
        bestFaq = FAQS[i];
      }
    }
    if (bestFaq && bestScore >= 8) return bestFaq[code] || bestFaq.en;
    return t().fallback;
  }

  var booking = { step: "", name: "", email: "", when: "", age: "" };
  var rec = null;
  var voiceTurn = false;
  var voicesReady = false;
  var speechUnlocked = false;
  var speakTimer = null;
  var VISITS_KEY = "brightsteps-demo-visits";

  function currentSession() {
    try {
      if (window.BrightStepsDemoAuth && typeof window.BrightStepsDemoAuth.getSession === "function") {
        return window.BrightStepsDemoAuth.getSession();
      }
    } catch (e) {}
    if (window.BrightStepsUser && window.BrightStepsUser.name) return window.BrightStepsUser;
    try {
      var raw = sessionStorage.getItem("brightsteps-demo-session") || localStorage.getItem("brightsteps-demo-session");
      return raw ? JSON.parse(raw) : null;
    } catch (e2) {
      return null;
    }
  }

  function authPaths() {
    if (window.BrightStepsDemoAuth && window.BrightStepsDemoAuth.paths) return window.BrightStepsDemoAuth.paths;
    if (/\/demos\/brightsteps\//.test(location.pathname || "")) {
      return {
        login: "/demos/brightsteps/login.html",
        register: "/demos/brightsteps/register.html",
      };
    }
    return { login: "/Portal/Login", register: "/Portal/Login" };
  }

  function sessionEmail(session) {
    var email = String((session && (session.email || session.login)) || "").trim();
    if (isEmail(email)) return email;
    var stub = email.replace(/[^a-z0-9._-]/gi, "") || "parent";
    return stub + "@brightsteps.local";
  }

  function saveVisitLocal(visit) {
    try {
      var list = [];
      var raw = localStorage.getItem(VISITS_KEY);
      if (raw) list = JSON.parse(raw) || [];
      if (!Array.isArray(list)) list = [];
      list.unshift(visit);
      localStorage.setItem(VISITS_KEY, JSON.stringify(list.slice(0, 80)));
    } catch (e) {}
  }

  function loadVisitsLocal() {
    try {
      var raw = localStorage.getItem(VISITS_KEY);
      var list = raw ? JSON.parse(raw) : [];
      return Array.isArray(list) ? list : [];
    } catch (e) {
      return [];
    }
  }

  function normalizeWhen(value) {
    return String(value || "")
      .toLowerCase()
      .replace(/[^\w\u0600-\u06ff\s:]/g, " ")
      .replace(/\s+/g, " ")
      .trim();
  }

  function extractTimeToken(value) {
    var m = String(value || "").match(/\b(\d{1,2})([:.]?\d{0,2})\s*(am|pm)?\b/i);
    if (!m) return "";
    var h = parseInt(m[1], 10);
    var mins = m[2] ? m[2].replace(/\D/g, "") : "";
    if (mins.length === 1) mins = mins + "0";
    if (mins.length > 2) mins = mins.slice(0, 2);
    if (!mins) mins = "00";
    var ap = (m[3] || "").toLowerCase();
    if (ap === "pm" && h < 12) h += 12;
    if (ap === "am" && h === 12) h = 0;
    return String(h).padStart(2, "0") + ":" + mins.padStart(2, "0");
  }

  function visitTimeTaken(when) {
    var wanted = normalizeWhen(when);
    if (!wanted) return false;
    var wantedTime = extractTimeToken(when);
    var visits = loadVisitsLocal();
    for (var i = 0; i < visits.length; i++) {
      var existing = normalizeWhen(visits[i] && visits[i].when);
      if (!existing) continue;
      if (existing === wanted) return true;
      if (wantedTime && extractTimeToken(visits[i].when) === wantedTime) {
        // Same clock time on a similar day phrase (e.g. Friday 10:00 vs friday 10)
        var dayA = wanted.replace(wantedTime.replace(":", ""), "").replace(/\d/g, "").trim();
        var dayB = existing.replace(wantedTime.replace(":", ""), "").replace(/\d/g, "").trim();
        if (!dayA || !dayB || dayA.indexOf(dayB) !== -1 || dayB.indexOf(dayA) !== -1) return true;
      }
    }
    return false;
  }

  function setStatus(msg) {
    var st = document.getElementById("campusBotStatus");
    if (st) st.textContent = msg || "";
  }

  function hushVoice() {
    if (speakTimer) {
      window.clearTimeout(speakTimer);
      speakTimer = null;
    }
    stopGoogleTts();
    if (window.speechSynthesis) {
      try {
        window.speechSynthesis.cancel();
      } catch (e) {}
    }
  }

  function warmVoices() {
    if (!window.speechSynthesis) return;
    try {
      if ((window.speechSynthesis.getVoices() || []).length) voicesReady = true;
      window.speechSynthesis.onvoiceschanged = function () {
        voicesReady = true;
      };
      window.speechSynthesis.getVoices();
    } catch (e) {}
  }

  function unlockSpeech() {
    if (speechUnlocked) return;
    try {
      warmVoices();
      if (window.speechSynthesis) {
        var warm = new SpeechSynthesisUtterance(" ");
        warm.volume = 0.01;
        warm.rate = 10;
        window.speechSynthesis.speak(warm);
      }
      var ping = new Audio(
        "data:audio/wav;base64,UklGRiQAAABXQVZFZm10IBAAAAABAAEAESsAACJWAAACABAAZGF0YQAAAAA="
      );
      ping.volume = 0.01;
      ping.play().catch(function () {});
      speechUnlocked = true;
    } catch (e) {}
  }

  function speechLocalePrefs() {
    var c = activeLang();
    // Never list English for Urdu/Punjabi — English voices only read Latin words.
    if (c === "ur") return ["ur-PK", "ur-IN", "ur", "hi-IN", "hi", "ar-SA", "ar-AE", "ar-EG", "ar"];
    if (c === "pa") return ["pa-IN", "pa-Guru-IN", "pa-PK", "pa", "ur-PK", "ur", "hi-IN", "hi"];
    if (c === "it") return ["it-IT", "it"];
    return ["en-US", "en-GB", "en"];
  }

  function speechLocale() {
    return speechLocalePrefs()[0];
  }

  function isEnglishVoice(v) {
    if (!v) return true;
    var lang = (v.lang || "").toLowerCase();
    var name = (v.name || "").toLowerCase();
    if (lang.indexOf("en") === 0) return true;
    if (/english|david|zira|mark|susan|hazel|george|aria|guy|jenny/i.test(name) && !/urdu|hindi|punjabi|arabic|india/i.test(name))
      return true;
    return false;
  }

  function pickVoice() {
    if (!window.speechSynthesis) return null;
    var voices = window.speechSynthesis.getVoices() || [];
    if (!voices.length) return null;
    var c = activeLang();
    var prefs = speechLocalePrefs();
    var best = null;
    var bestScore = -1;
    var i;
    var j;

    // Name-based match first (Windows often labels "Microsoft Urdu" with odd lang codes).
    if (c === "ur" || c === "pa") {
      for (i = 0; i < voices.length; i++) {
        var nm = (voices[i].name || "").toLowerCase();
        var lg = (voices[i].lang || "").toLowerCase();
        if (c === "ur" && (/urdu|اردو/.test(nm) || lg.indexOf("ur") === 0)) {
          return voices[i];
        }
        if (c === "pa" && (/punjabi|panjabi|پنجابی|gurmukhi/.test(nm) || lg.indexOf("pa") === 0)) {
          return voices[i];
        }
      }
      for (i = 0; i < voices.length; i++) {
        var nm2 = (voices[i].name || "").toLowerCase();
        var lg2 = (voices[i].lang || "").toLowerCase();
        if (/hindi|हिन्दी|हिंदी/.test(nm2) || lg2.indexOf("hi") === 0) return voices[i];
      }
      if (c === "ur") {
        for (i = 0; i < voices.length; i++) {
          var lg3 = (voices[i].lang || "").toLowerCase();
          if (lg3.indexOf("ar") === 0) return voices[i];
        }
      }
    }

    for (i = 0; i < prefs.length; i++) {
      var want = prefs[i].toLowerCase();
      var prefix = want.split("-")[0];
      for (j = 0; j < voices.length; j++) {
        var v = voices[j];
        if ((c === "ur" || c === "pa") && isEnglishVoice(v)) continue;
        var lang = (v.lang || "").toLowerCase();
        var score = 0;
        if (lang === want) score = 100 - i;
        else if (lang.indexOf(prefix) === 0) score = 70 - i;
        else continue;
        if (v.localService) score += 15;
        if (score > bestScore) {
          bestScore = score;
          best = v;
        }
      }
      if (best && bestScore >= 70) break;
    }

    if ((c === "ur" || c === "pa") && (!best || isEnglishVoice(best))) return null;
    if (best) return best;

    if (c === "en" || c === "it") {
      for (i = 0; i < voices.length; i++) {
        if (voices[i].localService) return voices[i];
      }
      return voices[0] || null;
    }
    return null;
  }

  var ttsAudio = null;

  function stopGoogleTts() {
    if (ttsAudio) {
      try {
        ttsAudio.pause();
        ttsAudio.src = "";
      } catch (e) {}
      ttsAudio = null;
    }
  }

  function ttsSrc(text, tl) {
    var chunk = String(text || "").replace(/\s+/g, " ").trim().slice(0, 160);
    return (
      "/api/tts?tl=" +
      encodeURIComponent(tl) +
      "&q=" +
      encodeURIComponent(chunk)
    );
  }

  /** Real Urdu/Punjabi audio via same-origin proxy (Chrome blocks translate.google.com). */
  function speakGoogleTts(text, tl, onFail) {
    stopGoogleTts();
    var chunk = String(text || "").replace(/\s+/g, " ").trim().slice(0, 160);
    if (!chunk) return false;
    try {
      ttsAudio = new Audio();
      ttsAudio.preload = "auto";
      ttsAudio.volume = 1;
      ttsAudio.muted = false;
      ttsAudio.src = ttsSrc(chunk, tl);
      ttsAudio.onplaying = function () {
        setStatus(
          { en: "Speaking…", it: "Sto parlando…", ur: "بول رہا ہوں…", pa: "بول رہا واں…" }[activeLang()] ||
            "Speaking…"
        );
      };
      ttsAudio.onended = function () {
        setStatus("");
        ttsAudio = null;
      };
      ttsAudio.onerror = function () {
        if (typeof onFail === "function") onFail();
      };
      var playPromise = ttsAudio.play();
      if (playPromise && playPromise.catch) {
        playPromise.catch(function () {
          if (typeof onFail === "function") onFail();
        });
      }
      return true;
    } catch (e) {
      return false;
    }
  }

  function googleTl() {
    var c = activeLang();
    if (c === "pa") return "pa";
    if (c === "ur") return "ur";
    if (c === "it") return "it";
    return "en";
  }

  function shouldSpeak() {
    return !!voiceTurn;
  }

  function keepSpeakingAlive() {
    var kicks = 0;
    var id = window.setInterval(function () {
      kicks += 1;
      try {
        if (window.speechSynthesis && window.speechSynthesis.paused) {
          window.speechSynthesis.resume();
        }
        if (!window.speechSynthesis || (!window.speechSynthesis.speaking && kicks > 3) || kicks > 40) {
          window.clearInterval(id);
        }
      } catch (e) {
        window.clearInterval(id);
      }
    }, 120);
  }

  /** Must run inside a click/tap handler on phones — delayed speak is muted. */
  function speakNow(text) {
    if (!text) return;
    warmVoices();
    unlockSpeech();
    stopGoogleTts();
    try {
      if (window.speechSynthesis) window.speechSynthesis.cancel();
    } catch (e) {}

    var c = activeLang();

    if (c === "ur" || c === "pa") {
      var started = speakGoogleTts(text, googleTl(), function () {
        speakUtterance(text);
      });
      if (started) return;
    }

    speakUtterance(text);
  }

  function speakUtterance(text) {
    var c = activeLang();
    if (!window.speechSynthesis) {
      setStatus("Speech not supported in this browser.");
      return;
    }

    var voice = pickVoice();
    if ((c === "ur" || c === "pa") && (!voice || isEnglishVoice(voice))) {
      setStatus(
        {
          en: "Turn volume up and tap 🔊 again. If it is still silent, allow sound for this site in Chrome.",
          ur: "والیوم بڑھا کر 🔊 دوبارہ دبائیں۔ اگر پھر بھی خاموش ہو تو Chrome میں اس سائٹ کی آواز اجازت دیں۔",
          pa: "والیوم ودھا کے 🔊 فیر دباؤ۔ جے فیر وی خاموش ہووے تاں Chrome وچ اس سائٹ دی آواز دیو۔",
          it: "Alza il volume e tocca di nuovo 🔊.",
        }[c] || "Tap 🔊 again"
      );
      return;
    }

    try {
      if (window.speechSynthesis.paused) window.speechSynthesis.resume();
    } catch (e2) {}

    var u = new SpeechSynthesisUtterance(String(text));
    u.lang = voice && voice.lang ? voice.lang : speechLocale();
    if (voice) u.voice = voice;
    u.rate = c === "ur" || c === "pa" ? 0.9 : 1;
    u.pitch = 1;
    u.volume = 1;
    u.onstart = function () {
      setStatus(
        { en: "Speaking…", it: "Sto parlando…", ur: "بول رہا ہوں…", pa: "بول رہا واں…" }[c] || "Speaking…"
      );
    };
    u.onend = function () {
      setStatus("");
    };
    u.onerror = function () {
      setStatus(
        {
          en: "Still no sound — turn volume up, or tap 🔊 again.",
          it: "Ancora niente audio — alza il volume.",
          ur: "آواز نہیں — والیوم چیک کریں یا 🔊 دوبارہ دبائیں۔",
          pa: "آواز نہیں — والیوم چیک کرو یا 🔊 فیر دباؤ۔",
        }[c] || "No sound"
      );
    };
    window.speechSynthesis.speak(u);
    keepSpeakingAlive();
  }

  function speak(text, force) {
    if (!text) return;
    if (force) {
      speakNow(text);
      return;
    }
    if (!shouldSpeak()) return;
    if (speakTimer) window.clearTimeout(speakTimer);
    speakTimer = window.setTimeout(function () {
      speakTimer = null;
      if (!shouldSpeak()) return;
      var c = activeLang();
      var voice = pickVoice();
      if ((c === "ur" || c === "pa") && (!voice || isEnglishVoice(voice))) {
        setStatus(
          {
            en: "Tap the yellow 🔊 to hear in Urdu/Punjabi.",
            ur: "اردو سننے کے لیے پیلا 🔊 دبائیں۔",
            pa: "سنن لئی پیلا 🔊 دباؤ۔",
            it: "Tocca 🔊 per ascoltare.",
          }[c] || "Tap 🔊"
        );
        return;
      }
      speakNow(text);
      window.setTimeout(function () {
        if (shouldSpeak()) {
          setStatus(
            {
              en: "If you hear English only, tap 🔊 for Urdu audio.",
              ur: "اگر انگریزی آواز آئے تو اردو کے لیے 🔊 دبائیں۔",
              pa: "جے انگریزی آواز آوے تاں 🔊 دباؤ۔",
              it: "Tocca 🔊 se serve.",
            }[c] || "Tap 🔊"
          );
        }
      }, 900);
    }, 300);
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
    warmVoices();
    document.getElementById("campusBotFab").addEventListener("click", function () {
      unlockSpeech();
      openPanel();
    });
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
    fab.setAttribute("aria-label", copy.fab);
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
      [copy.chipHours, "__hours__"],
      [copy.chipPrograms, "__programs__"],
      [copy.chipVisit, "__book__"],
      [copy.chipPortal, "__portal__"],
    ]
      .concat(currentSession() ? [] : [
        [copy.chipLogin, "__login__"],
        [copy.chipRegister, "__register__"],
      ])
      .forEach(function (pair) {
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
    var row = document.createElement("div");
    row.className = "campus-bot__row campus-bot__row--" + who;
    var b = document.createElement("div");
    b.className = "campus-bot__bubble campus-bot__bubble--" + who;
    b.textContent = text;
    row.appendChild(b);
    if (who === "bot") {
      var btn = document.createElement("button");
      btn.type = "button";
      btn.className = "campus-bot__speak";
      btn.setAttribute("aria-label", "Speak reply");
      btn.title = "Speak";
      btn.textContent = "🔊";
      btn.addEventListener("click", function (ev) {
        ev.preventDefault();
        ev.stopPropagation();
        speakNow(text);
      });
      row.appendChild(btn);
    }
    log.appendChild(row);
    log.scrollTop = log.scrollHeight;
  }

  function botSay(text) {
    addBubble("bot", text);
    if (shouldSpeak()) {
      speak(text);
      window.setTimeout(function () {
        var st = document.getElementById("campusBotStatus");
        if (st && /Speaking/.test(st.textContent || "")) return;
        if (shouldSpeak()) {
          setStatus(
            {
              en: "Tap the yellow 🔊 to hear the reply.",
              it: "Tocca il 🔊 giallo per ascoltare.",
              ur: "سننے کے لیے پیلا 🔊 دبائیں۔",
              pa: "سنن لئی پیلا 🔊 دباؤ۔",
            }[activeLang()] || "Tap 🔊"
          );
        }
      }, 900);
    }
  }

  function beginTurn(fromVoice) {
    voiceTurn = !!fromVoice;
    if (!voiceTurn) hushVoice();
  }

  function looksLikeNewQuestion(text) {
    var folded = foldText(text);
    if (!folded || isEmail(text)) return false;
    if (
      hasAny(folded, [
        "hello",
        "hi",
        "hey",
        "ہیلو",
        "هيلو",
        "ہائے",
        "سلام",
        "السلام",
        "ciao",
        "salam",
        "hy",
        "helo",
      ])
    )
      return true;
    if (
      hasAny(folded, [
        "hour",
        "timing",
        "fee",
        "program",
        "portal",
        "teacher",
        "address",
        "اوقات",
        "فیس",
        "پروگرام",
        "پورٹل",
        "استاد",
        "اواریں",
      ])
    )
      return true;
    return false;
  }

  function handleUser(text, fromVoice, forced) {
    beginTurn(fromVoice);
    // Follow THIS message's language — but not name/email answers mid-booking.
    var chipIntent = forced && String(forced).indexOf("__") === 0;
    if (!booking.step || looksLikeNewQuestion(text) || chipIntent) {
      noteUserLang(text);
      paintChrome();
    }
    addBubble("user", text);

    if (forced === "__login__") {
      window.location.href = authPaths().login;
      return;
    }
    if (forced === "__register__") {
      window.location.href = authPaths().register;
      return;
    }

    if (forced === "__book__" || (forced == null && wantsBook(text) && !booking.step)) {
      startBooking();
      return;
    }

    // Chips resolve in the language just selected from the chip label / chatLang.
    if (forced === "__hours__") {
      booking.step = "";
      botSay(t().hours);
      return;
    }
    if (forced === "__programs__") {
      booking.step = "";
      botSay(t().programs);
      return;
    }
    if (forced === "__portal__") {
      booking.step = "";
      botSay(t().portal);
      return;
    }

    if (booking.step) {
      // "hello" / new FAQ must not be treated as a booking name.
      if (forced == null && looksLikeNewQuestion(text)) {
        booking.step = "";
      } else {
        stepBooking(text);
        return;
      }
    }

    var reply = intentReply(text);
    if (reply == null) {
      startBooking();
      return;
    }
    botSay(reply);
  }

  function startBooking() {
    var session = currentSession();
    if (!session) {
      botSay(t().needLogin);
      renderChips();
      return;
    }
    booking = {
      step: "when",
      name: session.name || "Family",
      email: sessionEmail(session),
      when: "",
      age: "",
    };
    botSay(fill(t().askWhenLoggedIn, { name: booking.name }));
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
      if (visitTimeTaken(s)) {
        botSay(fill(t().timeTaken, { when: s }));
        return;
      }
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
    if (!currentSession()) {
      booking.step = "";
      botSay(t().needLogin);
      renderChips();
      return;
    }
    if (visitTimeTaken(booking.when)) {
      booking.step = "when";
      botSay(fill(t().timeTaken, { when: booking.when }));
      return;
    }
    var visit = {
      id: String(Date.now()),
      name: booking.name,
      email: booking.email,
      when: booking.when,
      age: booking.age,
      language: activeLang(),
      createdAt: new Date().toISOString(),
    };
    saveVisitLocal(visit);
    var message =
      "[School visit]\nWhen: " +
      visit.when +
      "\nChild age / year: " +
      visit.age +
      "\nLanguage: " +
      visit.language;
    var headers = { "Content-Type": "application/json" };
    Promise.allSettled([
      fetch("/api/campus-visits", {
        method: "POST",
        headers: headers,
        credentials: "same-origin",
        body: JSON.stringify({
          name: visit.name.slice(0, 120),
          email: visit.email.slice(0, 200),
          when: String(visit.when).slice(0, 200),
          age: String(visit.age).slice(0, 120),
          language: visit.language,
        }),
      }),
      fetch("/api/contact", {
        method: "POST",
        headers: headers,
        body: JSON.stringify({
          name: visit.name.slice(0, 120),
          email: visit.email.slice(0, 200),
          message: message.slice(0, 4000),
        }),
      }),
    ]).then(function () {
      booking.step = "";
      botSay(t().booked);
    });
  }

  function speechRecognitionLocale() {
    var c = activeLang();
    // Pakistani Punjabi (Shahmukhi) is recognized more reliably with Urdu STT.
    if (c === "ur" || c === "pa") return "ur-PK";
    if (c === "it") return "it-IT";
    return "en-US";
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
      var voiceHint = {
        en: "Voice works in Chrome or Edge. For Urdu/Punjabi, type one word first or use Edge with language packs.",
        it: "La voce funziona in Chrome o Edge.",
        ur: "آواز Chrome یا Edge میں کام کرتی ہے۔ اردو/پنجابی بولنے سے پہلے ایک لفظ لکھیں، پھر مائیک دبائیں۔",
        pa: "آواز Chrome یا Edge وچ کم کردی اے۔ اردو/پنجابی بولن توں پہلے اک لفظ لکھو، فیر مائیک دباؤ۔",
      };
      document.getElementById("campusBotStatus").textContent =
        voiceHint[activeLang()] || voiceHint.en;
      return;
    }
    warmVoices();
    unlockSpeech();
    rec = new Ctor();
    rec.lang = speechRecognitionLocale();
    rec.interimResults = false;
    rec.maxAlternatives = 3;
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
    try {
      rec.start();
    } catch (e) {
      stopMic();
      return;
    }
    mic.classList.add("is-on");
    mic.setAttribute("aria-label", t().micOn);
    var listenHint = {
      en: t().micOn,
      it: t().micOn,
      ur: "سن رہا ہوں… اردو یا پنجابی میں بولیں",
      pa: "سُن رہا واں… اردو یا پنجابی وچ بولو",
    };
    document.getElementById("campusBotStatus").textContent =
      listenHint[activeLang()] || t().micOn;
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
    warmVoices();
    mount();
  }

  if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", boot);
  else boot();
})();
