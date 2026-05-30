# Feature Specification: Quitly Habit Reduction MVP

**Feature Branch**: `002-before-specify-hook`

**Created**: 2026-05-28

**Status**: Draft

**Input**: User description: "Baue Quitly, eine WebApp, mit der Nutzer schlechte Gewohnheiten reduzieren oder beenden koennen (z. B. Rauchen, Social Media, Zucker, Impulskaeufe). Fokus auf das Was und Warum, nicht auf Tech-Details. MVP-Funktionen: Onboarding mit Gewohnheitsziel, taeglicher Check-in (Status, Stimmung, Trigger), Streak-Tracking, Rueckfall-Erfassung mit Recovery-Flow, Craving-Notfallhilfe, Reminder, Wochen-Insights. Erstelle eine vollstaendige Spezifikation mit Problem Statement, Ziele/Nicht-Ziele, Personas, User Stories mit Akzeptanzkriterien, Edge Cases und messbaren MVP-KPIs."

## Problem Statement

Viele Menschen scheitern beim Reduzieren oder Beenden schlechter Gewohnheiten nicht an fehlendem Wissen, sondern an fehlender alltagstauglicher Unterstuetzung in kritischen Momenten. Bestehende Loesungen setzen haeufig auf Druck, Scham oder Engagement-Mechaniken statt auf nachhaltige Verhaltensaenderung. Quitly soll eine verlaessliche, nicht-beschaemende und praxisnahe Begleitung bieten, die Rueckfaelle als Teil des Prozesses behandelt und Nutzern hilft, handlungsfaehig zu bleiben.

## Clarifications

### Session 2026-05-28

- Q: Wie wird ein Rueckfall im MVP definiert? → A: Rueckfall nur bei Nutzer-Selbstmarkierung; sonst keine automatische Klassifikation.
- Q: Welche Streak-Regel gilt im MVP? → A: Streak zaehlt nur bei komplett abstinenten Tagen; jeder Rueckfall setzt auf 0.
- Q: Welche Reminder-Logik gilt im MVP? → A: Keine aktiven Reminder im MVP, nur passive Hinweise in der App.
- Q: Welche Datenschutz-Einwilligung gilt im MVP? → A: Keine Analytics-Einwilligung, da keinerlei Analytics im MVP.
- Q: Welche Grenzen gelten fuer den MVP-Umfang? → A: MVP enthaelt Onboarding, Check-in, Streak, minimalen Recovery-Flow, passive Hinweise und Insights; Craving-Notfallhilfe bleibt Post-MVP.

## Ziele

- Nutzer koennen ein klares, persoenliches Gewohnheitsziel definieren und ihren Fortschritt taeglich nachvollziehen.
- Nutzer erkennen durch Wochen-Insights Muster zwischen Stimmung, Triggern und Fortschritt.
- Das MVP liefert frueh messbaren Nutzen fuer Kontinuitaet, Selbstwirksamkeit und Abbruchreduktion.

## Nicht-Ziele

- Keine soziale Rangliste, kein Wettbewerbsmodus und keine oeffentlichen Erfolgspunkte.
- Keine medizinische Diagnose, Therapie oder Krisenintervention im klinischen Sinn.
- Keine aufwaendige Personalisierung durch komplexe KI-Profile im MVP.
- Kein Multi-Habit-Portfolio-Management im MVP (ein primaeres Ziel je Nutzer).
- Keine Craving-Notfallhilfe im MVP (Post-MVP).

## Personas

### Persona 1: Neustarterin Nora

- Motivation: Will eine schaedliche Alltagsgewohnheit erstmals ernsthaft reduzieren.
- Herausforderung: Unsicher, wann und warum Rueckfaelle passieren.
- Bedarf: Klarer Start, einfacher Tagesablauf, schnelle Erfolge ohne Druck.

### Persona 2: Wiederanfaenger Ben

- Motivation: Hat bereits mehrere Abbruchversuche hinter sich und will diesmal dranbleiben.
- Herausforderung: Scham nach Rueckfaellen fuehrt oft zu komplettem Abbruch.
- Bedarf: Rueckfallfreundlicher Recovery-Flow, der Kontinuitaet statt Perfektion foerdert.

### Persona 3: Berufstaetige Aylin

- Motivation: Will impulsive Gewohnheiten trotz engem Zeitplan reduzieren.
- Herausforderung: Wenig Zeit, hoher Stress, vergisst regelmaessige Reflexion.
- Bedarf: Kurze Check-ins, hilfreiche Reminder, klare Wochenzusammenfassung.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Ziel setzen und starten (Priority: P1)

