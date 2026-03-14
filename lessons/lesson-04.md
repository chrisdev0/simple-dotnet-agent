# Lesson 04 — Decision Making

Branch: `lesson-04`

## What's introduced

`LlmClient` gains a `DecideAsync<TAction>` method that constrains the LLM to pick exactly one value from a C# enum. The valid choices are embedded in the prompt and the response is parsed as an enum member.

## Key concepts

- **`DecideAsync<TAction> where TAction : struct, Enum`** — the generic constraint ensures only enum types can be used;
- **Enum-as-decision-space** — defining a `NextAction` enum gives you a compile-time-checked vocabulary for the LLM's possible responses

## Changed files

| File | Change |
|---|---|
| `Shared/LlmClient.cs` | Adds `DecideAsync<TAction>` with enum names injected into the prompt |
