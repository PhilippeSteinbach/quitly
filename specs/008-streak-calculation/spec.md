# Feature Specification: Präzise Streak- und Abstinenz-Berechnung inkl. Rückfall-Logik

**Feature Branch**: `008-streak-calculation`

**Created**: 2026-05-30

**Status**: Draft

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Sekundengenaue Streak-Anzeige (Priority: P1)

Als Nutzer möchte ich, dass meine aktuelle Abstinenzzeit sekundengenau, manipulationssicher und unter allen Bedingungen (Zeitzonenwechsel, Sommerzeit, Offline) korrekt angezeigt wird — damit ich jederzeit auf die Anzeige vertrauen kann.

**Why this priority**: Die Streak ist das zentrale Motivationsinstrument von Quitly. Wird sie falsch berechnet, verliert der Nutzer das Vertrauen in die App. Zudem ist sie Grundlage aller anderen Features (Achievements, XP, Kalender).

**Independent Test**: Eine Abstinenz-Abgewöhnung kann erstellt werden; der angezeigte Streak-Wert lässt sich gegen eine Referenzzeit verifizieren — ohne Kalender oder Statistik-Ansicht.

**Acceptance Scenarios**:

1. **Given** eine aktive Abgewöhnung (Startzeit T), **When** der Nutzer das Dashboard öffnet, **Then** zeigt das System die Abstinenzzeit als "X Tage Y Stunden Z Minuten" an, berechnet auf Basis der Serverszeit (bei Online-Zustand) bzw. der monotonen Geräteuhr (bei Offline-Zustand).
2. **Given** eine aktive Abgewöhnung, **When** der Nutzer die Zeitzone oder Sommerzeitumstellung erlebt, **Then** bleibt die angezeigte Streak-Dauer konsistent und wird nicht zurückgesetzt.
3. **Given** eine Abgewöhnung im Offline-Modus, **When** das Gerät wieder online geht und eine Zeitmanipulation erkannt wird (Geräteuhr wurde zurückgestellt), **Then** korrigiert das System die Streak auf die verifizierte Serverzeit und zeigt dem Nutzer einen nicht-strafenden Hinweis.
4. **Given** eine aktive Abgewöhnung, **When** der Nutzer die Startzeit nachträglich ändert (Feature 007), **Then** wird die Streak sofort auf Basis des neuen Startzeitpunkts und aller Rückfall-Einträge rekalkuliert.

---

### User Story 2 - Abstinenz-Statistik nach Rückfall (Priority: P2)

Als Nutzer möchte ich nach einem Rückfall sehen, an wie vielen Tagen ich im betreffenden Monat trotzdem abstinent war — damit ein Rückfall nicht das gesamte bisherige Bild meiner Fortschritte auslöscht.

**Why this priority**: Statistiken nach einem Rückfall sind entscheidend für die Motivation zur Weiterführung (Constitution II — Rückfall ohne Scham). Ohne diese Ansicht wirkt ein Rückfall wie ein vollständiger Verlust aller Fortschritte.

**Independent Test**: Nach einer simulierten Abgewöhnung mit einem Rückfall kann die Monatsstatistik ("X von Y Tagen abstinent") separat geprüft werden — unabhängig von der Kalenderansicht.

**Acceptance Scenarios**:

1. **Given** ein Monat mit 30 Tagen, von denen 7 als Rückfall-Tage markiert sind, **When** der Nutzer die Monatsstatistik öffnet, **Then** zeigt das System "23 von 30 Tagen abstinent" an.
2. **Given** ein Monat, in dem die Abgewöhnung erst am 15. begonnen hat, **When** die Monatsstatistik berechnet wird, **Then** zählt der Nenner Y nur die Tage ab Startdatum (also Y = 16), nicht den vollen Monat.
3. **Given** eine jahresbasierte Statistik, **When** der Nutzer die Jahresübersicht öffnet, **Then** zeigt das System "X von Y Tagen abstinent" über das gesamte Jahr aggregiert.
4. **Given** eine Abgewöhnung mit periodischem Intervall (z. B. max. 2× pro Woche), **When** der Nutzer die Wochenstatistik öffnet, **Then** zeigt das System "X von Y Perioden eingehalten" an (nicht eine tagesbasierte Kette).