Als Nutzer moechte ich beim Onboarding ein konkretes Gewohnheitsziel festlegen, damit ich mit einer klaren Richtung starte und meine taeglichen Eintraege darauf beziehen kann.

**Why this priority**: Ohne Zieldefinition fehlt der Referenzrahmen fuer alle weiteren MVP-Funktionen.

**Independent Test**: Ein neuer Nutzer kann Onboarding abschliessen, sieht sein aktives Ziel und kann danach den ersten Check-in erfolgreich anlegen.

**Acceptance Scenarios**:

1. **Given** ein neuer Nutzer startet Quitly, **When** er ein Gewohnheitsziel auswaehlt und ein Reduktions- oder Stoppziel bestaetigt, **Then** wird das Ziel gespeichert und als aktives Hauptziel angezeigt.
2. **Given** ein Nutzer befindet sich im Onboarding, **When** er unklare oder zu vage Zielangaben macht, **Then** erhaelt er eine verstaendliche Hilfestellung zur Praezisierung und kann trotzdem fortfahren.

---

### User Story 2 - Taeglich reflektieren und Progress sehen (Priority: P1)

Als Nutzer moechte ich taeglich einen kurzen Check-in zu Status, Stimmung und Triggern machen, damit ich Fortschritt sichtbar halte und Muster erkenne.

**Why this priority**: Regelmaessige Selbstbeobachtung ist der Kernmechanismus fuer nachhaltige Veraenderung.

**Independent Test**: Ein Nutzer kann an drei aufeinanderfolgenden Tagen Check-ins erfassen und sieht aktualisierte Streak-Informationen.

**Acceptance Scenarios**:

1. **Given** ein aktives Gewohnheitsziel, **When** der Nutzer einen Check-in mit Status, Stimmung und Trigger erfasst, **Then** wird der Tagesstatus gespeichert und in der Verlaufssicht angezeigt.
2. **Given** vorhandene Check-ins, **When** der Nutzer die Progress-Uebersicht oeffnet, **Then** sieht er aktuelle Streak-Informationen und den Zusammenhang zu den letzten Tagesmeldungen.

---

### User Story 3 - Rueckfall ohne Scham verarbeiten (Priority: P1)

Als Nutzer moechte ich einen Rueckfall erfassen koennen und sofort in einen Recovery-Flow gefuehrt werden, damit ich weitermache statt abzubrechen.

**Why this priority**: Rueckfaelle sind erwartbar; ohne Recovery-Mechanik sinkt die Wahrscheinlichkeit langfristiger Verhaltensaenderung.

**Independent Test**: Ein Nutzer markiert einen Rueckfall und durchlaeuft den Recovery-Flow bis zu einem neuen naechsten Schritt fuer den Folgetag.

**Acceptance Scenarios**:

1. **Given** ein Nutzer erlebt einen Rueckfall, **When** er diesen dokumentiert, **Then** wird kein beschaemender Text angezeigt und stattdessen ein neutrales Recovery-Feedback mit naechster Aktion angeboten.
2. **Given** ein dokumentierter Rueckfall, **When** der Nutzer den Recovery-Flow abschliesst, **Then** bleibt der bisherige Verlauf sichtbar und der Nutzer erhaelt einen konkreten Wiedereinstiegspunkt.

---

### User Story 4 - Craving-Moment akut ueberbruecken (Priority: Post-MVP)

Status: Post-MVP (nicht Teil dieses MVP-Scope). Priorisierung (vormals P2) wird erst bei Aktivierung nach MVP final festgelegt.

Als Nutzer moechte ich in akuten Craving-Situationen sofort Notfallhilfe aufrufen, damit ich den Impuls unterbrechen kann.

**Why this priority**: Akute Momente entscheiden ueber kurzfristige Rupturen; schnelle Hilfe reduziert Rueckfallwahrscheinlichkeit.

**Independent Test**: Ein Nutzer startet die Notfallhilfe, waehlt eine Sofortstrategie und dokumentiert danach den Ausgang.

**Acceptance Scenarios**:

1. **Given** ein akutes Craving, **When** der Nutzer die Notfallhilfe oeffnet, **Then** erhaelt er innerhalb weniger Schritte konkrete, alltagstaugliche Sofortoptionen.
2. **Given** der Nutzer hat eine Soforthilfe genutzt, **When** er den Ausgang eintraegt, **Then** wird das Ergebnis im Verlauf festgehalten und fuer Insights beruecksichtigt.

