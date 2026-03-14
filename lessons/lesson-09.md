# Lesson 09 — Planning

Branch: `lesson-09`

## What's introduced

Before acting, the agent generates a multi-step plan and validates it — first structurally (no duplicates, no empty steps) then with a second LLM call that checks whether the plan actually makes sense for the goal.

## Key concepts

- **`Planner.CreatePlanAsync`** — asks the LLM to break the goal into an ordered list of steps
- **`Planner.ValidatePlanAsync`** — two-pass validation:
  1. Structural: checks for empty steps, duplicate entries, minimum/maximum step count
  2. LLM sanity check: a second prompt asks whether the plan is coherent and complete
- **Plan event** — the validated plan is emitted as an `AgentEventType.Plan` event so the UI can display it before execution begins
- **Graceful fallback** — if planning fails, the agent falls back to the direct decide loop from lesson 06

## New files

| File | Purpose |
|---|---|
| `Agent/Planner.cs` | Plan generation and two-pass validation |

## Changed files

| File | Change |
|---|---|
| `Agent/Agent.cs` | Calls `Planner` before the loop; emits `Plan` event; falls back if planning fails |
