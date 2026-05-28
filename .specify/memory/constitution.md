<!--
Sync Impact Report
Version change: 0.0.0 -> 1.0.0
Modified principles:
- None (initial adoption)
Added sections:
- Core Principles
- Scope and Delivery Boundaries
- Conflict Resolution and Decision Rules
- Governance
Removed sections:
- None
Templates requiring updates:
- .specify/templates/plan-template.md: ✅ updated
- .specify/templates/spec-template.md: ✅ updated
- .specify/templates/tasks-template.md: ✅ updated
- .specify/templates/commands/*.md: ⚠ pending (directory not present)
Follow-up TODOs:
- None
-->

# Quitly Constitution

## Core Principles

### I. User Welfare Over Engagement
Product and delivery decisions MUST prioritize measurable user wellbeing outcomes over
engagement metrics. Features that increase session frequency, streak pressure, or
notification volume are forbidden if they are likely to reduce autonomy, increase stress,
or undermine sustainable habit change.
Rationale: Quitly exists to help people recover agency, not to maximize app dependency.

### II. Relapse Without Shame
Relapse handling MUST be non-punitive and dignity-preserving. The product MUST avoid
language, visuals, scoring, or gamification patterns that imply failure identity.
Recovery flows MUST normalize setbacks, offer actionable next steps, and preserve
continuity of progress context.
Rationale: Shame increases abandonment risk and undermines long-term behavior change.

### III. Privacy by Default and Data Minimization
Quitly MUST collect the minimum data required for core functionality. Sensitive data
collection is opt-in by default, purpose-limited, and documented. Features MUST function
in a meaningful baseline mode without analytics tracking. Data retention MUST have clear
limits and deletion pathways for users.
Rationale: Trust and safety are prerequisites for honest self-reflection and sustained use.

### IV. Clear, Everyday UX
User flows MUST be clear, low-cognitive-load, and usable in everyday contexts with
limited time and attention. Each screen MUST have one primary action, plain language,
and concise feedback. Avoid feature complexity that requires training or expert mental
models.
Rationale: The product serves people in vulnerable moments; clarity is a safety property.

### V. Testable Quality Standards
Every releasable feature MUST meet explicit, testable quality gates for reliability,
accessibility, and performance:
- Reliability: critical user journeys succeed at >= 99% in automated and monitored runs.
- Accessibility: conformance target is WCAG 2.2 AA for affected surfaces.
- Performance: P95 interactive response <= 300 ms for core UI actions on target devices.
If a gate is not met, the feature MUST not be marked done.
Rationale: Quality failures directly harm trust and behavior-change continuity.

## Scope and Delivery Boundaries

MVP scope MUST include only capabilities necessary to deliver independent value for
habit interruption, relapse-safe recovery, and user-controlled progress visibility.
Post-MVP scope MAY include advanced personalization, broader integrations, and
non-essential engagement tooling only after MVP quality and privacy goals are stable.

All features MUST be labeled as MVP or Post-MVP in spec artifacts. If uncertain,
default to Post-MVP until user-welfare impact and operational readiness are proven.

Definition of Done for every feature:
- Constitutional alignment documented for all five principles.
- Automated tests cover primary, failure, and relapse-related paths.
- Accessibility checks (automated and manual spot-check) are passed.
- Performance and reliability evidence is attached to the feature.
- Privacy impact and data-minimization review is completed.
- Rollout and rollback notes are recorded.

## Conflict Resolution and Decision Rules

When goals conflict, teams MUST apply this order of precedence:
1. User welfare and non-shaming outcomes
2. Privacy and data minimization
3. Reliability, accessibility, and performance quality gates
4. UX clarity and simplicity
5. Delivery speed, growth, and internal convenience

Trade-offs MUST be documented in the relevant plan or PR with the rejected alternatives.
If a decision would violate a higher-priority rule, the change MUST be redesigned,
de-scoped, or escalated as a constitutional amendment proposal.

## Governance

This Constitution is the highest governance artifact for product and engineering decisions.
All specs, plans, tasks, and PR reviews MUST include a compliance check against this
document.

Amendment procedure:
1. Create a documented proposal with rationale, impact, and migration steps.
2. Obtain approval from product and engineering maintainers.
3. Update dependent templates and guidance files in the same change.

Versioning policy:
- MAJOR: backward-incompatible governance changes or principle removals/redefinitions.
- MINOR: new principle or materially expanded mandatory guidance.
- PATCH: clarifications or editorial updates without normative change.

Compliance review expectations:
- Every implementation plan MUST pass a Constitution Check before design and again
	before execution.
- Every feature task set MUST include objective evidence for Definition of Done criteria.

**Version**: 1.0.0 | **Ratified**: 2026-05-28 | **Last Amended**: 2026-05-28
