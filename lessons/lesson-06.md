# Lesson 06 — Agent Loop

Branch: `lesson-06`

## What's introduced

A full observe → decide → act loop replaces the simple one-shot chat. The agent maintains state across steps, uses tools when needed, and emits typed events so the caller can render progress without coupling the core logic to any specific UI.

## Key concepts

- **`Agent.RunAsync(goal, onEvent)`** — drives the loop; calls back via `Action<AgentEvent>` rather than returning an `IAsyncEnumerable`, keeping the API simple
- **`AgentEvent` / `AgentEventType`** — typed events: `Plan`, `ToolCall`, `ToolResult`, `Response`, `Error`
- **`AgentState`** — tracks current step, goal, last tool result, and done flag; exposes `GetContext()` for prompt injection
- **`AgentPrompts`** — static class that builds prompt strings in cache-friendly order: Memory → State → Goal
- **Repeated-result detection** — if the same tool result appears twice in a row the loop aborts to prevent infinite retries

## New files

| File | Purpose |
|---|---|
| `Agent/Agent.cs` | Main agent loop |
| `Agent/AgentState.cs` | Mutable loop state |
| `Agent/AgentEvent.cs` | Event record + static factories |
| `Agent/AgentPrompts.cs` | Prompt builders |

## Changed files

| File | Change |
|---|---|
| `Program.cs` | Instantiates `Agent`, calls `RunAsync`, renders events via `RenderEvent` |
