<!--
SYNC IMPACT REPORT
==================
Version change: (uninitialized template) → 1.0.0
Bump rationale: Initial ratification of the Quitly project constitution.

Modified principles:
- [PRINCIPLE_1_NAME] → I. Privacy & Data Ownership First
- [PRINCIPLE_2_NAME] → II. Empathy-Driven UX
- [PRINCIPLE_3_NAME] → III. Time-Accuracy is Non-Negotiable
- [PRINCIPLE_4_NAME] → IV. Offline-First
- [PRINCIPLE_5_NAME] → V. Test-First for Streak / XP Logic (NON-NEGOTIABLE)

Added principles (beyond the 5-principle template baseline):
- VI. Accessibility
- VII. Gamification Without Dark Patterns

Added sections:
- Technology Stack & Architectural Guardrails
- Development Workflow & Quality Gates

Removed sections:
- None

Templates requiring updates:
- ✅ .specify/templates/plan-template.md — generic "Constitution Check" gate,
  no principle names hardcoded; no edit required. Plan authors MUST evaluate
  gates against the 7 principles defined here.
- ✅ .specify/templates/spec-template.md — generic; no edit required. Specs
  inheriting from this template MUST surface privacy, accessibility, and
  offline-behavior acceptance criteria when applicable.
- ✅ .specify/templates/tasks-template.md — generic; no edit required. Tasks
  involving streak/XP/achievement logic MUST be preceded by failing unit tests
  (see Principle V).
- ✅ .specify/templates/checklist-template.md — generic; no edit required.
- ✅ .github/prompts/speckit.constitution.prompt.md — generic command file;
  no edit required.

Follow-up TODOs:
- None. All placeholders resolved.
-->

# Quitly Constitution

Quitly is a mobile-first application for tracking abstinence days from any
self-chosen habit, reinforced by humane gamification. This constitution
defines the non-negotiable principles that every feature, code change, and
design decision MUST uphold.

## Core Principles

### I. Privacy & Data Ownership First

Quitly treats user data as a liability, not an asset.

- The guest mode MUST be fully functional for all local-only features
  (tracking, counters, achievements, motivation text). Registration MUST
  remain optional and MUST only be required when the user opts into cloud
  sync or public leaderboards.
- Every user MUST be able to export their full tracking data in a
  machine-readable format (JSON) and permanently delete their account and all
  associated data. These flows implement GDPR Art. 15 (access) and Art. 17
  (erasure) and MUST be reachable in at most three taps from the main
  settings screen.
- No telemetry, crash reporting, or analytics event MAY be transmitted
  without explicit, granular, opt-in consent. The default state is "off".
- Leaderboards MUST be anonymized: no real name, no email, no precise
  geolocation, no device identifier. A user-chosen pseudonym is the only
  identifier permitted on public surfaces.

Rationale: Users in recovery contexts are vulnerable. Trust is the product.
A single privacy incident destroys the product irreversibly.

### II. Empathy-Driven UX

The interface MUST treat relapses as part of recovery, never as failure.

- Loss-framed language is forbidden in user-facing copy. The phrase "streak
  lost" (and translations / equivalents) MUST NOT appear. Replacement framing
  MUST emphasize partial achievement ("29 of 30 days") and prompt
  trigger reflection.
- Destructive actions (delete habit, delete account, reset progress) MUST
  require an explicit confirmation step with a clear consequence description.
  Archiving MUST be the default action wherever a delete affordance is
  offered.
- The user's self-written motivation statement MUST be surfaced contextually
  during high-risk moments (e.g., relapse-reporting flow, late-evening
  reminders) when one has been provided.

Rationale: Shame is a primary relapse trigger. The product's tone is a
clinical feature, not decoration.

### III. Time-Accuracy is Non-Negotiable

Streaks are the product's central promise. Their calculation MUST be
trustworthy under any condition.

- Streak duration MUST be computed to second-level precision against an
  authoritative time source: server time when sync is available, a monotonic
  clock anchored at a verified timestamp when offline. Wall-clock device time
  is untrusted and MUST NOT be the sole input.
- Time zone changes, daylight-saving transitions, and travel across the
  international date line MUST NOT break, shorten, or duplicate a streak.
  The canonical streak value is stored as a UTC instant pair (start,
  last-confirmed) and rendered into the user's current locale only at
  display time.
- Any change to time-handling code MUST be accompanied by property-based
  tests (see Principle V).

Rationale: A wrongly-reset streak is the single fastest path to user churn
and the deepest violation of trust the product can commit.

### IV. Offline-First

Core tracking MUST work without network connectivity.

- The following operations MUST succeed fully offline and reflect in the UI
  immediately: viewing the current counter, starting a new abstinence track,
  reporting a relapse, viewing achievements earned locally, editing the
  motivation text.
- Server synchronization is eventual-consistent. Conflict resolution rules
  MUST be deterministic and documented; ties MUST resolve in favor of the
  outcome more protective of the user's streak.
- Network failures MUST never block a core write. They MUST surface as a
  passive "will sync later" indicator, never as a blocking error dialog.

Rationale: Cravings do not wait for a 4G signal.

### V. Test-First for Streak / XP Logic (NON-NEGOTIABLE)

Any code touching streak calculation, XP accrual, or achievement unlocking
MUST follow strict TDD.

