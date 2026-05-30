# Tasks: Präzise Streak- und Abstinenz-Berechnung inkl. Rückfall-Logik

**Branch**: `008-streak-calculation` | **Date**: 2026-05-30
**Spec**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md)

## Format: `[ID] [P?] [Story?] Description — file path`

- **[P]**: Parallelisierbar (unterschiedliche Dateien, keine offenen Abhängigkeiten)
- **[Story]**: User Story label (US1, US2, US3)
- Foundational- und Setup-Phasen tragen kein Story-Label

---

## Phase 1: Setup

**Purpose**: Neue Abhängigkeiten installieren und Testinfrastruktur bereitstellen.

- [X] T001 `fast-check` als devDependency installieren — `frontend/package.json`
- [X] T002 [P] `FsCheck.Xunit` NuGet-Paket zu Backend-Tests hinzufügen — `backend/tests/`
- [X] T003 [P] Verzeichnis `backend/src/Domain/Calculation/` anlegen
- [X] T004 [P] Verzeichnis `frontend/src/lib/streak-calc/` anlegen
- [X] T005 [P] Verzeichnis `backend/src/Api/Streaks/` anlegen
- [X] T006 [P] Verzeichnis `backend/tests/Streaks/` anlegen

---

## Phase 2: Foundational (Blocker für alle User Stories)

**Purpose**: Reine Berechnungsmodule (Backend + Frontend), Schema-Migration und Feldverschlüsselung. Kein User-Story-Work beginnt, bis diese Phase abgeschlossen ist.

**⚠️ KRITISCH**: Phase A und Phase E (Backend-Domain + Frontend-Calc) können parallel entwickelt werden. Phase B/C/D warten auf Phase A. Phase F/G warten auf Phase E.

### Backend: Domain-Berechnungsmodul (Phase A)

- [X] T007 `StreakCalculator.cs` erstellen mit `CalculateCurrentSeconds(startedAt, relapses, now)` — `backend/src/Domain/Calculation/StreakCalculator.cs`
- [X] T008 `CalculateDayStatus(date, tz, startedAt, relapses)` zu `StreakCalculator.cs` hinzufügen — `backend/src/Domain/Calculation/StreakCalculator.cs`
- [X] T009 `CalculateMonthStats(year, month, tz, startedAt, relapses, today)` mit Today-gedeckeltem Nenner für laufende Monate hinzufügen — `backend/src/Domain/Calculation/StreakCalculator.cs`
- [X] T010 `CalculateYearStats(year, tz, startedAt, relapses, today)` hinzufügen — `backend/src/Domain/Calculation/StreakCalculator.cs`
- [X] T011 `DetectManipulation(offlineDeltaMs, serverDeltaMs)` hinzufügen (nur negative Abweichung > 300 000 ms) — `backend/src/Domain/Calculation/StreakCalculator.cs`
- [X] T012 `DayStatus`-Enum, `MonthStats`- und `YearStats`-Records anlegen — `backend/src/Domain/Calculation/StreakCalculator.cs`
- [X] T013 Unit-Tests für alle Berechnungsfunktionen inkl. Edge Cases (Mitternacht UTC, DST-Grenze, Schaltjahr 29. Feb, Rückfall am Starttag, mehrere Rückfälle am selben Tag) — `backend/tests/Streaks/StreakCalculatorTests.cs`
- [X] T014 FsCheck Property-based Tests: `CalculateCurrentSeconds` nie negativ; Timezone-Shift ändert Sekunden nicht; Rückfall reduziert Streak; `MonthStats.RelevantDays` ≤ Tage-im-Monat — `backend/tests/Streaks/StreakCalculatorPropertyTests.cs`

### Backend: Schema-Migration (Phase B — nach T007–T014)