---

### User Story 3 - Kalenderansicht abstinenter und Rückfall-Tage (Priority: P3)

Als Nutzer möchte ich eine Kalenderansicht (Monat / Jahr) meiner abstinenten, Rückfall- und pausierten Tage — damit ich Muster in meinem Verhalten erkennen kann.

**Why this priority**: Die Kalenderansicht ist die visuelle Evidenzschicht für Fortschritt. Sie ist wertvoll, setzt aber die korrekte Berechnung aus User Story 1 und 2 voraus.

**Independent Test**: Der Kalender kann für eine einzelne Abgewöhnung mit bekannten Rückfall-Daten geprüft werden — Farbcodes und Korrektheit der Tages-Status sind unabhängig von Streak und Statistik-Aggregation verifizierbar.

**Acceptance Scenarios**:

1. **Given** eine Abgewöhnung mit gemischtem Verlauf (abstinent, Rückfall, pausiert), **When** der Nutzer den Monats-Kalender öffnet, **Then** zeigt jeder Tag einen von drei Farbcodes: grün (abstinent), rot (Rückfall), grau (pausiert), neutral (Zukunft oder vor Startdatum).
2. **Given** ein Monat mit einem Rückfall an Tag 10, **When** der Nutzer auf den Tag 10 im Kalender tippt, **Then** zeigt das System den Rückfall-Zeitpunkt und — wenn angegeben — den Auslöser an (ohne urteilende Sprache).
3. **Given** eine nachträgliche Startzeit-Änderung (Feature 007), **When** der Kalender neu geladen wird, **Then** zeigt der Kalender die korrekte Einfärbung ab dem geänderten Startdatum.
4. **Given** eine Abgewöhnung im Jahresüberblick, **When** der Nutzer zur Jahresansicht wechselt, **Then** zeigt der Kalender jeden Monat als Heatmap-Kachel mit der Abstinenzrate des Monats.

---

### Edge Cases

- **Zeitzonenwechsel über Mitternacht**: Ein Nutzer fliegt von UTC+1 nach UTC−5 und landet nach Mitternacht Ortszeit — das Gerät springt auf einen früheren Tag zurück. Das System DARF die Streak nicht zurücksetzen, da die UTC-Gesamtdauer sich nicht geändert hat.
- **Sommerzeit-Umstellung**: In der Nacht der Uhren-Umstellung entsteht eine Lücke (Uhren vor) oder eine Dopplung (Uhren zurück). Das System MUSS Tagesgrenzen in UTC berechnen und darf keine Stunde als "fehlend" werten.
- **Schaltjahr und Schaltmonat**: Februar mit 29 Tagen muss korrekt als 29-Tage-Monat in Statistiken verwendet werden.
- **Geräteuhr-Manipulation (offline)**: Nutzer stellt die Geräteuhr manuell zurück, um die Streak künstlich zu verlängern. Beim nächsten Sync MUSS das System die Diskrepanz erkennen; die Streak wird auf den verifiziert korrekten Wert korrigiert. Kein beschämender Vorwurf an den Nutzer.
- **Mehrere Rückfälle am selben Kalendertag**: Mehr als ein Rückfall innerhalb von 24 Stunden — der Tag gilt als Rückfall-Tag; alle Rückfall-Ereignisse bleiben einzeln gespeichert.
- **Rückfall exakt um Mitternacht UTC**: Ein Rückfall, dessen Zeitstempel genau 00:00:00 UTC liegt, gehört dem neuen Tag.
- **Startzeit-Änderung auf einen Tag mit bestehendem Rückfall**: Liegt das neue Startdatum nach einem bestehenden Rückfall-Zeitstempel, MUSS das System warnen (konsistent mit Feature 007).
- **Erster Tag (Teilperiode)**: Der Startdatum-Tag wird als Teilperiode gewertet — er zählt als abstinenter Tag, wenn kein Rückfall an diesem Tag existiert, unabhängig von der Startzeit.
- **Pausierende Abgewöhnung (Feature 005)**: Tage, an denen die Abgewöhnung archiviert/pausiert war, zählen weder als abstinent noch als Rückfall — sie erhalten den Status "pausiert" im Kalender.