- A failing unit test MUST exist and be reviewed before any production
  implementation is written.
- Property-based tests are mandatory for all time- and date-arithmetic
  functions, covering at minimum: DST transitions, leap years, time-zone
  changes mid-streak, offline-to-online clock reconciliation, and clock skew
  up to ±24 h.
- Pull requests modifying these areas without accompanying new or updated
  tests MUST be rejected at review.

Rationale: These three subsystems are the product's value proposition. Bugs
here are user-visible, emotionally costly, and frequently irreversible (a
lost streak cannot be honestly restored).

### VI. Accessibility

Quitly MUST be usable by everyone, regardless of ability.

- All shipped UI MUST meet WCAG 2.1 Level AA as a minimum. New screens MUST
  pass automated accessibility checks in CI before merge.
- Touch targets MUST be ≥ 44×44 pt; color MUST NOT be the sole carrier of
  information; all interactive elements MUST expose accessible labels.
- Authentication MUST offer Apple Sign-In and Google Sign-In as
  fully-supported alternatives to email/password, both for accessibility and
  to reduce the password-recall burden on users in stressful states.

Rationale: Recovery does not discriminate by ability; neither does the tool.

### VII. Gamification Without Dark Patterns

Gamification serves the user's goal, not engagement metrics.

- No artificial scarcity (limited-time achievements, fake countdowns), no
  pay-to-win mechanics, no premium tiers that gate core tracking or basic
  achievements.
- Push notifications MUST be opt-in, rate-limited to at most one per day by
  default, and MUST NOT be used for re-engagement of inactive users beyond a
  single check-in invitation per week.
- Achievements MUST be tied to genuine progress against the user's stated
  goal. Achievements that reward app-usage time (sessions opened, screens
  viewed) for their own sake are forbidden.

Rationale: The user's success is measured in days outside the app, not
sessions inside it.

## Technology Stack & Architectural Guardrails

These are the default technology choices. Deviations require an entry in the
plan's Complexity Tracking section and review approval.

- **Mobile (iOS + Android)**: React Native via Expo, pinned to the latest
  stable SDK at project start; upgrades follow Expo's stable release cadence.
- **Desktop / Web companion** (when introduced): React on the latest stable
  release; shares a TypeScript domain-logic package with the mobile client.
- **Backend**: .NET (latest LTS or current stable, whichever is newer)
  Minimal API. Persistent data in PostgreSQL. Authentication via JWT issued
  by the Quitly backend; social providers (Apple, Google) federate into the
  same identity.
- **Shared domain logic**: Streak / XP / achievement calculation MUST live
  in a portable, dependency-light module that the mobile, desktop, and
  backend test suites can all execute. Divergent implementations across
  platforms are forbidden.
- **Data minimization**: Backend schemas MUST justify every column that
  references a user. Free-text fields containing user reflections MUST be
  encrypted at rest with a key tied to the user's account.

## Development Workflow & Quality Gates

- **CI/CD merge gate**: No pull request may merge unless the CI pipeline is
  green. The pipeline MUST run, at minimum: unit tests, property-based tests
  for time logic, lint, type checks, and automated accessibility checks on
  any changed UI.
- **Constitution Check**: Every plan generated under `.specify/` MUST
  include an explicit Constitution Check that evaluates each of the seven
  principles above. Violations require a written justification in the plan's
  Complexity Tracking section.
- **Code review**: At least one reviewer other than the author. Reviews MUST
  verify constitution compliance, not only code correctness.
- **Release versioning**: The application uses semantic versioning. Any
  release that changes streak, XP, or achievement semantics in a
  user-observable way is a MINOR bump at minimum; any release that can
  invalidate previously stored streak data is a MAJOR bump and requires a
  documented data-migration plan.
- **Incident handling**: Any production bug that can falsely break a user's
  streak is a Severity-1 incident, takes precedence over feature work, and
  requires a post-mortem published to the repository.

## Governance

This constitution supersedes all other engineering practices and stylistic
preferences. In any conflict between this document and other guidance, this
document wins.

- **Amendment procedure**: Amendments are proposed via pull request
  modifying this file, accompanied by an updated Sync Impact Report comment
  at the top. Approval requires sign-off from the project maintainer and one
  additional contributor.
- **Versioning policy**: This constitution itself follows semantic
  versioning.
  - **MAJOR**: A principle is removed, redefined in a
    backward-incompatible way, or governance rules change in a way that
    invalidates prior decisions.
  - **MINOR**: A new principle or section is added, or an existing principle
    gains materially new normative content.
  - **PATCH**: Clarifications, wording fixes, typo corrections, or
    non-semantic refinements.
- **Compliance review**: Every pull request description MUST state whether
  the change touches any constitutional principle and, if so, which one.
  Reviewers MUST verify the claim. Quarterly, the maintainer reviews merged
  changes against this constitution and records findings in
  `.specify/memory/`.
- **Runtime guidance**: Day-to-day development guidance and conventions live
  in repository-local documentation (e.g., `README.md`, `AGENTS.md`,
  `.github/copilot-instructions.md`). Those documents MUST defer to this
  constitution on any conflict.

**Version**: 1.0.0 | **Ratified**: 2026-05-30 | **Last Amended**: 2026-05-30
