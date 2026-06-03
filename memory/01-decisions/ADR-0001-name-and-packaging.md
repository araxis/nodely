# ADR-0001 — Name and packaging

- Status: **Accepted**
- Date: 2026-06-02
- Deciders: user

## Context

We are building a .NET library that maps Blazor.Diagrams to Avalonia. We need a product name and a
package/assembly layout. The user wants the name to be brandable and not locked to Avalonia forever
(room to grow). The Avalonia C# skill discourages squatting the reserved `Avalonia.` namespace prefix
for non-official packages.

## Decision

**Name: `Nodely`.** Framework-neutral, brandable, easy to search.

**Packages / projects:**

| Package | Target | Responsibility |
|---|---|---|
| `Nodely.Core` | `netstandard2.0;net8.0` | UI-agnostic brain: models, layers, behaviors, geometry, routers, path generators, anchors, options, neutral events. No UI dependency. |
| `Nodely.Avalonia` | `net8.0` | Avalonia controls: `DiagramCanvas`, node/port/link/group/label rendering, widgets, input translation, theme. References `Nodely.Core` + Avalonia 12. Main consumer package. |
| `Nodely.Algorithms` | `netstandard2.0;net8.0` | Optional: auto-layout, traversal, graph utilities. References `Nodely.Core`. |
| `Nodely.Serialization` | `netstandard2.0;net8.0` | Optional: versioned snapshot DTOs + System.Text.Json round-trip. References `Nodely.Core`. |
| `Nodely.Demo` | `net8.0` (desktop) | Sample gallery app. Not published. References `Nodely.Avalonia`. |
| `Nodely.Core.Tests` | `net8.0` | xUnit + Shouldly. |
| `Nodely.Avalonia.Tests` | `net8.0` | Avalonia.Headless.XUnit + Shouldly. |

Root namespace: `Nodely`. Sub-namespaces mirror folders (`Nodely.Geometry`, `Nodely.Models`,
`Nodely.Behaviors`, `Nodely.Avalonia.Controls`, etc.).

The working directory is currently `D:\Projects\AlaloniaDiagrams` ("Alalonia" is a typo of Avalonia).
We keep the folder but name everything inside `Nodely`. The solution file will be `Nodely.sln`.

## Consequences

- Clear separation lets a non-UI consumer (tests, tools, future WPF/MAUI port) use `Nodely.Core` alone.
- "Avalonia" appears only as a sub-package qualifier (`Nodely.Avalonia`), not as a top-level prefix —
  no reserved-namespace concern, and a future `Nodely.Wpf` fits the same pattern.
- Optional packages keep the default install lean (perf-by-default; pay for algorithms/serialization only if used).

## Alternatives considered

- `AvaloniaDiagrams` / `AvaDiagrams` — more discoverable but ties the brand to Avalonia and flirts
  with the reserved prefix. Rejected for growth flexibility.
- `Diavalo` — cute portmanteau, but locks to Avalonia and is less obvious.