- [X] T015 `Habit.cs` aktualisieren: `StartedAt DateTimeOffset?` hinzufügen — `backend/src/Domain/Entities/Habit.cs`
- [X] T016 `Streak.cs` neu gestalten: per-Habit-Schema (id, habit_id FK UNIQUE, current_streak_seconds, last_server_utc_ms, last_sync_at) — `backend/src/Domain/Entities/Streak.cs`
- [X] T017 `Relapse.cs` aktualisieren: `PreviousStreakSeconds long` hinzufügen, `ContextNote string?` → `ContextNoteEncrypted byte[]?` umbenennen/ändern — `backend/src/Domain/Entities/Relapse.cs`
- [X] T018 `QuitlyDbContext.cs` aktualisieren: `bytea`-Konfiguration für `ContextNoteEncrypted`, neues Streak-Entity-Config — `backend/src/Persistence/QuitlyDbContext.cs`
- [X] T019 EF Core Migration `AddStreakCalculation` erstellen: `started_at` aus `started_on AT TIME ZONE 'UTC'` befüllen; alte Streak-Spalten entfernen; neue Streak-Spalten hinzufügen — `backend/src/Persistence/Migrations/`
- [X] T020 Migration lokal anwenden und Datenkonsistenz prüfen — `backend/src/Persistence/`

### Backend: Infrastruktur & Services (Phase C — nach T015–T020)

- [X] T021 `FieldEncryptor.cs` mit `IDataProtectionProvider` erstellen: `Encrypt(plaintext, userId)` und `Decrypt(bytes, userId)` mit user-scoped Purpose-String — `backend/src/Infrastructure/Security/FieldEncryptor.cs`
- [X] T022 `StreakService.cs` neu schreiben: `GetStreakAsync(habitId)` — lädt Habit + Relapse-Zeitstempel, ruft `StreakCalculator.CalculateCurrentSeconds` auf, persistiert `Streak`, gibt `StreakDto` mit `serverUtcMs` zurück — `backend/src/Application/Streaks/StreakService.cs`
- [X] T023 `SyncAndCorrectAsync(habitId, offlineDeltaMs)` **nicht implementieren** (kein API-Eingangsparameter für `offlineDeltaMs` vorhanden; Manipulation wird client-seitig erkannt, s. T035). `GetStreakAsync` gibt stets server-verifizierte `currentStreakSeconds` zurück — das ist die implizite Korrektur. `DetectManipulation` bleibt als pure Funktion in `StreakCalculator` für Testbarkeit — `backend/src/Application/Streaks/StreakService.cs`
- [X] T024 `CalendarService.cs` erstellen: lädt Relapse-Zeitstempel für Monat, ruft `CalculateDayStatus` pro Tag auf, entschlüsselt Notizen via `FieldEncryptor` — `backend/src/Application/Streaks/CalendarService.cs`
- [X] T025 `StatsService.cs` erstellen: ruft `CalculateMonthStats` und `CalculateYearStats` auf — `backend/src/Application/Streaks/StatsService.cs`
- [X] T026 Alle Services in `Program.cs` registrieren — `backend/src/Program.cs`

### Frontend: Berechnungsmodul (Phase E — parallel zu Phase A/B/C)

- [X] T027 [P] `streak-calc/index.ts` erstellen: 5 Pure Functions mit `Intl.DateTimeFormat` für Timezone-Grenzen, null externe Abhängigkeiten — `frontend/src/lib/streak-calc/index.ts`
- [X] T028 [P] `monotonic-store.ts` erstellen: `saveSnapshot`, `loadSnapshot`, `getNowUtcMs`; Stale-Guard (> 24 h → fallback zu `Date.now()`) — `frontend/src/lib/streak-calc/monotonic-store.ts`
- [X] T029 [P] Vitest-Unit-Tests + `fast-check` Property-based Tests: gleiche 4 Eigenschaften wie Backend; explizit 29. Feb 2028, DST Spring-Forward 2026-03-29 Europe/Berlin, DST Fall-Back 2026-10-25; Today-gedeckelter Nenner — `frontend/src/lib/streak-calc/index.spec.ts`

**Checkpoint Foundational**: Alle Backend-Tests grün (`dotnet test`); alle Frontend-Tests grün (`npm test`); Migration läuft sauber.

---

## Phase 3: User Story 1 — Sekundengenaue Streak-Anzeige (P1)

**Story Goal**: Nutzer sieht die aktuelle Abstinenzzeit sekundengenau, manipulationssicher und unter allen Bedingungen (Zeitzonenwechsel, Sommerzeit, Offline) korrekt.

**Independent Test**: `GET /habits/{id}/streak` gibt `currentStreakSeconds` + `serverUtcMs` zurück; `StreakCard` zeigt "X Tage Y Std Z Min" live.

