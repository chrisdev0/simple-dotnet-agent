# Lesson 05 — Tools

Branch: `lesson-05`

## What's introduced

A tool abstraction lets the agent interact with the outside world. Each tool declares its name, description, and parameters; the agent passes these to the LLM as context and the LLM selects which tool to call with which arguments.

## Key concepts

- **`ITool`** — interface with `Name`, `Description`, `Parameters`, and `ExecuteAsync(Dictionary<string,string>)`
- **`ToolParameter`** — record describing a single parameter: name, description, required flag
- **`ToolCall`** — record capturing a tool name and its resolved argument dictionary
- **`LlmClient.RequestToolAsync`** — presents the tool list to the LLM and deserializes its selection as a `ToolCall`

## Tools added

| Tool | What it does |
|---|---|
| `ReadFileTool` | Read a file's contents |
| `WriteFileTool` | Write text to a file |
| `ListFilesTool` | List files in a directory |
| `RunCommandTool` | Run a shell command and return stdout/stderr |
| `MoveFileTool` | Move or rename a file |
| `CreateDirectoryTool` | Create a directory |
| `GetWorkingDirectoryTool` | Return the current working directory |
| `SearchFilesTool` | Find files matching a glob pattern |
| `SearchInFilesTool` | Search file contents with a regex |

## New files

| File | Purpose |
|---|---|
| `Agent/ITool.cs` | `ITool`, `ToolParameter`, `ToolCall` definitions |
| `Agent/Tools/` | One file per tool implementation |
