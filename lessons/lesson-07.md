# Lesson 07 — Memory

Branch: `lesson-07`

## What's introduced

The agent can store and recall facts across both steps within a session and between sessions. After every tool call the result is stored automatically; conversational responses go through a structured extraction that returns both a reply and any facts worth saving. Memory is persisted to `memory.json` on disk.

## Key concepts

- **`Memory`** — stores a flat list of string facts; `GetContext()` returns the most recent 10 formatted for prompt injection
- **`Memory.Load()` / `Memory.Save()`** — read from / write to `memory.json` so facts survive restarts
- **`AgentResponse { Reply, SaveToMemory }`** — `LlmClient.RespondAsync` returns a structured object so the agent can save facts from plain conversation, not only from tool results
- **Cache-friendly prompt ordering** — prompts are ordered Memory (stable, cacheable) → State → Goal (volatile) to maximise KV-cache reuse on providers that support it

## New files

| File | Purpose |
|---|---|
| `Agent/Memory.cs` | Fact storage, search, persistence |

## Changed files

| File | Change |
|---|---|
| `Agent/Agent.cs` | Stores tool results in memory; uses `RespondAsync` to save conversational facts |
| `Shared/LlmClient.cs` | Adds `RespondAsync` returning `AgentResponse` |
| `Agent/AgentPrompts.cs` | Memory context added to all prompts; cache-friendly ordering |
| `Program.cs` | Instantiates `Memory`, calls `Load()` at startup |
