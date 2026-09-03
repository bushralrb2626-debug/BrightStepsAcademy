/**
 * Scuola Materna — keyed EN / IT switcher.
 * Default: Italian. Choice saved in localStorage.
 */
(function () {
  "use strict";

  var STORAGE_KEY = "scuola-materna-lang";
  var STRINGS = {};
  var LOOKUP = Object.create(null);

  function norm(s) {
    return String(s || "")
      .replace(/&#x2014;|&mdash;/gi, "—")
      .replace(/&#x2013;|&ndash;/gi, "–")
      .replace(/&#x27;|&apos;/gi, "'")
      .replace(/&amp;/g, "&")
      .replace(/&copy;/g, "©")
      .replace(/&#x2B;/g, "+")
      .replace(/&#x2026;/g, "…")
      .replace(/[\u2018\u2019]/g, "'")
      .replace(/[\u201C\u201D]/g, '"')
      .replace(/[\u00A0]/g, " ")
      .replace(/\s+/g, " ")
      .trim();
  }

  function add(key, en, it, aliases) {
    STRINGS[key] = { en: en, it: it };
    LOOKUP[norm(en)] = key;
    LOOKUP[norm(it)] = key;
    (aliases || []).forEach(function (a) {
      LOOKUP[norm(a)] = key;
    });
  }

  add("nav.home", "Home", "Home");
  add("nav.about", "About", "Chi siamo");
  add("nav.programs", "Programs", "Programmi");
  add("nav.facilities", "Facilities", "Strutture");
  add("nav.teachers", "Teachers", "Insegnanti", ["Insegnantes"]);
  add("nav.activities", "Activities", "Attività");
  add("nav.events", "Events", "Eventi");
  add("nav.gallery", "Gallery", "Galleria");
  add("nav.contact", "Contact", "Contatti");
  add("nav.login", "Login", "Accedi");
  add("nav.loginCta", "🔐 Login", "🔐 Accedi");
  add("nav.openMenu", "Open menu", "Apri menu");
  add("nav.homeAria", "Scuola Materna home", "Home Scuola Materna", ["BrightSteps Academy home"]);
  add("lang.group", "Language", "Lingua");
  add("brand.tagline", "Learn. Explore. Grow.", "Impara. Esplora. Cresci.");
  add("brand.growLine", "Learn, explore and grow.", "Impara, esplora e cresci.");
  add("close", "Close", "Chiudi");

  add("hero.welcome", "Welcome to Scuola Materna", "Benvenuti alla Scuola Materna");
  add("hero.h1a", "WHERE LITTLE MINDS", "DOVE LE PICCOLE MENTI");
  add("hero.h1b", "GROW INTO BIG DREAMS", "CRESCONO IN GRANDI SOGNI");
  add(
    "hero.lead",
    "A colorful campus where curiosity blooms, creativity shines, and every child gets to learn, explore and dream — every day.",
    "Un campus colorato dove fiorisce la curiosità, brilla la creatività e ogni bambino può imparare, esplorare e sognare — ogni giorno.",
    [
      "A colorful campus where curiosity blooms, creativity shines, and every child gets to learn, explore and dream",
      "Un campus colorato dove fiorisce la curiosità, brilla la creatività e ogni bambino può imparare, esplorare e sognare",
    ]
  );
  add("hero.explore", "Explore our school", "Esplora la nostra scuola", ["Explore Our School"]);
  add("hero.portal", "Student Portal", "Portale studenti");
  add("hero.campusAria", "Scuola Materna campus", "Campus Scuola Materna");
  add("img.kidsBooks", "Children learning with books", "Bambini che imparano con i libri");

  add("sec.learningWorld", "Our learning world", "Il nostro mondo dell’apprendimento");
  add("sec.brightBeginnings", "A story of bright beginnings", "Una storia di inizi luminosi");
  add(
    "sec.notJustCampus",
    "Not just a campus — a colorful journey families love to share.",
    "Non solo un campus — un viaggio colorato che le famiglie amano condividere. Aule illuminate dal sole, domande curiose e insegnanti che conoscono ogni bambino per nome.",
    [
      "Not just a campus — a colorful journey families love to join.",
      "Non solo un campus — un percorso colorato che le famiglie amano condividere.",
      "Sunlit classrooms, curious questions, and teachers who know every child by name — that’s the BrightSteps way.",
      "Aule illuminate dal sole, domande curiose e insegnanti che conoscono ogni bambino per nome — questo è lo stile BrightSteps.",
    ]
  );
  add("sec.whereBegins", "Where learning begins", "Dove inizia l’apprendimento");
  add("sec.mission", "Our mission", "La nostra missione");
  add("sec.vision", "Our vision", "La nostra visione");
  add("sec.whyFamilies", "Why families choose us", "Perché le famiglie ci scelgono");
  add(
    "sec.missionBody",
    "To nurture curious, kind and confident learners through joyful teaching and meaningful experiences.",
    "Coltivare apprendisti curiosi, gentili e sicuri di sé attraverso un insegnamento gioioso ed esperienze significative."
  );
  add(
    "sec.visionBody",
    "A world where every child feels seen, challenged and inspired to grow — one bright step at a time.",
    "Un mondo in cui ogni bambino si sente visto, stimolato e ispirato a crescere — un passo luminoso alla volta."
  );
  add(
    "sec.whyBody",
    "Safe spaces, creative learning, strong academics, and a portal that keeps parents close to every milestone.",
    "Spazi sicuri, apprendimento creativo, solida didattica e un portale che tiene i genitori vicini a ogni traguardo."
  );
  add(
    "sec.needAll",
    "Everything they need to learn, play and grow",
    "Tutto ciò di cui hanno bisogno per imparare, giocare e crescere"
  );
  add("sec.seeSpaces", "See all spaces", "Vedi tutti gli spazi");
  add("sec.meetTeachers", "Meet our teachers", "Incontra i nostri insegnanti");
  add("sec.ourPrograms", "Our programs", "I nostri programmi");
  add("sec.ourFacilities", "Our facilities", "Le nostre strutture");
  add("sec.schoolActivities", "School activities", "Attività scolastiche");
  add("sec.upcomingEvents", "Upcoming events", "Prossimi eventi");
  add("sec.schoolGallery", "School gallery", "Galleria della scuola", ["Galleria scolastica"]);
  add("sec.visitUs", "Visit us", "Vienici a trovare", ["Visita us", "Visit Us"]);
  add("sec.aboutSchool", "About our school", "La nostra scuola");
  add("sec.getInTouch", "Get in touch", "Scrivici");
  add("sec.schoolLife", "School life", "Vita scolastica");
  add("sec.explore", "Explore", "Esplora");
  add("sec.visit", "Visit", "Visita");
  add("sec.readyLearn", "WHO IS READY TO LEARN?", "CHI È PRONTO A IMPARARE?");
  add("sec.startReady", "READY TO BEGIN", "PRONTI A INIZIARE");
  add("sec.theAdventure", "THE ADVENTURE?", "L'AVVENTURA?");
  add(
    "sec.joinStory",
    "Join a school where learning is a discovery — and every hallway tells a new story.",
    "Unisciti a una scuola dove imparare è una scoperta — e ogni corridoio racconta una nuova storia."
  );
  add("sec.joinSchool", "Join our school", "Unisciti alla nostra scuola");
  add("sec.haveAccount", "Already have an account?", "Hai già un account?");
  add("sec.chooseAdventure", "Choose your adventure", "Scegli la tua avventura");
  add(
    "sec.chooseDoor",
    "Choose your door — Teacher, Parent or Student.",
    "Scegli la tua porta — Insegnante, Genitore o Studente."
  );
  add(
    "footer.joy",
    "A joyful place where children learn, explore and grow — every day.",
    "Un luogo gioioso dove i bambini imparano, esplorano e crescono — ogni giorno.",
    [
      "A joyful place where children learn, explore, and grow — every single day.",
      "Un luogo gioioso dove i bambini imparano, esplorano e crescono — ogni singolo giorno.",
    ]
  );
  add("footer.copy", "© 2026 Scuola Materna · Learn. Explore. Grow.", "© 2026 Scuola Materna · Impara. Esplora. Cresci.", [
    "© 2026 BrightSteps Academy · Learn. Explore. Grow.",
  ]);

  add("prog.early", "Early Learning", "Prima infanzia", ["Early Apprendimento"]);
  add("prog.primary", "Primary School", "Scuola primaria");
  add("prog.middle", "Middle School", "Scuola media");
  add("prog.arts", "Creative Arts", "Arti creative", ["Creativo Arts"]);
  add("prog.sports", "Sports", "Sport");
  add("prog.stem", "Science & Technology", "Scienza e tecnologia");
  add("prog.earlyDesc", "Building curiosity through play and discovery.", "Coltivare la curiosità attraverso il gioco e la scoperta.");
  add("prog.primaryDesc", "Strong foundations for lifelong learning.", "Basi solide per un apprendimento che dura tutta la vita.");
  add("prog.middleDesc", "Developing knowledge, confidence and independence.", "Sviluppare conoscenze, fiducia e indipendenza.");
  add("prog.artsDesc", "Helping students express their imagination.", "Aiutare gli studenti a esprimere la loro immaginazione.");
  add("prog.sportsDesc", "Building teamwork, confidence and healthy habits.", "Costruire collaborazione, fiducia e abitudini sane.");
  add("prog.stemDesc", "Exploring the world through innovation and discovery.", "Esplorare il mondo attraverso innovazione e scoperta.");
  add("prog.pathways", "Pathways for every learner", "Percorsi per ogni allievo");
  add(
    "prog.pathwaysLead",
    "Every path is a colorful tile of discovery — not a dull brochure list.",
    "Ogni percorso è una tessera colorata di scoperta — non un elenco noioso da depliant."
  );
  add("prog.learnMore", "Learn more", "Scopri di più");
  add("prog.exploreMore", "Explore more", "Esplora di più", ["Esplora More"]);
  add("prog.explorePath", "Explore pathway →", "Esplora pathway →");
  add("prog.findMix", "Find the right mix of study, art, sport and discovery.", "Trova il giusto mix di studio, arte, sport e scoperta.");

  add("fac.library", "Modern Library", "Biblioteca moderna");
  add("fac.science", "Science Laboratory", "Laboratorio di scienze");
  add("fac.computer", "Computer Lab", "Laboratorio di informatica");
  add("fac.sports", "Sports Ground", "Campo sportivo");
  add("fac.art", "Art & Creativity Room", "Sala arte e creatività");
  add("fac.music", "Music Room", "Sala musica");
  add("fac.play", "Play Area", "Area giochi");
  add("fac.libraryDesc", "Books, reading spaces and learning resources.", "Libri, spazi di lettura e risorse didattiche.");
  add("fac.scienceDesc", "Hands-on experiments and discovery.", "Esperimenti pratici e scoperta.");
  add("fac.computerDesc", "Technology and digital learning.", "Tecnologia e apprendimento digitale.");
  add("fac.sportsDesc", "Outdoor sports and physical activities.", "Sport all’aperto e attività motorie.");
  add("fac.artDesc", "Painting, crafts and creative expression.", "Pittura, laboratori e espressione creativa.");
  add("fac.musicDesc", "Music, instruments and performance.", "Musica, strumenti e performance.");
  add("fac.playDesc", "Safe and engaging recreational space.", "Spazio ricreativo sicuro e coinvolgente.");
  add("fac.spacesWonder", "Spaces designed for wonder", "Spazi pensati per meravigliarsi");
  add(
    "fac.exploreLibs",
    "Explore libraries, labs, fields, studios, and safe play areas.",
    "Esplora biblioteche, laboratori, campi, studi e aree gioco sicure.",
    ["Esplora libraries, labs, fields, studios, and safe play areas."]
  );
  add("fac.exploreFac", "Explore facility →", "Esplora la struttura →", ["Esplora Facility →"]);
  add(
    "fac.campusPlay",
    "A campus designed as a playground for imagination — with serious learning spaces inside.",
    "Un campus pensato come un parco giochi per l'immaginazione — con spazi di apprendimento seri al suo interno."
  );
  add("fac.smartClass", "Smart classrooms", "Aule smart");
  add("fac.interactive", "Interactive learning environments.", "Ambienti di apprendimento interattivi.");
  add("fac.canteen", "Cafeteria", "Mensa");
  add("fac.canteenDesc", "A comfortable dining space for students.", "Area mensa confortevole per gli studenti.");
  add("fac.transport", "School transport", "Trasporto scolastico");
  add("fac.transportDesc", "Safe transport services.", "Servizi di trasporto sicuri.");
  add("fac.clinic", "Nurse / first aid", "Infermeria / Pronto soccorso");
  add("fac.clinicDesc", "Student health and first-aid support.", "Assistenza sanitaria e primo soccorso per gli studenti.", [
    "Studente health and first-aid support.",
  ]);
  add("fac.secure", "Safe and secure campus", "Campus sicuro e protetto");
  add("fac.secureDesc", "Modern campus security systems.", "Sistemi moderni di sicurezza del campus.");
  add("fac.libraryShort", "Library", "Biblioteca");
  add("fac.campus", "Campus", "Campus");
  add("fac.yard", "Playground", "Cortile");

  add("why.teachers", "Qualified teachers", "Insegnanti qualificati");
  add("why.safe", "Safe environment", "Ambiente sicuro");
  add("why.creative", "Creative learning", "Apprendimento creativo");
  add("why.classrooms", "Modern classrooms", "Aule moderne");
  add("why.sports", "Sports & activities", "Sport e attività");
  add("why.parents", "Parent engagement", "Coinvolgimento dei genitori");
  add("why.teachersDesc", "Warm, trained educators who know every child by name.", "Educatori preparati e calorosi che conoscono ogni bambino per nome.");
  add("why.safeDesc", "Secure campus, caring staff and clear routines.", "Campus sicuro, personale attento e routine chiare.");
  add("why.creativeDesc", "Projects, stories and play that make ideas stick.", "Progetti, storie e gioco che fanno restare le idee.");
  add("why.classroomsDesc", "Bright rooms, smart boards and space to move.", "Aule luminose, lavagne interattive e spazio per muoversi.");
  add("why.sportsDesc", "Fields, clubs and festivals for every interest.", "Campi, club e feste per ogni interesse.");
  add("why.parentsDesc", "Clear updates, meetings and a portal that stays in sync.", "Aggiornamenti chiari, incontri e un portale sempre aggiornato.");

  add("act.art", "Art class", "Lezione di arte");
  add("act.fair", "Science fair", "Fiera della scienza");
  add("act.sports", "Sports day", "Giornata dello sport", ["Annual Giornata dello sport"]);
  add("act.trip", "Field trip", "Gita scolastica");
  add("act.reading", "Reading week", "Settimana della lettura");
  add("act.annual", "Annual function", "Saggio di fine anno");
  add("act.artDesc", "Brushes, collage and colour mixing every Wednesday.", "Pennelli, collage e mischiare i colori ogni mercoledì.");
  add("act.fairDesc", "Experiments, posters and proud inventors.", "Esperimenti, poster e piccoli inventori orgogliosi.");
  add("act.sportsDesc", "Races, relays and plenty of orange slices.", "Gare, staffette e tante fette d’arancia.");
  add("act.tripDesc", "Learning beyond the classroom walls.", "Imparare oltre le mura dell’aula.");
  add("act.readingDesc", "Blankets, book nooks and favourite characters.", "Coperte, angoli lettura e personaggi preferiti.");
  add("act.annualDesc", "Music, drama and a stage full of sparkle.", "Musica, teatro e un palco pieno di luccichio.");
  add("act.moments", "Moments children remember forever — art, science, sport and celebration.", "I momenti che i bambini ricordano per sempre — arte, scienza, sport e festa.");
  add("act.clubs", "Clubs, festivals and hands-on fun beyond the timetable.", "Club, feste e divertimento pratico oltre l'orario scolastico.");
  add("act.beyond", "Beyond the bell", "Oltre l'orario");
  add("act.bookFair", "Book fair", "Fiera del libro");
  add("act.bookFairDesc", "New stories, favourite authors and reading corners.", "Nuove storie, autori preferiti e angoli lettura.");
  add("act.picnic", "School picnic", "Picnic scolastico");
  add("act.picnicDesc", "Games, packed lunches and a day outdoors.", "Giochi, pranzi al sacco e una giornata all'aperto.");
  add("act.scienceShow", "Science exhibition", "Mostra scientifica");
  add("act.scienceShowDesc", "Student inventions, volcanoes and curious questions.", "Invenzioni degli studenti, vulcani e domande curiose.");
  add("act.ptm", "Parent-teacher meeting", "Incontro genitori-insegnanti", ["Parent Teacher Meeting", "Genitore Insegnante Meeting"]);
  add("act.ptmDesc", "A warm conversation about progress and next steps.", "Una conversazione calorosa su progressi e prossimi passi.");
  add("act.artWeek", "Art & creativity week", "Settimana dell'arte e della creatività");
  add("act.artWeekDesc", "Gallery walls full of prize-winning colour.", "Pareti della galleria piene di colori premiati.");
  add("act.sportsDayDesc2", "Races, relays and house cheers on a sunny field.", "Gare, staffette e tifo delle case sul campo soleggiato.");

  add("evt.markCal", "Mark the calendar — joy is scheduled", "Segna il calendario — la gioia è in programma");
  add("evt.posters", "Colorful posters for the days the whole school talks about.", "Manifesti colorati per i giorni di cui parla tutta la scuola.");
  add("evt.see", "See event →", "Vedi evento →");
  add("evt.days", "Sports days, exhibitions, meetings, and celebrations.", "Giornate sportive, mostre, incontri e celebrazioni.", [
    "Sport days, exhibitions, meetings, and celebrations.",
  ]);

  add("about.homeFeel", "A school that feels like home", "Una scuola che sembra casa");
  add("about.eyebrow", "About — Scuola Materna", "Chi siamo — Scuola Materna");
  add(
    "about.promise",
    "Learn. Explore. Grow. — our promise to every child and family.",
    "Impara. Esplora. Cresci. — la nostra promessa a ogni bambino e famiglia."
  );
  add("about.story", "Our story", "La nostra storia");
  add(
    "about.story1",
    "Scuola Materna began with a simple idea: childhood should be colorful, safe and full of discoveries. Today we welcome more than a thousand learners in bright classrooms, creative studios and lively fields.",
    "La Scuola Materna è nata da un'idea semplice: l'infanzia deve essere colorata, sicura e piena di scoperte. Oggi accogliamo più di mille allievi in aule luminose, studi creativi e campi vivaci."
  );
  add(
    "about.story2",
    "Our teachers combine solid teaching with kindness. Parents stay connected through notices, events and a welcoming portal. Together we celebrate curiosity, friendship and growth.",
    "I nostri insegnanti uniscono solidità didattica e gentilezza. I genitori restano connessi tramite avvisi, eventi e un portale accogliente. Insieme celebriamo curiosità, amicizia e crescita."
  );
  add("about.students", "Students", "Studenti");
  add("about.teachers", "Teachers", "Insegnanti");
  add("about.classes", "Classes", "Classi", ["Aule"]);
  add("about.years", "Years", "Anni");
  add("about.care", "Care at the heart of learning", "La cura al centro dell'apprendimento");
  add(
    "about.secondHome",
    "Creative, safe and full of colour. BrightSteps feels like a second home.",
    "Creativa, sicura e piena di colore. BrightSteps sembra una seconda casa."
  );
  add("about.happy", "HAPPY LEARNERS", "ALUNNI FELICI");
  add("about.bright", "BRIGHT CLASSROOMS", "CLASSI LUMINOSE");
  add("about.caring", "CARING TEACHERS", "INSEGNANTI ATTENTI");
  add("about.joyYears", "YEARS OF JOY", "ANNI DI GIOIA");
  add("about.enroll", "Admissions", "Iscrizioni");
  add("about.excellence", "Academic excellence", "Eccellenza scolastica");
  add("about.community", "Community awards", "Premi della comunità");
  add("about.comp", "Competition prizes", "Premi nelle gare");
  add("about.kindness", "Kindness, service and school spirit recognised.", "Gentilezza, servizio e spirito scolastico riconosciuti.");
  add("about.success", "% success in core subjects.", "% di successo nelle materie principali.");
  add("about.milestones", "+ student milestones celebrated.", "+ traguardi degli studenti celebrati.");
  add("about.trophies", "sports trophies this season.", "trofei sportivi in questa stagione.");
  add("about.medals", "medals in science, art and debate.", "medaglie in scienza, arte e dibattito.");
  add("about.parentsSay", "What parents say", "Cosa dicono i genitori");
  add("about.realVoices", "Real voices from the BrightSteps community.", "Voci reali dalla comunità BrightSteps.");
  add("about.parent", "Parent", "Genitore");
  add("about.quote1", "Every morning my child is excited to go to school — that says everything.", "Ogni mattina mio figlio è entusiasta di andare a scuola — questo dice tutto.");
  add("about.quote2", "The teachers know my child's strengths — and they celebrate the quiet wins too.", "Gli insegnanti conoscono i punti di forza di mio figlio — e festeggiano anche le sue vittorie silenziose.");
  add("stat.growing", "Growing every year", "In crescita ogni anno");
  add("stat.heart", "Excellence with heart", "Eccellenza con il cuore");
  add("stat.experts", "Experts who inspire", "Esperti che ispirano");

  add("teach.faces", "The faces children love to see", "I volti che i bambini amano vedere");
  add(
    "teach.warm",
    "Warm educators who turn classrooms into places of belonging.",
    "Educatori calorosi che trasformano le aule in luoghi di appartenenza."
  );
  add(
    "teach.hearts",
    "Warm educators with colorful classrooms and even bigger hearts.",
    "Educatori calorosi con aule colorate e cuori ancora più grandi."
  );
  add("teach.allClasses", "All classes", "Tutte le classi");
  add("teach.exp10", "10 years of experience", "10 anni di esperienza");
  add("teach.exp12", "12 years of experience", "12 anni di esperienza");
  add("teach.exp4", "4 years of experience", "4 anni di esperienza");
  add("teach.exp5", "5 years of experience", "5 anni di esperienza");
  add("teach.exp6", "6 years of experience", "6 anni di esperienza");
  add("teach.exp7", "7 years of experience", "7 anni di esperienza");
  add("teach.exp8", "8 years of experience", "8 anni di esperienza");
  add("teach.exp9", "9 years of experience", "9 anni di esperienza");
  add("subj.math", "Mathematics", "Matematica");
  add("subj.eng", "English", "Inglese");
  add("subj.sci", "Science", "Scienze");
  add("subj.art", "Art", "Arte");
  add("subj.pe", "Physical education", "Educazione fisica");
  add("subj.cs", "Computer science", "Informatica");
  add("subj.music", "Music", "Musica");
  add("subj.urdu", "Urdu", "Urdu");
  add("role.teacher", "Teacher", "Insegnante");
  add("role.parent", "Parent", "Genitore");
  add("role.student", "Student", "Studente");
  add("role.head", "Headmaster", "Preside");
  add("role.admin", "School Admin", "Amministratore scolastico");
  add("role.super", "Super Admin", "Super Admin");
  add("role.learning", "Learning", "Apprendimento");
  add("role.creative", "Creative", "Creativo");

  add("gal.lifeColor", "Life in colour — a scrapbook wall", "La vita a colori — un muro scrapbook");
  add("gal.click", "Click a photo to open it. Irregular, editorial, alive.", "Clicca su una foto per aprirla. Irregolare, editoriale, viva.");
  add("gal.moments", "Moments from classrooms, fields, studios and celebrations.", "Momenti dalle aule, dai campi, dagli studi e dalle celebrazioni.");
  add("gal.galleries", "Galleries, labs and a splash of colour.", "Gallerie, laboratori e un tocco di colore.");
  add("gal.readingStars", "Reading stars", "Stelle della lettura");
  add("gal.artWinners", "Art winners", "Vincitori d'arte");
  add("gal.sportChamps", "Sports champions", "Campioni sportivi", ["Sport Champions"]);
  add("gal.trophies", "Trophy-worthy celebrations", "Celebrazioni degne di un trofeo");
  add("gal.wins", "Big and small wins — always cheered with heart.", "Vittorie grandi e piccole — sempre applaudite con entusiasmo.");
  add("gal.building", "School building", "Edificio scolastico");
  add("img.classMoments", "Classroom moments", "Momenti in aula");
  add("img.storyTime", "Story time", "Ora della storia");
  add("img.libraryQuiet", "Library quiet hours", "Ore di silenzio in biblioteca");
  add("img.artStudio", "Art studio", "Studio d’arte");
  add("img.campusView", "Campus view", "Vista del campus");
  add("img.playJoy", "Playground joy", "Gioia nel cortile");
  add("img.annual", "Annual celebration", "Festa annuale");
  add("img.musicClass", "Music class", "Lezione di musica");
  add("img.gate", "Welcome gate", "Cancello di benvenuto");
  add("img.kidsPlay", "Children playing", "Bambini che giocano");
  add("img.brightRoom", "Bright classroom", "Aula luminosa");
  add("img.schoolLib", "School library", "Biblioteca scolastica");
  add("img.labCoats", "Lab coats and big questions", "Camici da laboratorio e grandi domande");
  add("img.clay", "Colour and clay in the studio", "Colori e argilla nello studio");
  add("img.dawn", "BrightSteps campus at dawn", "Campus BrightSteps all'alba");
  add("img.annualLights", "The annual show lights up", "Il saggio annuale si illumina");
  add("img.houseRaces", "House races on Sports Day", "Gare delle case alla Giornata dello sport", [
    "House races on Giornata dello sport",
  ]);
  add("img.wilson", "Ms Wilson with class 5", "La sig.ra Wilson con la classe 5");
  add("img.morning", "Morning in a bright classroom", "Mattina in un'aula luminosa");
  add("img.choir", "Choir practice after lunch", "Prove del coro dopo pranzo");
  add("img.laughs", "Laughs in the yard", "Risate nel cortile");
  add("img.newStory", "Students lost in a new story", "Studenti immersi in una nuova storia");
  add("img.outdoor", "A day of outdoor discovery", "Una giornata di scoperta all'aperto");
  add("img.quietCorners", "Quiet library corners", "Angoli silenziosi della biblioteca");
  add("img.sportDay", "Sports day", "Giornata dello sport", ["Sport day"]);

  add("contact.meet", "We would love to meet your family.", "Ci piacerebbe conoscere la vostra famiglia.");
  add("contact.maple", "We love meeting new families on Maple Grove.", "Ci piace conoscere nuove famiglie su Maple Grove.");
  add("contact.fullName", "Full name", "Nome completo", ["Nome"]);
  add("contact.yourName", "Your name", "Il tuo nome");
  add("contact.message", "Message", "Messaggio");
  add("contact.send", "Send message", "Invia messaggio");
  add("contact.hours", "Office hours", "Orari di ufficio", ["Orari di ufficio:"]);
  add("contact.hoursVal", "Mon–Fri 8:00 AM – 4:00 PM", "Lun–Ven 8:00 – 16:00", ["Lun–Ven 8:00 – 16:00"]);
  add("contact.sayHi", "Come say hello", "Vieni a salutarci");
  add("contact.mapSoon", "📍 Map coming soon", "📍 Mappa in arrivo");
  add("contact.mapPh", "Map placeholder", "Segnaposto mappa", ["📍 Segnaposto mappa"]);
  add("contact.placeholderMsg", "Tell us a little about your child or a visit request", "Raccontaci un po' di tuo figlio o della richiesta di visita");
  add("contact.helpPh", "How can we help?", "Come possiamo aiutarti?");
  add("contact.visitPh", "Admissions, visit, or question", "Iscrizioni, visita o domanda", [
    "Iscrizioni, visit, or question",
  ]);
  add("contact.phone", "Phone", "Telefono", ["Telefono:"]);
  add("contact.address", "Address", "Indirizzo", ["Indirizzo:"]);
  add("contact.subject", "Subject", "Oggetto");
  add("contact.social", "Social links", "Link social");

  add("portal.welcomeBack", "Welcome back", "Bentornati");
  add(
    "portal.authorized",
    "Authorized school staff and users only. Your dashboard opens based on your account.",
    "Solo personale e utenti autorizzati. La dashboard si apre in base al tuo account."
  );
  add("portal.schoolLogin", "School Login", "Accesso scuola");
  add(
    "portal.enterHint",
    "Enter your login ID and password. Your role is determined from your account.",
    "Inserisci email e password. Il ruolo dipende dal tuo account — non da questo modulo.",
    ["Enter your email and password. Role is determined from your account — not from this form."]
  );
  add("portal.emailId", "Email or Login ID", "Email o ID di accesso", ["Email or login ID"]);
  add("portal.remember", "Remember me", "Ricordami");
  add("portal.forgot", "Forgot password?", "Password dimenticata?");
  add("portal.portals", "School portals", "Portali della scuola", ["Portale scolastico"]);
  add("portal.choose", "Choose your portal", "Scegli il tuo portale");
  add("portal.signIn", "Sign in", "Accedi");
  add("portal.backWeb", "Back to website", "Torna al sito");
  add("portal.staffLogin", "Staff Login", "Accesso staff");
  add("portal.publicSite", "Public site", "Sito pubblico");
  add("portal.logOut", "Log out", "Esci");
  add("portal.connected", "Your School, Connected.", "La tua scuola, connessa.");
  add("portal.welcomePortal", "Welcome to Your School Portal", "Benvenuto nel portale della scuola");
  add("portal.demoCreds", "Demo credentials", "Credenziali demo");
  add("portal.worksHere", "(works on this site — no external login)", "(funziona su questo sito — nessun accesso esterno)");
  add("portal.howConnect", "Choose how you connect — teachers, parents and students. Sign in opens a working demo dashboard on this site.", "Scegli come collegarti — insegnanti, genitori e studenti. L'accesso apre una dashboard demo su questo sito.");
  add("portal.enterTeacher", "Enter Teacher Portal →", "Entra nel portale insegnanti →");
  add("portal.enterParent", "Enter Parent Portal →", "Entra nel portale genitori →");
  add("portal.enterStudent", "Enter Student Portal →", "Entra nel portale studenti →");
  add("portal.enterHead", "Enter Headmaster Portal →", "Entra nel portale preside →");
  add("portal.enterAdmin", "Enter School Admin Portal →", "Entra come amministratore →");
  add("portal.enterSuper", "Enter Super Admin Portal →", "Entra come Super Admin →");
  add("portal.manageLearn", "Manage classes, students and learning activities.", "Gestisci classi, studenti e attività di apprendimento.");
  add("portal.stayChild", "Stay connected with your child's learning.", "Resta connesso con l'apprendimento di tuo figlio.");
  add("portal.manageInfra", "Manage your school infrastructure, staff and website.", "Gestisci edifici, personale, studenti e sito web.");
  add("portal.monitor", "Monitor and manage the school.", "Monitora e gestisci la scuola.");
  add("portal.owner", "Platform owner — onboard schools and school admins.", "Proprietario della piattaforma — attiva scuole e amministratori.");
  add("title.home", "Home · Scuola Materna", "Home · Scuola Materna");
  add("title.about", "About our school · Scuola Materna", "La nostra scuola · Scuola Materna");
  add("title.programs", "Our programs · Scuola Materna", "I nostri programmi · Scuola Materna");
  add("title.facilities", "Our facilities · Scuola Materna", "Le nostre strutture · Scuola Materna");
  add("title.teachers", "Meet our teachers · Scuola Materna", "Incontra i nostri insegnanti · Scuola Materna");
  add("title.activities", "School activities · Scuola Materna", "Attività scolastiche · Scuola Materna");
  add("title.events", "Upcoming events · Scuola Materna", "Prossimi eventi · Scuola Materna");
  add("title.gallery", "School gallery · Scuola Materna", "Galleria della scuola · Scuola Materna");
  add("title.contact", "Visit us · Scuola Materna", "Vienici a trovare · Scuola Materna");
  add("title.portal", "School Portal · BrightSteps Academy", "Portale scolastico · BrightSteps Academy");
  add("title.login", "School Login · BrightSteps Academy", "Accesso scuola · BrightSteps Academy");

  add("bio.amina", "Amina helps children find their voice with stories, poetry and debate.", "Amina aiuta i bambini a trovare la propria voce con storie, poesia e diba…", ["Amina aiuta i bambini a trovare la propria voce con storie, poesia e diba&#x2026;"]);
  add("bio.david", "David believes every question is an experiment waiting to happen.", "David crede che ogni domanda sia un esperimento in attesa di nascere.");
  add("bio.emma", "Emma fills the halls with rhythm, choirs and a little everyday magic.", "Emma riempie i corridoi di ritmo, cori e un po' di magia quotidiana.");
  add("bio.fatima", "Fatima celebrates language, culture and the beauty of carefully chosen words.", "Fatima celebra la lingua, la cultura e la bellezza delle parole scelte con cura.");
  add("bio.james", "James coaches teamwork, grit and joy — on the field and off it.", "James allena spirito di squadra, tenacia e gioia — in campo e fuori.");
  add("bio.michael", "Michael presents coding as a creative language, not a chore.", "Michael presenta la programmazione come un linguaggio creativo, non come un compito.");
  add("bio.priya", "Priya's studio is a place where mess is welcome and imagination leads.", "Lo studio di Priya è un luogo dove il disordine è benvenuto e l'immaginazione guida.");
  add("bio.sarah", "Sarah turns numbers into stories. Her classroom is full of puzzles.", "Sarah trasforma i numeri in storie. La sua aula è piena di enigmi, co…");

  add("teach.math8", "Mathematics · 8 years", "Matematica · 8 anni");
  add("teach.sci6", "Science · 6 years", "Scienze · 6 anni");
  add("teach.eng10", "English · 10 years", "Inglese · 10 anni");
  add("teach.pe7", "Physical education · 7 years", "Educazione fisica · 7 anni");
  add("teach.art5", "Art · 5 years", "Arte · 5 anni");
  add("teach.cs9", "Computer science · 9 years", "Informatica · 9 anni");
  add("teach.urdu12", "Urdu · 12 years", "Urdu · 12 anni");
  add("teach.music4", "Music · 4 years", "Musica · 4 anni");
  add("about.parentG3", "Parent · Grade 3", "Genitore · Grade 3");
  add("about.parentG4", "Parent · Grade 4", "Genitore · Grade 4");
  add("about.parentG5", "Parent · Grade 5", "Genitore · Grade 5");
  add("stat.growingEmoji", "✨ Growing every year", "✨ In crescita ogni anno");
  add("stat.heartEmoji", "🎉 Excellence with heart", "🎉 Eccellenza con il cuore");
  add("stat.expertsEmoji", "🎯 Experts who inspire", "🎯 Esperti che ispirano");
  add("stat.wonderEmoji", "📚 Spaces designed for wonder", "📚 Spazi pensati per meravigliarsi");
  add("contact.email", "Email", "Email");
  add("contact.emailColon", "Email:", "Email:");
  add("contact.password", "Password", "Password");
  add("portal.col", "Portal", "Portale");
  add("portal.loginBtn", "Login", "Accedi");
  add("portal.studentLbl", "Student:", "Studente:");
  add("portal.parentLbl", "Parent:", "Genitore:");
  add("portal.teacherLbl", "Teacher:", "Insegnante:");
  add("portal.demoLoginPh", "e.g. student_demo", "es. student_demo");
  add("nav.toggle", "Open menu", "Apri menu", ["Toggle navigation"]);

  add("evt.artStudioWeek", "Art studio · All week", "Studio d'arte · Tutta la settimana");
  add("evt.scienceWing", "Science wing · 10:00", "Ala scientifica · 10:00");
  add("evt.library9", "Library · 9:00", "Biblioteca · 9:00");
  add("evt.sports830", "Sports ground · 8:30 AM", "Campo sportivo · 8:30 AM");
  add("evt.riverside", "Riverside Park · 9:30", "Parco Riverside · 9:30");
  add("evt.mainHall", "Main hall · 14:00", "Sala principale · 14:00");

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
    try {
      document.dispatchEvent(new CustomEvent("scuola-lang", { detail: { lang: lang } }));
    } catch (e2) {}
  }

  function valueFor(el, lang) {
    var key = el.getAttribute("data-i18n");
    if (!key || !STRINGS[key]) return null;
    var keep = el.querySelector(".btn-ico, .cta-arrow");
    var suffix = "";
    if (keep) suffix = " ";
    return STRINGS[key][lang];
  }

  function applyKeyed(lang) {
    document.querySelectorAll("[data-i18n]").forEach(function (el) {
      if (el.closest("[data-no-translate]")) return;
      var key = el.getAttribute("data-i18n");
      var pack = STRINGS[key];
      if (!pack) return;
      var keep = el.querySelector(".btn-ico, .cta-arrow");
      if (keep) {
        var icons = [];
        el.querySelectorAll(".btn-ico, .cta-arrow").forEach(function (n) {
          icons.push(n);
        });
        el.textContent = pack[lang] + " ";
        icons.forEach(function (n) {
          el.appendChild(n);
        });
      } else if (el.children.length === 0) {
        el.textContent = pack[lang];
      } else {
        var texts = [];
        el.childNodes.forEach(function (n) {
          if (n.nodeType === 3 && n.nodeValue && n.nodeValue.trim()) texts.push(n);
        });
        if (texts.length === 1) texts[0].nodeValue = pack[lang];
        else el.childNodes.forEach(function (n) {
          if (n.nodeType === 3 && n.nodeValue && n.nodeValue.trim()) {
            var k = LOOKUP[norm(n.nodeValue)];
            if (k && STRINGS[k]) n.nodeValue = STRINGS[k][lang];
          }
        });
      }
    });
    document.querySelectorAll("[data-i18n-placeholder]").forEach(function (el) {
      var pack = STRINGS[el.getAttribute("data-i18n-placeholder")];
      if (pack) el.setAttribute("placeholder", pack[lang]);
    });
    document.querySelectorAll("[data-i18n-aria]").forEach(function (el) {
      var pack = STRINGS[el.getAttribute("data-i18n-aria")];
      if (pack) el.setAttribute("aria-label", pack[lang]);
    });
    document.querySelectorAll("[data-i18n-alt]").forEach(function (el) {
      var pack = STRINGS[el.getAttribute("data-i18n-alt")];
      if (pack) el.setAttribute("alt", pack[lang]);
    });
    document.querySelectorAll("[data-i18n-title]").forEach(function (el) {
      var pack = STRINGS[el.getAttribute("data-i18n-title")];
      if (pack) el.setAttribute("title", pack[lang]);
    });
    var titleEl = document.querySelector("title");
    if (titleEl) {
      var tk = LOOKUP[norm(titleEl.textContent)];
      if (tk && STRINGS[tk]) titleEl.textContent = STRINGS[tk][lang];
    }
  }

  function translateUntagged(lang) {
    var root = document.body;
    if (!root) return;
    var walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT, null);
    var nodes = [];
    while (walker.nextNode()) nodes.push(walker.currentNode);
    nodes.forEach(function (node) {
      var parent = node.parentElement;
      if (!parent) return;
      if (parent.closest("[data-no-translate]")) return;
      if (parent.closest("[data-i18n]")) return;
      var tag = parent.tagName;
      if (tag === "SCRIPT" || tag === "STYLE" || tag === "NOSCRIPT") return;
      var text = node.nodeValue;
      if (!text || !text.trim()) return;
      var key = LOOKUP[norm(text)];
      if (!key || !STRINGS[key]) return;
      if (!parent.getAttribute("data-i18n") && parent.childNodes.length === 1) {
        parent.setAttribute("data-i18n", key);
      }
      node.nodeValue = STRINGS[key][lang];
    });

    root.querySelectorAll("[placeholder],[aria-label],[alt],[title]").forEach(function (el) {
      if (el.closest("[data-no-translate]")) return;
      ["placeholder", "aria-label", "alt", "title"].forEach(function (attr) {
        var val = el.getAttribute(attr);
        if (!val) return;
        var key = LOOKUP[norm(val)];
        if (!key || !STRINGS[key]) return;
        var map = { placeholder: "data-i18n-placeholder", "aria-label": "data-i18n-aria", alt: "data-i18n-alt", title: "data-i18n-title" };
        if (!el.getAttribute(map[attr])) el.setAttribute(map[attr], key);
        el.setAttribute(attr, STRINGS[key][lang]);
      });
    });
  }

  function translateDom(lang) {
    applyKeyed(lang);
    translateUntagged(lang);
    applyKeyed(lang);
  }

  function syncButtons(lang) {
    document.querySelectorAll("[data-set-lang]").forEach(function (btn) {
      var active = btn.getAttribute("data-set-lang") === lang;
      btn.classList.toggle("is-active", active);
      btn.setAttribute("aria-pressed", active ? "true" : "false");
    });
  }

  function ensureSwitcher() {
    var wrap = document.getElementById("langSwitch");
    if (!wrap) {
      var nav = document.querySelector(".site-nav__menu") || document.querySelector(".site-nav__inner");
      if (!nav) return;
      wrap = document.createElement("div");
      wrap.id = "langSwitch";
      wrap.className = "lang-switch";
      wrap.setAttribute("role", "group");
      wrap.setAttribute("aria-label", "Language");
      wrap.setAttribute("data-i18n-aria", "lang.group");
      wrap.innerHTML =
        '<button type="button" class="lang-switch__btn" data-set-lang="en" aria-pressed="false">EN</button>' +
        '<button type="button" class="lang-switch__btn" data-set-lang="it" aria-pressed="false">IT</button>';
      if (nav.classList.contains("site-nav__menu") && nav.parentElement) {
        nav.parentElement.insertBefore(wrap, nav.nextSibling);
      } else {
        nav.appendChild(wrap);
      }
    }
    if (wrap.dataset.bound === "1") return;
    wrap.dataset.bound = "1";
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

  window.addEventListener("pageshow", function () {
    setLang(currentLang());
  });

  window.ScuolaLang = { set: setLang, get: currentLang, strings: STRINGS };
})();
