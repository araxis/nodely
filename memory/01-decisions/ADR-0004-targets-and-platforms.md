# ADR-0004 — Targets, platforms, tooling

- Status: **Accepted**
- Date: 2026-06-02

## Context

We need to pin frameworks, the Avalonia version, UI style, MVVM stance, and tooling so the plan is
concrete. Source-first: latest stable Avalonia is **12.0.4** (published 2026-05-28, verified on
nuget.org/packages/Avalonia). Latest `Z.Blazor.Diagrams` is 3.0.4.1 (reference only; we port, not depend).

## Decision

- **Avalonia 12.0.4** (latest stable). Re-verify the exact patch at scaffold time; pin via central
  package management.
- **.NET 8 (LTS)** as the primary TFM for apps/controls. `Nodely.Core`, `Nodely.Algorithms`,
  `Nodely.Serialization` multi-target `netstandard2.0;net8.0` for maximum reuse. `Nodely.Avalonia`
  targets `net8.0` (Avalonia 12 minimum). Add `net9.0`/`net10.0` later if a feature needs it.
- **C#-only UI** (no XAML) per user convention. Controls, default styles, and `ControlTheme`s are built
  in C#. The library ships a C# `NodelyTheme : Styles` object consumers add to `Application.Styles`.
  (We still verify Avalonia theming APIs against source; if a resource-dictionary include is unavoidable
  for some control template, we isolate it and document why.)
- **MVVM-agnostic core.** `Nodely.Core` models raise their own `Changed` notifications and do not
  depend on any MVVM framework. The **sample app** uses **CommunityToolkit.Mvvm**. Consumers may use
  CommunityToolkit.Mvvm, ReactiveUI, or plain objects.
- **Central Package Management** (`Directory.Packages.props`) + `Directory.Build.props` (nullable enable,
  `LangVersion` latest, deterministic builds, SourceLink, XML docs, warnings-as-errors in CI).
- **Testing**: xUnit + Shouldly for the brain; `Avalonia.Headless.XUnit` for control/interaction tests;
  BenchmarkDotNet (Phase 13) for performance.
- **Repo**: git (currently NOT a git repo — to be initialized in Phase 0), MIT license,
  `THIRD-PARTY-NOTICES` for Blazor.Diagrams attribution, GitHub Actions CI (build + test + pack).

## Platform reach

- **Tier 1 (verified continuously):** Desktop — Windows, Linux, macOS.
- **Tier 2 (designed-for, verified later):** Browser (WASM) and mobile (Android/iOS). The hybrid
  renderer uses only portable Avalonia primitives; avoid desktop-only APIs in the library. Performance
  on WASM revisited in Phase 13.

## Consequences

- LTS + latest stable Avalonia balances longevity and current APIs.
- `netstandard2.0` on the brain keeps it usable from the widest set of hosts and tools.
- C#-only theming is more code than a XAML dictionary but matches the user's convention and keeps the
  whole library navigable in one language.

## Update 2026-06-02 (Phase 0 scaffolding — toolchain reality)

Verified against the actual dev machine and nuget.org; several pins changed from the original draft:

- **Primary TFM is now `net10.0`, not `net8.0`.** The machine has .NET 10 SDK (10.0.300, stable) and
  .NET 11 SDK (preview) — no .NET 8 SDK. **.NET 10 is the current LTS** (GA Nov 2025). So apps/controls/
  tests target `net10.0`; `Nodely.Core/.Algorithms/.Serialization` multi-target `netstandard2.0;net10.0`
  (netstandard2.0 still covers net8.0 consumers). `global.json` pins SDK `10.0.300`, `rollForward
  latestMinor`, `allowPrerelease false` so builds use stable 10.x, not the 11 preview. (F-007)
- **Solution format is `.slnx`** — `dotnet new sln` defaults to the new XML solution format on .NET 10.
  Build/test/CICD reference `Nodely.slnx`. (F-010)
- **Test stack is xUnit v3**, because `Avalonia.Headless.XUnit` 12.x depends on `xunit.v3.extensibility.core`.
  Pins: `xunit.v3` 3.2.2, `xunit.runner.visualstudio` 3.1.5, `Microsoft.NET.Test.Sdk` 17.12.0; v3 test
  projects need `<OutputType>Exe</OutputType>`. (F-009)
- **`Avalonia.Diagnostics` dropped for now** — no 12.x release exists (latest 11.3.17). Re-add when a 12.x
  ships. (F-008)
- Verified working: `dotnet build` (0 warnings/0 errors) and `dotnet test` (Core 2/2, Avalonia headless
  1/1) on net10.0 with Avalonia 12.0.4.

Net-effect on platform reach: a `net8.0` TFM for `Nodely.Avalonia` (for consumers on .NET 8) is deferred
to the packaging phase; trivial to add later. `netstandard2.0` already serves net8.0 consumers of the brain.