## Requirements *(mandatory)*

### Funktionale Anforderungen

**Zeitquelle und Manipulation**

- **FR-001**: Das System MUSS bei bestehender Online-Verbindung die Server-Zeit als autoritative Zeitquelle für alle Streak- und Datumsberechnungen verwenden.
- **FR-002**: Das System MUSS im Offline-Modus die monotone Geräteuhr verwenden, um die verstrichene Zeit seit dem letzten bekannten Server-Zeitstempel zu berechnen. Die angezeigte Streak-Dauer ergibt sich aus dem letzten verifizierten Zeitstempel plus der offline verstrichenen monotonen Zeit.
- **FR-003**: Das System MUSS beim Wiederherstellen der Online-Verbindung den lokal berechneten Streak-Wert mit dem Server-Zeitstempel abgleichen. Wird eine **negative** Abweichung von mehr als 5 Minuten festgestellt (Geräteuhr liegt hinter der Serverzeit — Hinweis auf absichtliches Zurückstellen), MUSS das System die Streak auf den serverseitig verifizierten Wert korrigieren und dem Nutzer einen sachlichen, nicht-strafenden Hinweis anzeigen (Constitution II). Eine positive Abweichung (Geräteuhr läuft der Serverzeit voraus) wird still korrigiert, ohne einen Hinweis auszulösen.

**Streak-Berechnung**

- **FR-004**: Streak-Dauer MUSS in UTC-Sekunden gespeichert und berechnet werden. Zeitzonenwechsel und Sommerzeit-Umstellungen DÜRFEN die gespeicherte Streak-Dauer nicht verändern; die Anzeige erfolgt in der lokalen Zeitzone des Nutzers, die Berechnung ausschließlich in UTC.
- **FR-005**: Tagesgrenzen für Kalender und Statistiken MÜSSEN in UTC ermittelt und anschließend in die lokale Zeitzone des Nutzers umgerechnet werden.
- **FR-006**: Alle Streak- und Abstinenz-Berechnungen MÜSSEN über ein gemeinsames Berechnungsmodul erfolgen, das plattformübergreifend (mobil und web) identische Ergebnisse liefert (Constitution V — testbare Qualitätsstandards).

**Rückfall-Logik**

- **FR-007**: Ein Rückfall MUSS die aktuelle Streak sofort auf 0 zurücksetzen und einen unveränderlichen Rückfall-Eintrag anlegen, der enthält: UTC-Zeitstempel, Dauer der vorigen Streak in Sekunden, optionalen Auslöser-Text (max. 500 Zeichen, frei formulierbar).
- **FR-008**: Der Auslöser-Text eines Rückfalls MUSS für eingeloggte Nutzer serverseitig at-rest mit account-gebundenem Schlüssel verschlüsselt werden (Constitution III). Im Gast-Modus wird er lokal gespeichert ohne account-key-Verschlüsselung (Gast-Modus-Ausnahme, konsistent mit Feature 004 und 006).
- **FR-009**: Der Tages-Status eines Kalendertages für eine gegebene Abgewöhnung MUSS einer von vier Werten sein: **abstinent** (kein Rückfall, nicht pausiert), **Rückfall** (mindestens ein Rückfall-Eintrag an diesem Tag), **pausiert** (Abgewöhnung war an diesem Tag archiviert/pausiert), **neutral** (vor Startdatum oder zukünftig).

**Statistik-Aggregation**

