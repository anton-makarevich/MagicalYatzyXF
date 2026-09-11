# AGENTS.md

Guidance for AI coding agents working in this repository.

## Overview

MagicalYatzy is a cross-platform Yatzy dice game: .NET 9, AvaloniaUI (Desktop, Android, iOS, Browser/WASM), MVVM via the Sanet.MVVM NuGet framework. It is a remake of an older WP7/UWP app. Default branch: `main`.

## Build & Test Commands

- .NET SDK is pinned in `global.json` (9.0.x); solutions use the `.slnx` format (recent SDK/IDE required).
- Build the app solution: `dotnet build src/Avalonia/MagicalYatzy.Avalonia.slnx`
- Mobile/WASM heads require workloads (once per machine), e.g.: `dotnet workload restore src/Avalonia/MagicalYatzy.Avalonia.Android/MagicalYatzy.Avalonia.Android.csproj`
- Run all tests: `dotnet test tests/MagicalYatzy.Core.Tests/MagicalYatzy.Core.Tests.csproj`
- Run a subset: append `--filter "FullyQualifiedName~DiceTests"`
- Coverage (mirrors CI): `dotnet test tests/MagicalYatzy.Core.Tests/MagicalYatzy.Core.Tests.csproj /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:ExcludeByAttribute=GeneratedCodeAttribute /p:Include=[MagicalYatzy]*`
- Run the desktop app: `dotnet run --project src/Avalonia/MagicalYatzy.Avalonia.Desktop`

## Testing Conventions

- xUnit + Shouldly + NSubstitute. Always use Shouldly assertions (`result.ShouldBe(expected)`); FluentAssertions is not used in this repo.
- Tests live in `tests/MagicalYatzy.Core.Tests`, mirroring the `src/MagicalYatzy.Core` structure.
- UI projects have no unit tests by design — keep logic in Core ViewModels/Services.
- Use the `generate-unit-tests` skill when writing tests.

## Architecture

Dependency flow: `Avalonia heads → MagicalYatzy.Avalonia (views) → MagicalYatzy.Core → MagicalYatzy.Dto`.

- `src/MagicalYatzy.Core` — game logic (`Models/Game`), ViewModels, Services. Root namespace is `Sanet.MagicalYatzy`.
- `src/MagicalYatzy.Dto` — API DTOs, `IWebService`, legacy WCF client.
- `src/Avalonia/MagicalYatzy.Avalonia` — shared UI + composition root (`App.axaml.cs`, `DependencyInjection/CoreServices.cs`).
- `src/Avalonia/MagicalYatzy.Avalonia.{Desktop,Android,iOS,Browser}` — thin platform heads.

Key conventions:

- Every ViewModel inherits `DicePanelViewModel`; every view inherits `DicePanelView<TViewModel>` (animated dice background on all screens).
- ViewModels never reference views. Navigation goes through the inherited `NavigationService` (`NavigateToViewModelAsync<T>()`, `NavigateToRootAsync()`).
- View↔ViewModel mappings are registered in `App.RegisterViews`; `Lobby` and `Game` screens have **Narrow (mobile) / Wide** variants selected via `IsMobile()`.
- Adding a screen? Use the `add-view-viewmodel` skill. Sanet.MVVM framework questions? Use the `sanet-mvvm` skill.
- Deep dive: `docs/architecture.md`.

## Repository Hazards

- `src/XF` and `src/Web` are **legacy archives** — not in any solution, not built, not covered by CI. Reference only; do not modify. Same for `tests/Web`.
- `libs/SanetLegacyEncryption` is a **binary-only** dependency referenced via HintPath — no source in this repo.
- `infra/` is a separate Pulumi C# solution (net6.0) with its own README — not part of the app solution.
- CI (`.github/workflows/`) triggers on `main` only; publish workflows also run on manual dispatch.

## Documentation

Start at `docs/INDEX.md` and load only the documents you need rather than bulk-loading everything.

## Skills

`skills/` holds skills maintained in this repo. Shared skills (e.g. `sanet-mvvm`, `generate-unit-tests`, `navigate-docs`) are installed from other repos into `.agents/skills` (git-ignored):

- Install/update all skills: `mise run install-skills`

## MCP Tools

Serena MCP provides symbolic C# navigation/editing: `mise run install-serena` (project config in `.serena/project.yml`). Prefer using serena tools when working with the code base.