---

### User Story 5 - Dranbleiben durch passive Hinweise und Wochen-Insights (Priority: P2)

Als Nutzer moechte ich hilfreiche passive Hinweise in der App und eine woechentliche Zusammenfassung erhalten, damit ich regelmaessig reflektiere und Muster in Triggern und Stimmung erkenne.

**Why this priority**: Kontinuitaet und Musterlernen verbessern die Erfolgschance ueber die Anfangsmotivation hinaus.

**Independent Test**: Ein Nutzer sieht bei ausstehendem Check-in einen passiven Hinweis in der App und sieht am Wochenende eine Insight-Zusammenfassung mit handlungsrelevanten Erkenntnissen.

**Acceptance Scenarios**:

1. **Given** ein Nutzer hat noch keinen Tages-Check-in, **When** er die App oeffnet, **Then** wird ein freundlicher passiver Hinweis zum Check-in angezeigt.
2. **Given** eine abgeschlossene Woche mit Check-in-Daten, **When** der Nutzer die Wochen-Insights oeffnet, **Then** sieht er Trends fuer Status, Stimmung, Trigger und konkrete Reflexionshinweise.

### Edge Cases

- Nutzer ueberspringt mehrere Tage: Fehlende abstinente Tage unterbrechen die aktuelle Streak nachvollziehbar.
- Nutzer waehlt keine Trigger bei Check-in: Eintrag bleibt moeglich, Trigger-Feld wird als "nicht angegeben" behandelt.
- Passiver Hinweis erscheint waehrend bereits laufendem Check-in: Kein zusaetzlicher Unterbrechungs-Hinweis, nur konsistente Nutzerfuehrung.
- Sehr wenige Wochen-Daten vorhanden: Insight-Sicht zeigt reduzierte, aber sinnvolle Aussagen statt irrefuehrender Trends.
- Nutzer deaktiviert passive Hinweise: System respektiert sofort die Entscheidung ohne erneute Aktivierungsaufforderung.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST Nutzer beim ersten Start durch ein Onboarding mit genau einem primaeren Gewohnheitsziel fuehren.
- **FR-002**: System MUST dem Nutzer ermoeglichen, zwischen Reduzieren und Beenden als Zielrichtung zu waehlen.
- **FR-003**: System MUST das aktive Gewohnheitsziel persistent speichern und jederzeit sichtbar machen.
- **FR-004**: System MUST taegliche Check-ins mit mindestens Status, Stimmung und Triggern erfassen koennen.
- **FR-005**: System MUST auch unvollstaendige Check-ins speichern koennen, solange ein Mindeststatus angegeben ist.
- **FR-006**: System MUST den taeglichen Verlauf chronologisch und fuer Nutzer verstaendlich darstellen.
- **FR-007**: System MUST einen Streak ausschliesslich auf Basis aufeinanderfolgender komplett abstinenter Tage berechnen.
- **FR-007a**: System MUST die aktuelle Streak auf 0 setzen, wenn ein Tag als nicht abstinent markiert ist.
- **FR-007b**: System MUST bei fehlendem abstinenten Tagesnachweis die aktuelle Streak unterbrechen.
- **FR-008**: System MUST bei Rueckfall-Eintrag unmittelbar einen nicht-beschaemenden Recovery-Flow anbieten.
- **FR-008a**: System MUST einen Rueckfall nur dann als Rueckfall klassifizieren, wenn Nutzer ihn aktiv selbst markieren; keine automatische Rueckfall-Klassifikation aus anderen Signalen.
- **FR-009**: System MUST im Recovery-Flow mindestens einen konkreten naechsten Schritt fuer die naechsten 24 Stunden erfassen.
- **FR-010**: System MUST den Verlauf auch nach Rueckfaellen sichtbar halten, statt Fortschritt vollstaendig zurueckzusetzen.
- **FR-013**: System MUST bei ausstehendem taeglichem Check-in einen passiven In-App-Hinweis bereitstellen.
- **FR-014**: System MUST passive Hinweise standardmaessig respektvoll und druckfrei formulieren.
- **FR-015**: System MUST woechentliche Insights zu Status-, Stimmungs- und Triggermustern erzeugen.
- **FR-016**: System MUST Insights als leicht verstaendliche Aussagen mit mindestens einem umsetzbaren Hinweis darstellen.
- **FR-017**: System MUST dem Nutzer erlauben, passive Hinweise jederzeit ohne Nachteile zu deaktivieren.
- **FR-018**: System MUST Datenverarbeitung auf fuer MVP notwendige Nutzerdaten begrenzen.
- **FR-018a**: System MUST im MVP keinerlei Analytics-Tracking-Events erheben oder uebermitteln.
- **FR-018b**: System MUST daher im MVP keine Analytics-Einwilligungsabfrage anzeigen.
- **FR-019**: System MUST dem Nutzer ermoeglichen, Check-in-Eintraege nachtraeglich zu korrigieren.
- **FR-020**: System MUST Onboarding bei P95 in <= 3 Minuten und taeglichen Check-in bei P95 in <= 90 Sekunden ermoeglichen.
- **FR-020a**: System MUST das Oeffnen der Wochen-Insights bei P95 in <= 2 Sekunden ermoeglichen.
- **FR-021**: System MUST Craving-Notfallhilfe als Post-MVP kennzeichnen und im MVP nicht aktivieren.

