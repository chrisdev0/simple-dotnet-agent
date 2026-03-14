# Lesson 11 — Atom of Thought (AoT) Dependency Graph

Branch: `lesson-11`

## What's introduced

Instead of a flat ordered list, the agent generates a dependency graph where each step declares which other steps it depends on. Steps with no unmet dependencies are executed in parallel. Cycles are detected before execution begins.

## Key concepts

- **`AotStep`** — a plan step with an `Id` and a list of `DependsOn` step IDs
- **`Planner.CreateAotGraphAsync`** — asks the LLM to produce a dependency graph rather than a flat list
- **`Planner.ValidateAotGraph`** — structural validation: duplicate IDs, references to nonexistent IDs, and cycle detection via Kahn's algorithm
- **Kahn's algorithm** — topological sort by repeatedly removing nodes with no incoming edges; any node left at the end is part of a cycle
- **Parallel wave execution** — steps are grouped into waves (all steps whose dependencies are already done); each wave runs via `Task.WhenAll`
- **Fallback** — if graph generation or validation fails, the agent falls back to the flat sequential plan from lesson 09

## New files

| File | Purpose |
|---|---|
| `Agent/AotStep.cs` | Step record with `Id` and `DependsOn` |

## Changed files

| File | Change |
|---|---|
| `Agent/Planner.cs` | Adds `CreateAotGraphAsync` and `ValidateAotGraph` |
| `Agent/Agent.cs` | `ExecuteAotGraphAsync` drives parallel wave execution; falls back to sequential on failure |
