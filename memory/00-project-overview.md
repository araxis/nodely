# Project Overview — Nodely

## What it is

**Nodely** is a .NET component library that brings the capabilities of
[Z.Blazor.Diagrams / Blazor.Diagrams](https://github.com/Blazor-Diagrams/Blazor.Diagrams) to
**Avalonia UI**. It is an interactive node/graph/diagram editor toolkit: a pannable, zoomable
canvas with nodes, ports, links, groups, labels, routers, path generators, behaviors, and widgets
— rebuilt natively for Avalonia instead of Blazor's HTML/SVG/JS rendering model.

It is **not** a wrapper around Blazor or a WebView. It is a native Avalonia control library backed by
a first-party, UI-agnostic model/behavior engine (`Nodely.Core`) that is a faithful port of the
proven Blazor.Diagrams.Core design.

## Who it is for

App developers building, inside Avalonia desktop (and later mobile/browser) apps:
workflow builders, node editors, ETL/automation canvases, topology/network maps, state machines,
dependency graphs, database/ERD designers, mind maps, visual mappers, and read-only inspectors.

## Priorities (in order, set by the user)

1. **Performance** — smooth pan/zoom/drag with large graphs; immediate-mode rendering for
   high-count visuals (links, grid, overview); virtualization for nodes; batched refresh; minimal allocations.
2. **Customizability** — every visual and interaction is replaceable: custom nodes, ports, links,
   groups, labels, routers, path generators, anchors, behaviors, and theme.
3. **Ease for end users** — defining a custom node or link must be trivial: subclass a model and
   register a `DataTemplate`. This is the single most important UX goal.

Then: precise, mature, well-designed, SOLID/KISS, vertical-slice, testable, source-grounded.

## Goals

- A clean, Avalonia-native, **MVVM-agnostic** API (works with CommunityToolkit.Mvvm, ReactiveUI, or plain objects).
- Faithful feature parity with Blazor.Diagrams' core capabilities (see `02-research/api-surface-and-mapping.md`).
- C#-only UI (no XAML required to build or consume), per project convention.
- Multi-package: a reusable headless brain (`Nodely.Core`) + Avalonia controls (`Nodely.Avalonia`) +
  optional `Nodely.Algorithms`, `Nodely.Serialization`, and side packages such as Database, UML, Workflow,
  MindMap, and StateMachine.
- Strong test coverage: pure unit tests for the brain, Avalonia headless tests for controls/interaction.

## Non-goals (initially)

- Pixel-identical visual reproduction of Blazor.Diagrams' default CSS (we ship an Avalonia-native theme instead).
- A WebView/Blazor interop bridge.
- Server-side or remote diagram sync (the library is local/in-process; apps add their own persistence).
- Re-implementing every niche algorithm on day one — `Nodely.Algorithms` grows on demand.

## Definition of "done" for the whole project (v0.1.0)

A developer can `dotnet add package Nodely.Avalonia`, drop a `DiagramCanvas` into an Avalonia app,
create nodes/links in code, define a custom node with ~10 lines (model + `DataTemplate`), and get a
performant editor with pan, zoom, multi-select, drag-move, interactive link creation, grouping,
snap-to-grid, an overview/navigator, zoom-to-fit, read-only mode, theming, serialization, and
undo/redo — all verified by headless tests and a sample gallery app.

## Key architectural insight (the reason this is feasible)

`Blazor.Diagrams.Core` has **no Blazor dependency** — its only dependency is `SvgPathProperties`,
and it defines its own pointer/keyboard/wheel event types. The Blazor-specific work lives entirely
in the rendering package (`Blazor.Diagrams`: `DiagramCanvas`, renderers, widgets, and a small JS
interop file for `ResizeObserver`/`getBoundingClientRect`). Therefore the mapping to Avalonia is:

- **Port the brain** (`Nodely.Core`) — models, behaviors, geometry, routers, path generators, anchors,
  layers, virtualization — as first-party, rendering-neutral code.
- **Re-implement the rendering + input layer** (`Nodely.Avalonia`) using Avalonia controls, layout,
  pointer events, and `DrawingContext` — natively replacing the HTML/SVG/CSS/JS layer.

See `02-research/` for the full source-grounded map.
