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

## Learning path

Each lesson lives on its own branch and builds directly on the previous one. You can follow the lessons as a step-by-step guide for building an agent from scratch, or jump to any branch to see the full working code at that stage.

### How to follow along

```bash
# Start at the beginning
git checkout lesson-01
dotnet run --project src/ccode

# Move to the next lesson when you're ready
git checkout lesson-02
```

Each lesson has a dedicated README in the `lessons/` directory explaining what was introduced, the key concepts, and which files changed. Read it before or after you look at the diff:

```bash
# See what changed between lessons
git diff lesson-01..lesson-02

# Read the lesson notes
cat lessons/lesson-02.md
```

To implement a lesson yourself instead of reading the solution, branch off the previous lesson and compare your result to the lesson branch when you're done:

```bash
git checkout lesson-03
git checkout -b lesson-04-attempt
# implement lesson 04 yourself...
git diff lesson-04  # compare with the reference solution
```

### Lessons

| Branch | What's built | `lessons/` notes |
|---|---|---|
| `lesson-01` | Basic LLM chat — `LlmClient`, Spectre.Console loop, OpenAI/Ollama provider selection | [lesson-01.md](lessons/lesson-01.md) |
| `lesson-02` | System prompt loaded from file | [lesson-02.md](lessons/lesson-02.md) |
| `lesson-03` | Structured output — `GenerateStructuredAsync<T>` with JSON schema + retry | [lesson-03.md](lessons/lesson-03.md) |
| `lesson-04` | Decision making — `DecideAsync<TEnum>` constrained to any enum type | [lesson-04.md](lessons/lesson-04.md) |
| `lesson-05` | Tools — `ITool` interface + 9 built-in tools | [lesson-05.md](lessons/lesson-05.md) |
| `lesson-06` | Agent loop — observe → decide → act with event callbacks | [lesson-06.md](lessons/lesson-06.md) |
| `lesson-07` | Memory — persistent facts, cache-friendly prompt ordering | [lesson-07.md](lessons/lesson-07.md) |
| `lesson-08` | Structured logging — JSONL file sink + TUI sink | [lesson-08.md](lessons/lesson-08.md) |
| `lesson-09` | Planning — multi-step plan with structural + LLM validation | [lesson-09.md](lessons/lesson-09.md) |
| `lesson-10` | Atomic actions — each plan step resolved to a single tool call | [lesson-10.md](lessons/lesson-10.md) |
| `lesson-11` | Atom of Thought — dependency graph with parallel execution | [lesson-11.md](lessons/lesson-11.md) |

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