### Key Entities *(include if feature involves data)*

- **User Profile**: Relevante Nutzergrunddaten fuer Zielfuehrung, Reminder-Praeferenz und Fortschrittskontext.
- **Habit Goal**: Primaeres Reduktions- oder Beendigungsziel mit Startzeitpunkt, Zielrichtung und Aktivstatus.
- **Daily Check-in**: Tagesmeldung mit Status, Stimmung, Triggern, optionalen Notizen und Zeitbezug.
- **Streak Record**: Abgeleitete Fortschrittsmetrik fuer aufeinanderfolgende positive Tage im Kontext des aktiven Ziels.
- **Relapse Event**: Vom Nutzer aktiv markierter Rueckfall mit situativem Kontext und Recovery-Verknuepfung.
- **Recovery Plan Step**: Konkreter Wiedereinstiegsschritt fuer die naechsten 24 Stunden.
- **Reminder Preference**: MVP: Aktivstatus passiver In-App-Hinweise; Post-MVP: optionale aktive Reminder-Parameter.
- **Weekly Insight**: Verdichtete Wochenauswertung aus Check-ins, Triggern, Stimmung und beobachtbaren Trends.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Mindestens 80% neuer Nutzer schliessen das Onboarding in unter 3 Minuten ab.
- **SC-002**: Mindestens 65% der aktiven Nutzer erfassen in Woche 1 an mindestens 5 von 7 Tagen einen Check-in.
- **SC-003**: Mindestens 80% der aktiven Nutzer sehen in Woche 2 mindestens einen Wochen-Insight mit einem klaren Musterhinweis.
- **SC-004**: Mindestens 70% der Nutzer mit passivem In-App-Hinweis starten in derselben Session einen Check-in oder schliessen ihn ab.
- **SC-005**: Mindestens 75% der aktiven Nutzer oeffnen die Wochen-Insights mindestens einmal pro Woche.
- **SC-006**: Mindestens 75% der als Rueckfall markierten Tage fuehren innerhalb von 24 Stunden zu einem abgeschlossenen Recovery-Plan-Schritt.
- **SC-007**: Mindestens 90% der Nutzer bewerten den taeglichen Check-in als "einfach" oder "sehr einfach".

### MVP KPIs

- **KPI-001 (Activation)**: Anteil neuer Nutzer mit abgeschlossenem Onboarding und erstem Check-in innerhalb von 24 Stunden >= 70%.
- **KPI-002 (Early Retention)**: Anteil neuer Nutzer mit Check-in an Tag 7 >= 45%.
- **KPI-003 (Consistency)**: Anteil Nutzer mit mindestens 5 Check-ins pro 7 Tage >= 50%.
- **KPI-004 (Prompt Utility)**: Anteil Nutzer, die nach passivem In-App-Hinweis innerhalb derselben Session einen Check-in erfassen >= 55%.
- **KPI-005 (Insight Utility)**: Anteil Nutzer, die nach Wochen-Insights mindestens eine reflektierte Anpassung (z. B. Trigger-Vermeidungsplan) dokumentieren >= 40%.
- **KPI-006 (Recovery Continuity)**: Anteil Rueckfall-Nutzer mit abgeschlossenem Recovery-Plan-Schritt innerhalb von 24 Stunden >= 75%.