### Backend-Endpoint

- [X] T030 [US1] `StreakEndpoints.cs` mit `MapStreakEndpoints(IEndpointRouteBuilder app)` erstellen — `backend/src/Api/Streaks/StreakEndpoints.cs`
- [X] T031 [US1] `GET /habits/{habitId}/streak` implementieren: ruft `StreakService.GetStreakAsync` auf, gibt `StreakDto` mit `currentStreakSeconds` und `serverUtcMs` zurück — `backend/src/Api/Streaks/StreakEndpoints.cs`
- [X] T032 [US1] `GET /time` (public, kein Auth, Rate-Limit 60/min pro IP) implementieren — `backend/src/Api/Streaks/StreakEndpoints.cs`
- [X] T033 [US1] Endpoint-Gruppe in `Program.cs` registrieren — `backend/src/Program.cs`

### Frontend-Integration & UI

- [X] T034 [US1] `streak.api.ts` erstellen: `useStreak(habitId)` Hook — fetcht `/habits/{id}/streak`, speichert monotonen Snapshot via `saveSnapshot` — `frontend/src/services/streak.api.ts`
- [X] T035 [US1] Visibility-Change-Listener implementieren: re-fetcht Streak bei Foreground; **client-seitige Manipulationserkennung**: vergleiche `serverUtcMs` aus Response gegen erwartete Zeit aus Monoton-Snapshot (`lastSnapshot.serverUtcMs + (performance.now() - lastSnapshot.performanceNowMs)`); negative Abweichung > 300.000 ms → Toast "Deine Streak wurde mit dem Server abgeglichen" (kein `serverCorrectionFlag` im DTO erforderlich) — `frontend/src/services/streak.api.ts`
- [X] T036 [US1] `StreakCard.tsx` aktualisieren: `currentStreakSeconds` + `getNowUtcMs()` via `setInterval(1000)`, Anzeige "X Tage Y Std Z Min" — `frontend/src/features/streak/StreakCard.tsx`
- [X] T037 [US1] `StreakCard.spec.tsx` aktualisieren: Counter-Logik testen, Monotonic-Store-Integration mocken — `frontend/src/features/streak/StreakCard.spec.tsx`

**Acceptance Scenarios US1**:
1. Online: `GET /habits/{id}/streak` gibt `currentStreakSeconds` zurück; Anzeige weicht ≤ 1 s von Serverzeit ab
2. Offline: `getNowUtcMs()` verwendet monotones Delta statt `Date.now()`; keine Streak-Unterbrechung bei Timezone-Wechsel
3. Sync nach Offline: client-seitige Erkennung via `serverUtcMs` vs Monoton-Snapshot; negative Abweichung > 5 min → Toast + Annahme des Serverwertes; positive Abweichung → stille Korrektur

> **FR-013 Scope Note**: Retroaktive Startzeit-Änderung (US1-AS4, US3-AS3) ist kein Feature-008-Task — `StreakCalculator` akzeptiert `startedAt` als Parameter und rekalkuliert automatisch korrekt. Die Integration wird von Feature 007 ausgelöst; kein separater Task in diesem Feature erforderlich.

---

## Phase 4: User Story 2 — Abstinenz-Statistik nach Rückfall (P2)

**Story Goal**: Nutzer sieht nach einem Rückfall "X von Y Tagen abstinent" pro Monat/Jahr — Rückfall löscht nicht das gesamte Fortschrittsbild.

**Independent Test**: `GET /habits/{id}/stats/2026/5` gibt korrekte `abstinentDays` und `relevantDays` zurück; `MonthStats`-Komponente rendert "X von Y Tagen abstinent".

### Backend-Endpoints & Relapse-Recording

- [X] T038 [P] [US2] `POST /habits/{habitId}/relapses` implementieren: validiert `occurredAt` ≤ now und ≥ `habit.StartedAt`; validiert `contextNote` ≤ 500 Zeichen; speichert `PreviousStreakSeconds`; verschlüsselt Note via `FieldEncryptor` — `backend/src/Api/Streaks/StreakEndpoints.cs`
- [X] T039 [P] [US2] `GET /habits/{habitId}/stats/{year}/{month}` implementieren: ruft `StatsService.CalculateMonthStats` auf; Nenner Today-gedeckelt für laufende Monate — `backend/src/Api/Streaks/StreakEndpoints.cs`
- [X] T040 [P] [US2] `GET /habits/{habitId}/stats/{year}` implementieren: ruft `StatsService.CalculateYearStats` auf; 12 MonthStats im Response — `backend/src/Api/Streaks/StreakEndpoints.cs`

