# Magical Yatzy #

Remake of old [WindowsPhone/UWP game](https://github.com/anton-makarevich/MagicalYatzyLegacy).

Now, I use it as a playground for anything I would like to try with AvaloniaUI.

[![CodeFactor](https://www.codefactor.io/repository/github/anton-makarevich/magicalyatzyxf/badge)](https://www.codefactor.io/repository/github/anton-makarevich/magicalyatzyxf)

## Repository layout

| Path | Contents |
|------|----------|
| `src/MagicalYatzy.Core` | Game logic, ViewModels, services (.NET 9) |
| `src/MagicalYatzy.Dto` | DTOs and API contracts |
| `src/Avalonia` | AvaloniaUI clients |
| `src/Web`, `src/XF` | legacy projects (see Archive) |
| `tests/` | mirrors `src/`; Core and DTO tests are included in the Avalonia solution |
| `infra/` | Pulumi landing zones for Azure (`MagicalYatzy.Infra.slnx`) |

## Core

Logic, ViewModels, DTOs — .NET 9

[![Build and Test](https://github.com/anton-makarevich/MagicalYatzyXF/actions/workflows/test.yml/badge.svg)](https://github.com/anton-makarevich/MagicalYatzyXF/actions/workflows/test.yml)
[![codecov](https://codecov.io/gh/anton-makarevich/MagicalYatzyXF/branch/develop/graph/badge.svg)](https://codecov.io/gh/anton-makarevich/MagicalYatzyXF/branch/develop)

## Clients (AvaloniaUI)

### WASM
[![Build WASM](https://github.com/anton-makarevich/MagicalYatzyXF/actions/workflows/build-wasm.yml/badge.svg)](https://github.com/anton-makarevich/MagicalYatzyXF/actions/workflows/build-wasm.yml)

### Android
[![Build Android](https://github.com/anton-makarevich/MagicalYatzyXF/actions/workflows/build-android.yml/badge.svg)](https://github.com/anton-makarevich/MagicalYatzyXF/actions/workflows/build-android.yml)

### iOS
[![Build iOS](https://github.com/anton-makarevich/MagicalYatzyXF/actions/workflows/build-ios.yml/badge.svg)](https://github.com/anton-makarevich/MagicalYatzyXF/actions/workflows/build-ios.yml)

### Windows
[![Build Windows](https://github.com/anton-makarevich/MagicalYatzyXF/actions/workflows/build-windows.yml/badge.svg)](https://github.com/anton-makarevich/MagicalYatzyXF/actions/workflows/build-windows.yml)

### Mac
[![Build Mac](https://github.com/anton-makarevich/MagicalYatzyXF/actions/workflows/build-mac.yml/badge.svg)](https://github.com/anton-makarevich/MagicalYatzyXF/actions/workflows/build-mac.yml)

### Linux
[![Build Linux](https://github.com/anton-makarevich/MagicalYatzyXF/actions/workflows/build-linux.yml/badge.svg)](https://github.com/anton-makarevich/MagicalYatzyXF/actions/workflows/build-linux.yml)

## Archive

Legacy projects kept for reference only; they are not part of any solution or CI:

- `src/XF` — Xamarin.Forms clients, 10+ years old and not relevant
- `src/Web/Functions` — Azure Functions backend (Login, ScoreSaver), with tests under `tests/Web/Functions`