- **FR-010**: Das System MUSS pro Abgewöhnung und Kalendermonat die Anzahl der abstinenten Tage (Zähler) und die Gesamtzahl der relevanten Tage (Nenner) berechnen und als "X von Y Tagen abstinent" anzeigen. **Nenner-Definition**: Für laufende (noch nicht abgeschlossene) Monate ist der Nenner die Anzahl der Tage vom Startdatum (oder Monatsbeginn, falls Startdatum vor dem Monat liegt) bis einschließlich heute. Für abgeschlossene Monate ist der Nenner die vollständige Monatslänge (ab Startdatum oder ab dem ersten Tag des Monats, falls Startdatum vor dem Monat liegt).
- **FR-011**: Das System MUSS pro Abgewöhnung und Kalenderjahr eine analoge Jahresstatistik aggregieren.
- **FR-012**: Für Abgewöhnungen mit periodischem Intervall (z. B. maximal N× pro Woche) MUSS das System die Einhaltung pro Zeitperiode tracken. Eine Periode gilt als **eingehalten**, wenn die Häufigkeit ≤ erlaubter Grenzwert; als **überschritten**, wenn die Häufigkeit > Grenzwert; als **ausstehend**, wenn die Periode noch läuft. **Perioden-Streak-Regeln**: Eine überschrittene Periode setzt die Perioden-Streak auf 0 zurück. Eine ausstehende Periode friert die Streak auf dem zuletzt bestätigten Wert ein (kein Zuwachs, kein Verlust) — der Nutzer wird nicht für eine noch offene Periode bestraft. Eine eingehaltene (abgeschlossene) Periode erhöht die Streak um 1.
- **FR-013**: Statistiken und Kalenderdarstellung MÜSSEN nach einer retroaktiven Startzeit-Änderung (Feature 007) vollständig und korrekt rekalkuliert werden.

**Kalenderansicht**

- **FR-014**: Der Kalender MUSS eine Monatsansicht anbieten mit Tages-Farbcodes: grün (abstinent), rot (Rückfall), grau (pausiert), neutral/leer (vor Startdatum oder Zukunft).
- **FR-015**: Der Kalender MUSS eine Jahresansicht anbieten, die jeden Monat als kompakte Kachel zeigt (Heatmap-Stil). Kacheln für abgeschlossene oder laufende Monate zeigen die Abstinenzrate des Monats als Farbintensität. Kacheln für Monate **vor dem Startdatum** der Abgewöhnung werden als neutrale/leere Kacheln gerendert (kein Farbton, kein Prozentwert, kein Tooltip) — das 12-Monats-Raster bleibt vollständig und keine Kacheln werden ausgeblendet.
- **FR-016**: Ein Tap/Klick auf einen Rückfall-Tag MUSS alle Rückfall-Einträge dieses Tages als chronologisch sortierte, scrollbare Liste anzeigen. Jeder Eintrag zeigt: Rückfall-Zeitpunkt (lokal formatiert) und — wenn vorhanden — den Auslöser-Text. Die Darstellung MUSS würdevoll und nicht-urteilend formuliert sein (Constitution II). Kein Eintrag wird versteckt oder hinter einem weiteren Tap verborgen (Constitution III).
- **FR-017**: Die Kalenderansicht MUSS WCAG 2.2 Level AA erfüllen: Farbcodes MÜSSEN durch ein zweites visuelles Merkmal (Muster oder Symbol) ergänzt werden, damit Nutzer mit Farbfehlsichtigkeit die Tages-Status unterscheiden können (Constitution V).

**Property-based Tests**

- **FR-018**: Das gemeinsame Berechnungsmodul MUSS durch Property-based Tests abgedeckt sein, die mindestens folgende Szenarien generativ prüfen: Zeitzonenwechsel über Datumsgrenzen, Schaltjahre (Feb 29), Sommerzeit-Umstellungen (vor- und zurückstellen), Schaltminuten (Leap seconds werden ignoriert — Standard-UTC-Annahme).

### Key Entities

