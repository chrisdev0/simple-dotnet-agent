# Lesson 02 — System Prompt

Branch: `lesson-02`

## What's introduced

The agent's persona and instructions are moved out of code into `system_prompt.txt`. The file is copied to the output directory on build so it can be edited without recompiling.

## Key concepts

- **Externalised system prompt** — `File.ReadAllText("system_prompt.txt")` reads the prompt at startup; changing the agent's behaviour is a text edit, not a code change
- **`CopyToOutputDirectory`** — the `.csproj` `<Content>` item ensures the file is always next to the binary

## Changed files

| File | Change |
|---|---|
| `Program.cs` | Loads `system_prompt.txt` and passes it to `LlmClient` |
| `Shared/LlmClient.cs` | Accepts a `systemPrompt` string and prepends it as the first `ChatMessage` |
| `system_prompt.txt` | Agent persona — edit freely without recompiling |
| `ccode.csproj` | `<Content>` item for `system_prompt.txt` |
