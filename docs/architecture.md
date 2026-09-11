# Architecture

## Solutions

- `src/Avalonia/MagicalYatzy.Avalonia.slnx` — the app: Core, Dto, shared Avalonia UI, 4 platform heads, Core tests.
- `infra/MagicalYatzy.Infra.slnx` — Azure infrastructure (Pulumi, net6.0), independent of the app.

## Layers

Dependency flow: `Avalonia heads → MagicalYatzy.Avalonia → MagicalYatzy.Core → MagicalYatzy.Dto`.

### MagicalYatzy.Core (`src/MagicalYatzy.Core`, net9.0)

Root namespace `Sanet.MagicalYatzy`. No UI dependencies.

- `Models/Game` — game domain: `YatzyGame`, `DicePanel`, `Die`, rules and scoring, `Ai/BotDecisionMaker`, `IDiceGenerator` with `RandomDiceGenerator` (dice randomness is injectable for tests). Dice animation frames are embedded PNG resources in this project.
- `ViewModels` — all screen ViewModels. All inherit `Base/DicePanelViewModel` (which extends Sanet.MVVM `BaseViewModel` and exposes the shared `IDicePanel`). `ObservableWrappers/` holds UI-facing wrappers around models (e.g. `PlayerViewModel`).
- `Services` — `Api` (`AzureApiClient`, `LegacyWcfClient`), `Game` (`GameService`, `PlayerService`, `RulesService`, `GameSettingsService`), `Localization` (`GlobalizationInvariantLocalizationService` over `Resources/Strings.resx`), `Storage` (`LocalJsonStorageService`), `Navigation` (`IExternalNavigationService`), `Media` (`ISoundsProvider`).

### MagicalYatzy.Dto (`src/MagicalYatzy.Dto`, net9.0, nullable enabled)

Request/response DTOs, `IWebService`/`WebService`, and the WCF connected-service reference for the legacy score service. Referenced by Core.

### MagicalYatzy.Avalonia (`src/Avalonia/MagicalYatzy.Avalonia`, net9.0, nullable enabled)

Shared UI for all heads, plus the **composition root**:

- `App.axaml.cs` — on startup pulls the `IServiceCollection` from AppBuilder resources, calls `RegisterServices()` + `RegisterViewModels()` (`DependencyInjection/CoreServices.cs`), builds the provider, creates the platform-appropriate `INavigationService` (`NavigationService` for desktop windows, `SingleViewNavigationService` for mobile/browser), calls `RegisterViews(...)`, then shows `MainMenuView` with `MainMenuViewModel`.
- `CoreServices.RegisterServices()` — services are singletons, except `IDicePanel` (transient). ViewModels are transient. Platform stubs (`ExternalNavigationStub`, `SoundsProviderStub`) live in `Services/Stubs`; heads can override registrations with their own DI modules (e.g. `BrowserServices`).
- `RegisterViews(...)` maps view types to ViewModel types. `Lobby` and `Game` have **Narrow/Wide** variants; `IsMobile()` (iOS/Android) selects Narrow, everything else Wide.
- `Views/` — all views inherit `Base/DicePanelView<TViewModel>` (Sanet.MVVM `BaseView<T>`), which hosts the animated dice background. Compiled bindings are on by default — always set `x:DataType`. Subfolders: `Game/`, `Lobby/`, `Fragments/`, `Cells/`, `TemplatedControls/`, plus Converters, Styles, Assets.

### Platform heads (`src/Avalonia/MagicalYatzy.Avalonia.{Desktop,Android,iOS,Browser}`)

Thin entry points (1–3 files each): `Program.cs`/`MainActivity`/`AppDelegate` + optional head-specific DI module. Desktop is the local development head (`dotnet run`); Browser is WASM (published to Azure static website); Android/iOS need restored workloads.

## Testing

`tests/MagicalYatzy.Core.Tests` mirrors the Core structure: xUnit + Shouldly + NSubstitute, coverlet for coverage (opencover → Codecov in CI). UI projects are intentionally untested — testable logic belongs in Core.

## Legacy boundaries (do not modify)

- `src/XF` — the original Xamarin.Forms app (6 projects), kept for reference. Not in any solution.
- `src/Web` — legacy Azure Functions API (netcoreapp2.1) + swagger stub. Not in any solution.
- `tests/Web` — tests for the legacy Functions. Not in CI.
- `libs/SanetLegacyEncryption` — binary-only DLL dependency (HintPath), no source.

## Infrastructure

`infra/` — Pulumi C# projects (`AzureLZ` landing zone, `AzureResources`) on net6.0, deployed manually (`az login` / service principal, see `infra/README.md`).