- **Rückfall-Eintrag**: UTC-Zeitstempel (unveränderlich nach dem Erstellen), Dauer der vorigen Streak in Sekunden, optionaler Auslöser-Text (max. 500 Zeichen, verschlüsselt), Referenz zur Abgewöhnung.
- **Tages-Abstinenz-Status**: Kalenderdate (lokal, von UTC abgeleitet), Abgewöhnung-Referenz, Status (abstinent | Rückfall | pausiert | neutral).
- **Perioden-Compliance**: Nur für intervallbasierte Abgewöhnungen. Periode-Start und -Ende (UTC), erlaubte Häufigkeit, tatsächliche Häufigkeit, Status (eingehalten | überschritten | ausstehend).
- **Verifizierter Zeitstempel**: Server-Zeitstempel eines Online-Abgleichs, verwendeter monotoner Offset (für Offline-Berechnung), Abgewöhnung-Referenz.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Die angezeigte Streak-Dauer weicht im Normalfall (Online) zu keinem Zeitpunkt um mehr als 1 Sekunde von der Server-Zeit ab.
- **SC-002**: Zeitzonenwechsel und Sommerzeit-Umstellungen verursachen in 100 % der durch Property-based Tests abgedeckten Szenarien keinen Streak-Reset und keine Inkonsistenz in der Statistik.
- **SC-003**: Schaltjahre und Schaltmonate werden in 100 % der automatisierten Tests korrekt berechnet (Feb 29 zählt als Kalendertag).
- **SC-004**: Geräteuhr-Manipulation (offline) wird beim nächsten Sync in 100 % der Fälle erkannt und die Streak auf den serverseitig verifizierten Wert korrigiert, ohne dass die Korrektur für den Nutzer als Bestrafung formuliert wird.
- **SC-005**: Monatliche und jährliche Abstinenzstatistiken ("X von Y Tagen") stimmen nach einer retroaktiven Startzeit-Änderung in 100 % der Fälle mit der manuellen Nachzählung überein.
- **SC-006**: Die Kalenderansicht besteht automatisierte WCAG-2.2-AA-Checks; Farbcodes sind durch ein zweites visuelles Merkmal ergänzt und durch Screen-Reader-Test verifizierbar.
- **SC-007**: Die Ladezeit der Kalenderansicht (Monatsansicht) beträgt für mindestens 95 % der Aufrufe unter 300 ms auf Zielgeräten (Constitution V — P95 ≤ 300 ms).
- **SC-008**: Property-based Tests für das Berechnungsmodul decken Zeitzonenwechsel, Schaltjahre und DST-Szenarien ab und laufen fehlerfrei im CI.

## Assumptions

- Tagesgrenzen werden in UTC berechnet und zur Anzeige in die aktuelle Geräte-Zeitzone des Nutzers umgerechnet. Es gibt keine separate "Heimat-Zeitzone"-Einstellung — die Geräte-Zeitzone gilt als Anzeige-Zeitzone.
- "Pausiert" entspricht den Tagen, an denen eine Abgewöhnung den archivierten/pausierten Zustand hatte (Feature 005). Es gibt keinen manuellen Tages-Pause-Mechanismus außerhalb der Archivierung.
- Der Auslöser-Text eines Rückfalls ist identisch mit dem Feld aus Feature 006 (Dashboard) — kein neues Daten-Objekt, sondern dasselbe Rückfall-Konzept.
- Für periodische Intervall-Abgewöhnungen (z. B. max. N× pro Woche) ersetzt das Konzept "Perioden-Compliance-Streak" den klassischen Tages-Streak. Beide Streak-Konzepte koexistieren im System, werden aber je nach Abgewöhnungs-Typ angezeigt.
- Ein Rückfall-Eintrag ist nach dem Erstellen unveränderlich (Zeitstempel, vorige Streak-Dauer). Der Auslöser-Text ist nach dem Speichern nicht mehr editierbar (Konsistenz mit Feature 006).
- Schaltminuten (Leap seconds) werden nicht berücksichtigt — Standard-UTC-Annahme, konsistent mit gängigen Betriebssystem-APIs.
- Die Berechnungen orientieren sich am gregorianischen Kalender.
- Das gemeinsame Berechnungsmodul ist ein plattformunabhängiges Modul (bereits in der Projekt-Architektur für Streak/XP/Achievement vorgesehen, Constitution V).