### Frontend-Integration & UI

- [X] T041 [US2] `useRecordRelapse(habitId)` Mutation-Hook hinzufügen zu `streak.api.ts` — `frontend/src/services/streak.api.ts`
- [X] T042 [US2] `useMonthStats(habitId, year, month)` und `useYearStats(habitId, year)` Hooks hinzufügen zu `streak.api.ts` — `frontend/src/services/streak.api.ts`
- [X] T043 [US2] `MonthStats.tsx` erstellen: rendert "X von Y Tagen abstinent"; zeigt "laufender Monat"-Badge wenn `isCurrentMonth === true` — `frontend/src/features/streak/MonthStats.tsx`
- [X] T044 [US2] Unit-Tests für `MonthStats.tsx`: Today-gedeckelter Nenner, Rückfall-Zähler, laufender-Monat-Badge — `frontend/src/features/streak/MonthStats.spec.tsx`

**Acceptance Scenarios US2**:
1. Monat mit 7 Rückfall-Tagen von 30 → "23 von 30 Tagen abstinent"
2. Abgewöhnung startet am 15. des Monats → Nenner = 16 (nicht 31)
3. Jahresstatistik aggregiert alle 12 Monate korrekt
4. ~~`HabitMode.Reduce`-Abgewöhnung → "X von Y Perioden eingehalten"~~ **⚠️ Post-MVP (FR-012)**: `MonthStatsDto` enthält keine `periodsCompliant`/`totalPeriods`-Felder und `StreakCalculator` hat kein `CalculatePeriodCompliance`. Wird adressiert sobald FR-012 vollständig in die API-Contracts integriert ist (separates Feature oder 008a).

---

## Phase 5: User Story 3 — Kalenderansicht (P3)

**Story Goal**: Nutzer sieht eine Monats- und Jahresansicht mit Farbcodes (abstinent/Rückfall/pausiert/neutral) und kann auf Rückfall-Tage tippen um alle Rückfälle chronologisch zu sehen.

**Independent Test**: `GET /habits/{id}/calendar/2026/5` gibt 31 Tage mit korrekten Status zurück; `CalendarView` rendert Farbcodes; Tap auf Rückfall-Tag zeigt vollständige Liste; `YearHeatmap` zeigt neutrale Kacheln vor Startdatum.

### Backend-Endpoint

- [X] T045 [US3] `GET /habits/{habitId}/calendar/{year}/{month}` implementieren: ruft `CalendarService` auf; entschlüsselt Notizen; alle Rückfälle pro Tag in `relapses[]` chronologisch — `backend/src/Api/Streaks/StreakEndpoints.cs`
- [X] T046 [US3] Validierung: ungültige year/month → 400; Habit nicht gefunden → 404 — `backend/src/Api/Streaks/StreakEndpoints.cs`

### Frontend-Integration & UI

- [X] T047 [US3] `useCalendarMonth(habitId, year, month)` Hook hinzufügen zu `streak.api.ts` — `frontend/src/services/streak.api.ts`
- [X] T048 [US3] `CalendarView.tsx` erstellen: Monatsraster; Zellen farbcodiert nach Status (grün/rot/grau/weiß); zweites WCAG-Merkmal (Muster oder Symbol pro Status) für Farbfehlsichtigkeit; Tap → Sheet mit chronologischer scrollbarer Rückfall-Liste (alle Einträge, kein Verstecken) — `frontend/src/features/streak/CalendarView.tsx`
- [X] T049 [US3] `YearHeatmap.tsx` erstellen: 12-Kachel-Raster; Kacheln vor `startedAt` als neutrale leere Kacheln (kein Farbton, kein Prozentwert, kein Tooltip); Farbintensität = Abstinenzrate — `frontend/src/features/streak/YearHeatmap.tsx`
- [X] T050 [US3] `CalendarView.spec.tsx` erstellen: Unit-Tests für Zell-Farbcodierung; axe-Accessibility-Assertion (0 Verletzungen) — `frontend/src/features/streak/CalendarView.spec.tsx`
- [X] T051 [US3] `YearHeatmap.spec.tsx` erstellen: Unit-Tests für neutrale Kacheln vor Startdatum; Farbintensitäts-Logik — `frontend/src/features/streak/YearHeatmap.spec.tsx`

