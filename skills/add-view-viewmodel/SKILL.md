---
name: add-view-viewmodel
description: Use when adding a new screen (View + ViewModel pair) to the MagicalYatzy Avalonia app. Covers the DI registration, RegisterViews mapping, and Narrow/Wide variant ritual that must all be completed for navigation to work.
---

# Add a View + ViewModel pair

MagicalYatzy uses Sanet.MVVM viewmodel-first navigation. A new screen works only if **all** steps below are done — a missing step fails at runtime (at navigation time), not at compile time.

## Checklist

1. **Create the ViewModel** in `src/MagicalYatzy.Core/ViewModels/<Name>ViewModel.cs`
   - Inherit `DicePanelViewModel` (`Sanet.MagicalYatzy.ViewModels.Base`) — every screen shows the animated dice background. Constructor takes `IDicePanel` plus any required services, all constructor-injected.
   - Namespace is `Sanet.MagicalYatzy.ViewModels` (no `.Core` segment).
   - Override `AttachHandlers()` / `DetachHandlers()` to subscribe/unsubscribe from events (see `MainMenuViewModel` for the pattern).
   - Navigate via the inherited `NavigationService`: `await NavigationService.NavigateToViewModelAsync<OtherViewModel>()`, or `NavigateToRootAsync()` to go back to the root. Never reference view types.

2. **Register the ViewModel** in `src/Avalonia/MagicalYatzy.Avalonia/DependencyInjection/CoreServices.cs`, inside `RegisterViewModels()`:
   `services.AddTransient<<Name>ViewModel, <Name>ViewModel>();`
   (Services go in `RegisterServices()` — singletons, except `IDicePanel` which is transient.)

3. **Create the View(s)** in `src/Avalonia/MagicalYatzy.Avalonia/Views/`
   - Code-behind inherits `DicePanelView<<Name>ViewModel>`; make the AXAML root element the same base class; set `x:DataType` to the ViewModel (compiled bindings are on by default).
   - Choose the variant strategy:
     - **Single view** (like `MainMenuView`, `SettingsView`) when one layout fits all platforms.
     - **Narrow/Wide pair** (`<Name>ViewNarrow`, `<Name>ViewWide`, like `LobbyView*` and `GameView*`) when mobile needs a distinct layout.

4. **Map view(s) to the ViewModel** in `App.axaml.cs`, inside `RegisterViews(...)`:
   - Single: `navigationService.RegisterViews(typeof(<Name>View), typeof(<Name>ViewModel));`
   - Pair: register the Narrow view inside `if (IsMobile())` and the Wide view in the `else` branch.

## Verify

- `dotnet build MagicalYatzy.Avalonia.slnx` compiles.
- `dotnet run --project src/Avalonia/MagicalYatzy.Avalonia.Desktop` — navigate to the new screen, confirm it renders and binds.
- Add ViewModel tests in `tests/MagicalYatzy.Core.Tests/ViewModels/` (use the `generate-unit-tests` skill).

## Common mistakes

- Forgetting `RegisterViews` → navigation throws at runtime.
- Registering a Narrow/Wide pair outside the `IsMobile()` branches, or registering only one variant.
- Putting the ViewModel in the Avalonia project — ViewModels always live in `MagicalYatzy.Core`.
- Binding without `x:DataType` → silently broken bindings (compiled bindings are the default).