Hinweis: KPI-Messung im MVP basiert ausschliesslich auf produktnotwendigen Fachdaten (z. B. Check-ins, Recovery-Schritte und Insights-Nutzung) und nicht auf separaten Analytics-Events.

## Assumptions

- MVP richtet sich an erwachsene Selbstnutzer ohne klinischen Betreuungsanspruch.
- Ein primaeres Gewohnheitsziel pro Nutzer ist fuer MVP ausreichend und erhoeht Klarheit.
- Nutzer koennen taeglich mindestens 1-3 Minuten fuer Check-in investieren.
- Standardfall ist regulaere Internetverfuegbarkeit waehrend taeglicher Nutzung.
- Aktive Benachrichtigungs-Reminder sind im MVP nicht enthalten; Hinweise erfolgen nur passiv in der App.
- Im MVP gibt es keine separaten Analytics-Einwilligungen, da keine Analytics-Datenerhebung erfolgt.
- Wochen-Insights basieren auf den im jeweiligen Zeitraum verfuegbaren Selbstauskuenften.

## Constitution Alignment *(mandatory)*

### User Welfare Impact

- Die Spezifikation priorisiert Kontinuitaet, Selbstwirksamkeit und schadensarme Verhaltensaenderung statt Nutzungsmaximierung.
- Keine Funktion erfordert Druckmechaniken, soziale Beschamung oder wettbewerbsgetriebenes Engagement.

### Relapse Handling

- Rueckfaelle werden im MVP als minimaler, nicht-beschaemender Recovery-Flow umgesetzt.
- Rueckfall-Klassifikation erfolgt ausschliesslich durch aktive Nutzer-Selbstmarkierung.

### Privacy & Data Minimization

- Erfasst werden nur Daten fuer Zielverfolgung, taegliche Reflexion, Recovery und Insights.
- Im MVP werden keine aktiven Reminder versendet; passive Hinweise koennen nutzerkontrolliert angepasst oder deaktiviert werden.
- Datenkorrekturen durch Nutzer sind vorgesehen, um Kontrolle und Transparenz zu wahren.
- Im MVP werden keine Analytics-Events erhoben, daher ist keine Analytics-Einwilligung erforderlich.

### Clarity & Everyday UX

- Alle Kernablaeufe sind auf kurze Nutzung in Alltagssituationen ausgelegt.
- Pro Schritt steht die unmittelbare Handlungsfaehigkeit des Nutzers im Fokus.

### Quality Budgets

- Reliability target(s): Mindestens 99% erfolgreiche Durchlaeufe der Kernreisen (Onboarding, Check-in, Recovery, Wochen-Insights) in regulaeren Betriebsfenstern.
- Accessibility target(s): WCAG 2.2 AA fuer alle MVP-Kernflows; Tastaturbedienbarkeit, ausreichende Kontraste, verstaendliche Sprache.
- Performance target(s): Onboarding P95 <= 3 Minuten (FR-020), taeglicher Check-in P95 <= 90 Sekunden (FR-020), Oeffnen der Wochen-Insights P95 <= 2 Sekunden (FR-020a).

## Scope Classification *(mandatory)*

- Feature classification: MVP
- Scope rationale: MVP ist bewusst auf Onboarding, taeglichen Check-in, strikte Streak-Logik, minimalen Recovery-Flow, passive In-App-Hinweise und Wochen-Insights begrenzt, um einen klaren, testbaren Kern zu sichern.
- Post-MVP scope: Craving-Notfallhilfe wird nach MVP validiert und separat geplant.

## Goal Conflict Resolution *(mandatory)*

- Erwartete Konflikte: Hinweis-Haeufigkeit vs Nutzerwohl; Insight-Tiefe vs Datenminimierung; Geschwindigkeit der Eingabe vs Vollstaendigkeit der Reflexion.
- Entscheidungsregel: Bei Konflikten gilt die Verfassungsreihenfolge (Nutzerwohl und nicht-beschaemender Umgang vor Privacy, dann Qualitaetsbudgets, dann UX-Vereinfachung, dann Wachstumsziele).

## Feature Definition of Done *(mandatory)*

- [ ] Constitution alignment evidence attached
- [ ] Tests implemented for primary, failure, and relapse paths
- [ ] Accessibility checks passed (automated + manual spot-check)
- [ ] Reliability and performance evidence attached
- [ ] Privacy impact review completed
- [ ] Rollout and rollback notes prepared