**Acceptance Scenarios US3**:
1. Abgewöhnung mit gemischtem Verlauf: grüne/rote/graue Zellen korrekt
2. Tap auf Rückfall-Tag am 10.: alle Rückfälle des Tages chronologisch, mit Zeitstempel + Auslöser
3. Nachträgliche Startzeit-Änderung (Feature 007): Kalender zeigt korrekte Einfärbung nach Reload
4. Jahresansicht: Monate vor Startdatum = neutrale leere Kacheln; 12-Monats-Raster vollständig

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Accessibility-Verifikation, Performance-Messung, Integritätssicherung.

- [X] T052 Playwright + `@axe-core/playwright` E2E-Test erstellen: `CalendarView` mit Seeded-Daten laden; `checkA11y()` ausführen; 0 Verletzungen; zweites visuelles Merkmal pro Status prüfen; accessible Labels auf Kalender-Kacheln prüfen — `frontend/tests/streak-a11y.spec.ts`
- [X] T053 [P] P95-Ladezeit für `GET /habits/{id}/calendar/{year}/{month}` unter 50 parallelen Requests messen; Ziel ≤ 300 ms; **Fehlerrate = 0 aus 50 Requests** (Constitution V: ≥ 99 % Reliability). Ergebnis als Kommentar im Load-Test-Script dokumentieren — `backend/tests/` (Load-Test-Script)
- [X] T054 [P] Rate-Limit für `GET /time` verifizieren: > 60 Req/min pro IP → 429 — `backend/tests/Streaks/`
- [X] T055 [P] Integrations-Tests für Rückfall-Endpoint: `occurredAt` in Zukunft → 400; vor `startedAt` → 400; `contextNote` > 500 Zeichen → 400; Verschlüsselung round-trip korrekt — `backend/tests/Streaks/`
- [X] T056 [P] Encryption-Migration-Test: Datenmigration von `context_note` (plaintext) → `context_note_encrypted` (bytea) verlustfrei verifizieren — `backend/tests/Streaks/`

---

## Dependencies (Story Completion Order)

```
Phase 1 (Setup)
  └─→ Phase 2 Foundational (Backend Domain A + Frontend Calc E — parallel)
        ├─→ Phase 2 Backend Schema B → Services C → Endpoints (partial)
        └─→ Phase 2 Frontend Calc E → API Hooks (partial)
              └─→ Phase 3 US1 (Streak-Anzeige)
                    └─→ Phase 4 US2 (Statistik) — parallel mit Phase 3 sobald Endpoints bereit
                          └─→ Phase 5 US3 (Kalender)
                                └─→ Phase 6 Polish
```

## Parallel Execution per Story

| Phase | Parallel möglich |
|---|---|
| Setup T001–T006 | Alle parallel nach T001 |
| Foundational Backend A (T007–T014) | Parallel zu Frontend E (T027–T029) |
| US1 Backend (T030–T033) | Parallel zu US1 Frontend (T034–T037) nach Foundation |
| US2 Endpoints (T038–T040) | Alle untereinander parallel |
| US3 UI (T048–T051) | T048/T049/T050/T051 parallel nach T047 |
| Polish (T052–T056) | T053/T054/T055/T056 parallel nach T052 |

## Implementation Strategy

**MVP-Scope**: User Story 1 (Phase 3) allein liefert bereits einen unabhängig testbaren Wert — sekundengenaue Streak-Anzeige. Phase 2 Foundational ist vollständig geblockt, aber klein.

**Inkrementelle Lieferung**:
1. Phase 1 + 2 Foundation (Backend Domain + Frontend Calc parallel) — Basis
2. Phase 3 US1 — livefähige Streak-Anzeige (MVP)
3. Phase 4 US2 — Statistik nach Rückfall
4. Phase 5 US3 — Kalenderansicht
5. Phase 6 — Accessibility + Performance-Gate vor Release
