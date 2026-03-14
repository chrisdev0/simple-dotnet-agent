# Lesson 08 — Structured Logging

Branch: `lesson-08`

## What's introduced

Every LLM call, tool call, decision, and memory write is recorded as a structured log entry. Sinks are pluggable and configured at runtime via `logger.json` — no recompile needed to toggle file or TUI output.

## Key concepts

- **`ILogSink`** — interface with a single `Write(LogEntry)` method; any number of sinks can be active simultaneously
- **`FileLogSink`** — appends JSONL entries to `agent.log.jsonl`; each line is a self-contained JSON object suitable for `jq` or log aggregators
- **`TuiLogSink`** — prints dimmed debug lines inline in the Spectre.Console output during a session
- **`AgentLogger`** — aggregates sinks; typed methods (`LogLlmCall`, `LogToolCall`, `LogDecision`, `LogMemory`) keep call sites readable
- **`logger.json`** — `{ "fileSink": true, "tuiSink": true }` — toggle sinks without touching code

## New files

| File | Purpose |
|---|---|
| `Shared/Logging/ILogSink.cs` | Sink interface + `LogEntry` record |
| `Shared/Logging/AgentLogger.cs` | Aggregator with typed log methods |
| `Shared/Logging/FileLogSink.cs` | JSONL file sink |
| `Shared/Logging/TuiLogSink.cs` | Inline TUI sink |
| `logger.json` | Sink configuration |

## Changed files

| File | Change |
|---|---|
| `Program.cs` | Reads `logger.json`, constructs sinks, passes `AgentLogger` to `LlmClient` and `Agent` |
| `Shared/LlmClient.cs` | Logs every LLM call with prompt, response, and duration |
| `Agent/Agent.cs` | Logs tool calls and memory writes |