## Constitution Alignment *(mandatory)*

### User Welfare Impact

Diese Funktion schafft die Voraussetzung für akkurate Fortschrittsdarstellung — ein zentrales Vertrauensinstrument. Eine manipulationssichere Zeitquelle verhindert, dass Nutzer durch falsche Streak-Werte in ein falsches Sicherheitsgefühl oder in ungerechtfertigte Enttäuschung geführt werden. Die Statistik-Aggregation ("X von Y Tagen") zeigt, dass Fortschritt trotz Rückfällen real und messbar ist — dies stärkt die Selbstwirksamkeit (Constitution I). Kein Mechanismus optimiert auf Engagement oder Streak-Maximierung als Selbstzweck; die Daten dienen ausschließlich der Selbstreflexion des Nutzers.

### Relapse Handling

Ein Rückfall wird als normales Ereignis im Abstinenzprozess behandelt: Er setzt die aktuelle Streak zurück, aber alle bisherigen Fortschrittsdaten (Statistiken, Kalenderhistorie, frühere Streak-Dauern) bleiben vollständig erhalten und sichtbar. Die Korrektur einer manipulierten Gerätezeit nach Sync wird sachlich und nicht-urteilend kommuniziert (kein "Betrug erkannt"-Framing). Tage mit Rückfällen sind im Kalender als Rückfall-Tage sichtbar, werden aber gleichwertig neben abstinenten Tagen dargestellt — ohne Gesamtbewertung des Nutzers.

Acceptance Scenario für Rückfall-Recovery: **Given** eine Abgewöhnung mit 3 Rückfällen im Monat, **When** der Nutzer die Monatsstatistik öffnet, **Then** sieht er "X von Y Tagen abstinent" mit dem Fokus auf dem Erreichten, nicht auf den Rückfällen.

### Privacy & Data Minimization

Neue Datenelemente durch dieses Feature:
- **Rückfall-Eintrag** (UTC-Zeitstempel, vorige Streak-Dauer, optionaler Auslöser-Text): Zeitstempel und Dauer sind für die korrekte Berechnung zwingend erforderlich. Auslöser-Text ist optional und dient ausschließlich der persönlichen Reflexion des Nutzers.
- **Auslöser-Text** wird für eingeloggte Nutzer serverseitig at-rest mit account-gebundenem Schlüssel verschlüsselt; für Gast-Nutzer lokal ohne Verschlüsselung gespeichert.
- **Verifizierter Server-Zeitstempel**: Minimale Metadaten für die Manipulationserkennung; kein Bewegungsprofil, keine Geolokation.
- Alle oben genannten Daten unterliegen dem Nutzer-Daten-Export und dem Löschpfad (Constitution III).
- Statistik-Aggregationen werden clientseitig aus den ohnehin gespeicherten Tages-Status berechnet — keine separate Speicherung von Aggregaten.

### Clarity & Everyday UX

Der primäre Nutzerpfad ist: Nutzer öffnet Dashboard → sieht sekundengenaue Streak → bei Bedarf öffnet er die Kalenderansicht → sieht Muster → öffnet optional die Statistik. Jeder dieser Schritte ist eine eigenständige Ansicht mit einer Kernaussage. Die Kalenderansicht ist selbsterklärend durch Farbcodes und Legende. Statistiken verwenden Alltagssprache ("23 von 30 Tagen abstinent"), keine Fachbegriffe. Die Korrektur-Benachrichtigung bei Sync (Manipulations-Erkennung) erscheint als sachlicher Toast-Hinweis — kein Modal, kein Alarmton (Constitution IV).

### Quality Budgets

- **Zuverlässigkeit**: Streak-Berechnung und Kalender-Rendering gelingen in ≥ 99 % aller Aufrufe korrekt (Constitution V).
- **Zugänglichkeit**: Kalenderansicht erfüllt WCAG 2.2 AA; Farbcodes sind durch ein zweites Merkmal ergänzt (Constitution V).
- **Performance**: P95 Ladezeit der Kalenderansicht (Monat) ≤ 300 ms auf Zielgeräten (Constitution V).
- **Testbarkeit**: Property-based Tests decken Zeitzonen, Schaltjahre, DST ab; CI muss grün sein vor Release (Constitution V).

