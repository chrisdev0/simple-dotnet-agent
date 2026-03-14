# Lesson 01 — Basic LLM Chat

Branch: `lesson-01`

## What's introduced

A minimal chat loop that sends user input to an LLM and renders the response in the terminal. The agent has no memory, no tools, and no planning — just raw generation.

Provider selection is environment-driven: if `OPENAI_API_KEY` is set, `gpt-4o-mini` is used via OpenAI; otherwise the app falls back to a local Ollama model.

## Key concepts

- `IChatClient` — the `Microsoft.Extensions.AI` abstraction over any LLM provider
- `OllamaApiClient` — `IChatClient` implementation for local Ollama models
- `OpenAI.Chat.ChatClient.AsIChatClient()` — wraps the OpenAI SDK as an `IChatClient`
- `LlmClient` — thin wrapper holding the `IChatClient` and exposing `GenerateAsync`
- `MarkdownRenderer` — renders LLM markdown responses via Markdig to Spectre.Console

## New files

| File | Purpose |
|---|---|
| `Program.cs` | Entry point: provider selection, chat loop, response rendering |
| `Shared/LlmClient.cs` | Wraps `IChatClient`, manages chat history, exposes `GenerateAsync` |
| `Shared/MarkdownRenderer.cs` | Converts markdown text to styled Spectre.Console output |

## Running

```bash
# Local (requires Ollama running with a model pulled)
dotnet run

# Remote
OPENAI_API_KEY=sk-... dotnet run
```
