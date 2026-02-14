# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run Commands

```bash
dotnet build                                              # Build the solution
dotnet run --project src/Janet                             # Run with defaults (llama3.1 @ localhost:8080)
dotnet run --project src/Janet -- --model=<m> --url=<url>  # Run with custom model/endpoint
dotnet test                                                # Run all tests
dotnet test --filter "FullyQualifiedName~FileToolsTests"   # Run a single test class
```

Target framework is .NET 10.0. Tests use xUnit with Moq.

## Architecture

Janet is a CLI coding assistant that connects to any OpenAI-compatible LLM endpoint (Ollama, LM Studio, etc.) and gives it file system and shell tools.

**Core flow:** `Program.cs` (REPL + DI setup) → `Agent.RunAsync()` (conversation loop with tool dispatch) → Tool classes (static methods exposed to the LLM via `AIFunctionFactory.Create()`).

### Key Components

- **`Program.cs`** — Top-level entry point. Configures DI (IChatClient, Config, Agent), registers tools, runs the interactive REPL. CLI args: `--model=`, `--url=`.
- **`Agent/Agent.cs`** — Stateful conversation manager. Holds `List<ChatMessage>` history, sends to LLM with tools, extracts text from response. Uses `Microsoft.Extensions.AI` abstractions with `UseFunctionInvocation()` for automatic tool calling.
- **`Configuration/Config.cs`** — Settings POCO with defaults: model=`llama3.1`, baseUrl=`http://localhost:8080`, temperature=`0.7`, maxTokens=`4096`.
- **`Tools/`** — Three static classes whose methods are exposed as LLM-callable tools:
  - `FileTools` — ReadFile, WriteFile, FileExists, Glob
  - `ProcessTools` — RunCommand (cross-platform cmd.exe/bash, 30s timeout), CommandExists
  - `SearchTools` — Grep (regex, skips .git/node_modules/bin/obj), Tree (directory listing with depth limit)

### Tool Registration Pattern

Tools are static methods decorated with `[Description]`. They are wrapped with `AIFunctionFactory.Create()` and passed to `Agent.RunAsync()` as `IList<AITool>`. The `IChatClient` pipeline's `UseFunctionInvocation()` middleware handles tool call/response cycling automatically.

### Console Output

Uses `Spectre.Console` for markup rendering. All user-facing output goes through `AnsiConsole.MarkupLine()` — text displayed to the user must be escaped with `Markup.Escape()`.
