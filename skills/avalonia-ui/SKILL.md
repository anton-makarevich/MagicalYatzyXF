---
name: avalonia-ui
description: Query the avalonia-docs MCP server for authoritative, up-to-date Avalonia documentation. Use this skill whenever working on any Avalonia UI code in this repo — AXAML files, styles, control templates, data binding (especially compiled bindings with x:DataType), custom controls, animations, threading, or any Avalonia API/behavior question — even for seemingly small tasks like adding a binding, a style selector, or a new control property. Also use when porting XAML patterns from the legacy WP7/UWP app (MagicalYatzyLegacy), or whenever you are about to write Avalonia markup or API code from memory instead of verifying it.
---

# Avalonia Docs MCP

This repo has the `avalonia-docs` MCP server connected. It serves the official Avalonia documentation and expert rules directly into the session. Use it instead of relying on memorized Avalonia knowledge — Avalonia evolves quickly, and this repo targets a recent .NET/Avalonia stack where modern patterns (compiled bindings, `ControlTheme`, styled/direct properties) are the norm. Wrong-from-memory AXAML usually fails silently at runtime (broken bindings) rather than at compile time, so verifying against the docs is cheap insurance.

## Scope

Valuable **only for the Avalonia part** of the project: `src/Avalonia/**` (views, styles, heads) and Avalonia-specific code elsewhere. Not useful for game logic, DTOs, services, or tests.

## Tools

### `avalonia-docs_get_avalonia_expert_rules`

Comprehensive Avalonia development rules: AXAML syntax, property system (`StyledProperty` vs `DirectProperty`), styling, data binding, MVVM, custom controls, layout, theming, threading, and common mistakes.

**When:** Call **once** at the start of any Avalonia-focused task or session — not per file. This is the highest-value single call; it prevents the most common classes of mistakes up front.

### `avalonia-docs_search_avalonia_docs`

Full-text search over the official docs. Returns entire doc pages, so results are token-heavy.

**When:** Any specific question about controls, styling, binding, animation, etc.

**How:**
- Be specific: `"TreeView data binding"`, `"Style selector pseudo-classes"`, `"custom control StyledProperty"` — not `"binding"` or `"styles"`.
- Keep `max_results` low (3–5). Results include full page content; more results waste context.
- If the first query misses, refine the terms rather than broadening them.

### `avalonia-docs_lookup_avalonia_api`

Intended for looking up a specific class, property, method, or event by name.

**Known issue (verified 2026-09-12):** currently returns `No results found` even for common types (`TextBlock`, `Button`, `StyledProperty`). If it returns nothing, **do not conclude the API doesn't exist** — fall back to `search_avalonia_docs` with the type or concept name. Re-test this tool occasionally; if it starts returning results, prefer it for targeted API questions since it is cheaper than search.

### `avalonia-docs_lookup_wpf_to_avalonia_mapping`

Focused WPF→Avalonia mapping tables for a single topic (`controls`, `styling`, `bindings`, `properties`, `gotchas`, etc.).

**When:** Porting XAML/code patterns from the legacy app (`../MagicalYatzyLegacy` — WP7/UWP) or whenever WPF habits leak into Avalonia code (e.g. `Visibility` vs `IsVisible`, `DependencyProperty` vs `AvaloniaProperty`, WPF style selectors).

### Migration tools — usually NOT for this repo

`avalonia-docs_analyze_wpf_project`, `avalonia-docs_migrate_to_avalonia`, `avalonia-docs_migrate_to_xpf`, and `avalonia-docs_migrate_diagnostics` support migrating a **WPF application** to Avalonia. This repo is already native Avalonia, so do not call these during normal development. The only exception: `migrate_diagnostics` when setting up or fixing Avalonia DevTools configuration.

## Working habits

- Verify before writing: when unsure about a control's property, a style selector, or binding syntax, search first — don't guess and don't invent API names.
- One `get_avalonia_expert_rules` call per session is enough; cache what you learned and only search for gaps.
- Combine with other repo skills, don't overlap them:
  - `sanet-mvvm` — ViewModels, navigation, DI, framework behavior.
  - `add-view-viewmodel` — adding a new screen (registration ritual).
- When docs and repo conventions conflict, repo conventions win — but flag the conflict to the user.