## Scope Classification *(mandatory)*

- **Feature classification**: MVP
- **Scope rationale**: Die korrekte Streak- und Abstinenz-Berechnung ist das Fundament aller anderen Features (Dashboard, Achievements, XP, Kalender). Ohne akkurate Berechnung sind alle anderen Darstellungen wertlos. Die Kalenderansicht und Statistik-Aggregation sind die direkte Nutzerausgabe dieser Berechnungslogik und bilden zusammen die Mindestvisualisierung für sinnvolle Selbstreflexion.

## Goal Conflict Resolution *(mandatory)*

- **Genauigkeit vs. Offline-Verfügbarkeit**: Im Offline-Modus ist die Anzeige nicht mehr verifizierbar gegen den Server. Entscheidung (Constitution I > III): Offline-Verfügbarkeit hat Vorrang, da die Streak den Nutzer motiviert und eine Unterbrechung der Anzeige schädlicher wäre als eine vorübergehend nicht-verifizierte Anzeige. Die Korrektur erfolgt transparent beim nächsten Sync.
- **Manipulationsschutz vs. Constitution II (Rückfall ohne Scham)**: Die Erkennung einer Zeitmanipulation könnte als Vorwurf wahrgenommen werden. Entscheidung (Constitution II > Technische Korrektheit): Die Korrekturbenachrichtigung MUSS neutral formuliert sein ("Deine Streak wurde mit dem Server abgeglichen"); kein Hinweis auf Absicht.

## Feature Definition of Done *(mandatory)*

- [ ] Constitution alignment evidence attached
- [ ] Tests implemented for primary, failure, and relapse paths
- [ ] Property-based tests cover timezones, leap years, DST
- [ ] Accessibility checks passed (automated + manual spot-check, WCAG 2.2 AA)
- [ ] Reliability and performance evidence attached (P95 ≤ 300 ms, ≥ 99 % success rate)
- [ ] Privacy impact review completed (relapse trigger encryption, server timestamp minimization)
- [ ] Rollout and rollback notes prepared

## Clarifications

### Session 2026-05-30

- Q: Wie wird der Nenner Y der Monatsstatistik für den laufenden Monat bestimmt? → A: Nenner = Tage ab Startdatum bis einschließlich heute (für laufende Monate). Für abgeschlossene Monate gilt die vollständige Monatslänge.
- Q: In welche Richtung wird Geräteuhr-Manipulation erkannt? → A: Nur bei negativer Abweichung (Geräteuhr < Serverzeit, d. h. Uhr wurde zurückgestellt). Eine vorausgehende Geräteuhr (> Serverzeit) wird still korrigiert ohne Nutzer-Hinweis (Constitution II — kein ungerechtfertigter Vorwurf).
- Q: Wie viele Rückfälle werden beim Tap auf einen Mehrfach-Rückfall-Tag im Kalender angezeigt? → A: Alle Rückfälle des Tages, chronologisch sortiert (Zeitpunkt + Auslöser falls vorhanden), als scrollbare Liste. Kein Verstecken von Einträgen (Constitution III — vollständige Datenhoheit).
- Q: Was setzt die Perioden-Streak bei intervallbasierter Abgewöhnung zurück? → A: Nur eine explizit überschrittene Periode (Häufigkeit > Grenzwert). Eine ausstehende (noch laufende) Periode friert die Streak auf dem letzten bestätigten Wert ein — kein Zuwachs, kein Verlust.
- Q: Wie werden Monate vor dem Startdatum in der Jahres-Heatmap dargestellt? → A: Als neutrale/leere Kacheln (kein Farbton, kein Prozentwert, kein Tooltip). Das 12-Monats-Raster bleibt vollständig; kein Ausblenden von Kacheln.
