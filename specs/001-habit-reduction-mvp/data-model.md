# Data Model: Quitly Habit Reduction MVP

## Scope Notes
- MVP-active entities: User, Habit, CheckIn, Streak, Trigger, Reminder, WeeklyInsight, Relapse, RecoveryPlanStep.
- Post-MVP entities (defined now for extensibility): Intervention, Achievement.

## Entity: User
- Purpose: Identity and ownership root.
- Fields:
  - id (UUID, PK)
  - email (string, unique)
  - password_hash (string)
  - timezone (string)
  - locale (string)
  - created_at (timestamp)
  - updated_at (timestamp)
- Relationships:
  - 1..1 active Habit
  - 1..N CheckIn
  - 1..1 Reminder preference
  - 1..N Achievement (Post-MVP)

## Entity: Habit
- Purpose: Single active goal for MVP.
- Fields:
  - id (UUID, PK)
  - user_id (UUID, FK -> User)
  - category (enum: smoking, social_media, sugar, impulse_buying, custom)
  - mode (enum: reduce, quit)
  - title (string)
  - active (bool)
  - started_on (date)
  - created_at (timestamp)
  - updated_at (timestamp)
- Validation:
  - One active habit per user (unique partial index on user_id where active=true).

## Entity: CheckIn
- Purpose: Daily reflection record.
- Fields:
  - id (UUID, PK)
  - user_id (UUID, FK -> User)
  - habit_id (UUID, FK -> Habit)
  - day (date)
  - status (enum: abstinent, non_abstinent, unsure)
  - mood (enum: very_low, low, neutral, good, very_good)
  - note (string, nullable)
  - source (enum: manual, correction)
  - created_at (timestamp)
  - updated_at (timestamp)
- Relationships:
  - N..M Trigger via CheckInTrigger join table.
- Validation:
  - Max one latest effective check-in per user/day (upsert semantics).

## Entity: Trigger
- Purpose: Structured trigger taxonomy for patterns.
- Fields:
  - id (UUID, PK)
  - code (string, unique)
  - label (string)
  - active (bool)

## Join Entity: CheckInTrigger
- Fields:
  - check_in_id (UUID, FK -> CheckIn)
  - trigger_id (UUID, FK -> Trigger)
- Constraints:
  - Composite PK (check_in_id, trigger_id)

## Entity: Streak
- Purpose: Materialized streak snapshot for fast reads.
- Fields:
  - user_id (UUID, PK, FK -> User)
  - current_streak_days (int)
  - last_abstinent_day (date, nullable)
  - last_non_abstinent_day (date, nullable)
  - updated_at (timestamp)
- Rule:
  - Resets to 0 on non-abstinent day.

## Entity: Reminder
- Purpose: Passive in-app prompt preferences.
- Fields:
  - user_id (UUID, PK, FK -> User)
  - passive_prompt_enabled (bool)
  - prompt_tone (enum: gentle, neutral)
  - updated_at (timestamp)

## Entity: WeeklyInsight
- Purpose: Weekly aggregate summary and guidance.
- Fields:
  - id (UUID, PK)
  - user_id (UUID, FK -> User)
  - week_start (date)
  - check_in_count (int)
  - abstinent_days (int)
  - top_triggers (jsonb)
  - mood_trend (jsonb)
  - summary_text (string)
  - confidence (enum: low, medium, high)
  - created_at (timestamp)
- Constraint:
  - Unique (user_id, week_start)

## Entity: Relapse
- Purpose: Explicitly user-marked relapse events.
- Fields:
  - id (UUID, PK)
  - user_id (UUID, FK -> User)
  - habit_id (UUID, FK -> Habit)
  - occurred_at (timestamp)
  - context_note (string, nullable)
  - created_at (timestamp)

## Entity: RecoveryPlanStep
- Purpose: Minimal recovery continuation step after a relapse.
- Fields:
  - id (UUID, PK)
  - relapse_id (UUID, FK -> Relapse)
  - user_id (UUID, FK -> User)
  - step_text (string)
  - due_within_hours (int, default 24)
  - completed_at (timestamp, nullable)
  - created_at (timestamp)

## Post-MVP Entity: Intervention
- Purpose: Craving support actions and outcomes.
- Fields:
  - id (UUID, PK)
  - user_id (UUID, FK -> User)
  - type (enum: breathing, delay, walk, contact_support, custom)
  - outcome (enum: helped, partially_helped, not_helped)
  - created_at (timestamp)

## Post-MVP Entity: Achievement
- Purpose: Optional reinforcement badges.
- Decision status: Post-MVP only; product decision required before implementation.
- Fields:
  - id (UUID, PK)
  - user_id (UUID, FK -> User)
  - code (string)
  - unlocked_at (timestamp)

## State Transitions
- Habit.active: true -> false when replaced by new active habit.
- CheckIn.status:
  - unsure -> abstinent/non_abstinent via correction
  - abstinent -> non_abstinent correction allowed same day
- Streak.current_streak_days:
  - increments on consecutive abstinent days
  - resets to 0 on non_abstinent or missing abstinent day
