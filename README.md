# ccode

A coding agent built from scratch in C# / .NET, following the lesson structure from [agents-from-scratch](https://github.com/pguso/agents-from-scratch) by [@pguso](https://github.com/pguso).

The original repository teaches how AI agents work by building one progressively across lessons using Python and a local LLM. This is a C# port that follows the same progression, with some additions and adaptations for .NET and a coding agent use case.

## Differences from the original

| Original | This repo |
|---|---|
| Python | C# / .NET 9 |
| Local LLM via llama-cpp-python | Remote or local LLM via [Microsoft.Extensions.AI](https://github.com/dotnet/extensions) |
| llama.cpp / GGUF models | [OllamaSharp](https://github.com/awaescher/OllamaSharp) (local) or OpenAI (remote) |
| Plain terminal output | [Spectre.Console](https://spectreconsole.net/) TUI with markdown rendering |
| Flat planning | AoT dependency graph with parallel execution |
| Logging added late | Structured logging added early (lesson 08) as its own lesson |

## Stack

- **LLM abstraction** — `Microsoft.Extensions.AI` (`IChatClient`)
- **Local LLM** — OllamaSharp
- **TUI** — Spectre.Console + Markdig
- **Serialization** — System.Text.Json

## Lessons

Each lesson is a branch that builds on the previous one.

| Branch | Lesson |
|---|---|
| `lesson-01` | Basic LLM chat — `LlmClient` wrapping `IChatClient`, Spectre.Console loop |
| `lesson-02` | System prompt — loaded from `system_prompt.txt`, no recompile needed |
| `lesson-03` | Structured output — `GenerateStructuredAsync<T>` with JSON schema + retry |
| `lesson-04` | Decision making — `DecideAsync<TEnum>` constrained to any enum type |
| `lesson-05` | Tools — `ITool` interface + ReadFile, WriteFile, ListFiles, RunCommand, SearchFiles, SearchInFiles, MoveFile, CreateDirectory, GetWorkingDirectory |
| `lesson-06` | Agent loop — observe → decide → act with event callbacks |
| `lesson-07` | Memory — persistent facts across steps and sessions, cache-friendly prompt ordering |
| `lesson-08` | Logging — structured JSONL file sink + TUI sink, configurable via `logger.json` |
| `lesson-09` | Planning — AoT graph generation with structural + LLM validation, loop detection |
| `lesson-10` | Atomic actions — each plan step resolved to a single validated tool call |
| `lesson-11` | Atom of Thought — dependency graph with parallel execution and cycle detection |

## Running

Requires [Ollama](https://ollama.com) running locally with a model pulled, or an OpenAI API key.

```bash
# Clone and build
git clone https://github.com/your-username/ccode
cd ccode/src/ccode
dotnet run
```

Edit `system_prompt.txt` to change the agent's persona.
Edit `logger.json` to toggle file and TUI logging sinks.

## Credits

Based on [agents-from-scratch](https://github.com/pguso/agents-from-scratch) by [@pguso](https://github.com/pguso), which teaches agent fundamentals from first principles without frameworks. Highly recommended if you want to understand how agents actually work.
