# Lesson 10 — Atomic Actions

Branch: `lesson-10`

## What's introduced

Each plan step is resolved into a single, validated tool call before execution. This prevents the agent from conflating multiple operations in one step, makes failures easier to diagnose, and ensures every step has a concrete, observable effect.

## Key concepts

- **`AtomicAction`** — record containing a tool name and its fully-resolved argument dictionary; unlike `ToolCall` it is derived from a plan step rather than selected ad-hoc
- **`Planner.CreateAtomicActionAsync(step, originalGoal, tools)`** — maps a natural-language plan step to a specific tool + arguments; the original goal is threaded through so the LLM has full context when resolving ambiguous steps
- **`Agent.ExecutePlanStepAsync`** — resolves the step to an `AtomicAction`, validates all required arguments are present, then delegates to `ExecuteToolAsync`
- **`AgentState.Goal`** — stored so every prompt during plan execution has access to the original intent

## Changed files

| File | Change |
|---|---|
| `Agent/Planner.cs` | Adds `CreateAtomicActionAsync` |
| `Agent/Agent.cs` | `ExecutePlanStepAsync` uses atomic resolution; original goal stored in state |
| `Agent/ITool.cs` | Adds `AtomicAction` record |
