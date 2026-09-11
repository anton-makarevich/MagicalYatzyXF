# Magical Yatzy #

Remake of old [WindowsPhone/UWP game](https://github.com/anton-makarevich/MagicalYatzyLegacy).

Now, I use it as a playground for anything I would like to try with AvaloniaUI.

[![CodeFactor](https://www.codefactor.io/repository/github/anton-makarevich/magicalyatzyxf/badge)](https://www.codefactor.io/repository/github/anton-makarevich/magicalyatzyxf)

## Repository layout

| Path | Contents |
|------|----------|
| `src/MagicalYatzy.Core` | Game logic, ViewModels, services (.NET 9) |
| `src/MagicalYatzy.Dto` | DTOs and API contracts |
| `src/Avalonia` | AvaloniaUI clients (`MagicalYatzy.Avalonia.slnx`) |
| `src/Web`, `src/XF` | legacy projects (see Archive) |
| `tests/` | mirrors `src/`; Core and DTO tests are included in the Avalonia solution |
| `infra/` | Pulumi landing zones for Azure (`MagicalYatzy.Infra.slnx`) |

## Core

Logic, ViewModels, DTOs — .NET 9

[![Build and Test](https://github.com/anton-makarevich/MagicalYatzyXF/actions/workflows/test.yml/badge.svg)](https://github.com/anton-makarevich/MagicalYatzyXF/actions/workflows/test.yml)
[![codecov](https://codecov.io/gh/anton-makarevich/MagicalYatzyXF/branch/develop/graph/badge.svg)](https://codecov.io/gh/anton-makarevich/MagicalYatzyXF/branch/develop)

## Clients (AvaloniaUI)

### WASM
[![Build SPA](https://github.com/anton-makarevich/MagicalYatzyXF/actions/workflows/publish-wasm.yml/badge.svg)](https://github.com/anton-makarevich/MagicalYatzyXF/actions/workflows/publish-wasm.yml)

[Try it here in the browser](https://magicalyatzystoragedev.z6.web.core.windows.net/)

### Android
[![Publish Android App](https://github.com/anton-makarevich/MagicalYatzyXF/actions/workflows/publish-android.yml/badge.svg?branch=develop)](https://github.com/anton-makarevich/MagicalYatzyXF/actions/workflows/publish-android.yml)

[Download APK](https://install.appcenter.ms/users/anton.makarevich/apps/magical-yatzy-android/distribution_groups/alpha)

### iOS

[![Publish iOS App](https://github.com/anton-makarevich/MagicalYatzyXF/actions/workflows/publish-ios.yml/badge.svg?branch=develop)](https://github.com/anton-makarevich/MagicalYatzyXF/actions/workflows/publish-ios.yml)

[Download IPA](https://install.appcenter.ms/users/anton.makarevich/apps/magical-yatzy-ios/distribution_groups/alpha)
(Contact me if you want to be added to the profile)

### Windows     
[![Publish Windows App](https://github.com/anton-makarevich/MagicalYatzyXF/actions/workflows/publish-windows.yml/badge.svg)](https://github.com/anton-makarevich/MagicalYatzyXF/actions/workflows/publish-windows.yml)

[Download installer](https://install.appcenter.ms/users/anton.makarevich/apps/magical-yatzy-win/distribution_groups/alpha)

### Mac   
[![Publish Mac App](https://github.com/anton-makarevich/MagicalYatzyXF/actions/workflows/publish-mac.yml/badge.svg?branch=develop)](https://github.com/anton-makarevich/MagicalYatzyXF/actions/workflows/publish-mac.yml)

[Download installer](https://install.appcenter.ms/users/anton.makarevich/apps/magical-yatzy-mac/distribution_groups/alpha)

### Linux

[![Publish Linux App](https://github.com/anton-makarevich/MagicalYatzyXF/actions/workflows/publish-linux.yml/badge.svg)](https://github.com/anton-makarevich/MagicalYatzyXF/actions/workflows/publish-linux.yml)

[Download archive](https://install.appcenter.ms/users/anton.makarevich/apps/magical-yatzy-linux/distribution_groups/alpha)

## Archive

Legacy projects kept for reference only; they are not part of any solution or CI:

- `src/XF` — Xamarin.Forms clients, 10+ years old and not relevant
- `src/Web/Functions` — Azure Functions backend (Login, ScoreSaver), with tests under `tests/Web/Functions`
