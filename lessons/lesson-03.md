# Lesson 03 — Structured Output

Branch: `lesson-03`

## What's introduced

`LlmClient` gains the ability to ask the LLM for a response that conforms to a specific C# type. A JSON schema is derived from the type and injected into the prompt; the raw text response is then extracted and deserialized. Up to three retries are attempted on parse failure.

## Key concepts

- **`GenerateStructuredAsync<T>`** — sends the JSON schema for `T` as part of the prompt, then deserializes the response into `T`
- **`JsonHelper.GetSchema<T>()`** — derives a JSON schema string from a C# type using `System.Text.Json`
- **`JsonHelper.ExtractJson(text)`** — strips markdown fences and extracts the first JSON object or array from freeform LLM output
- **`JsonHelper.TryDeserialize<T>(text)`** — safe deserialize returning `null` on failure
- **Retry loop** — three attempts before giving up, so minor formatting errors are recovered automatically

## New files

| File | Purpose |
|---|---|
| `Shared/JsonHelper.cs` | Schema derivation, JSON extraction, safe deserialization |

## Changed files

| File | Change |
|---|---|
| `Shared/LlmClient.cs` | Adds `GenerateStructuredAsync<T>` with schema injection and retry |
