# ADR-0005 — Customization & end-user API (the headline UX)

- Status: **Accepted**
- Date: 2026-06-02
- Deciders: user ("for end user it should be easy to define custom nodes and links")

## Context

The top UX goal is that defining custom nodes and links is *easy* and *idiomatic*. Blazor.Diagrams does
this with `RegisterComponent<TModel, TComponent>()` mapping a model type to a Razor component. We need
the Avalonia-native equivalent, plus clean extension points for ports, links, routers, path generators,
anchors, behaviors, and theming — all without reflection magic.

## Decision

### Custom nodes — two tiers, both trivial

1. **Subclass + template (default):**
   ```csharp
   public sealed class TaskNode : NodeModel { public string Title { get; set; } = ""; }

   // register once (explicit, no scanning):
   canvas.RegisterNode<TaskNode>(node => new Border { Child = new TextBlock { Text = node.Title } });
   // or with a FuncDataTemplate / IDataTemplate for full Avalonia templating + binding
   ```
2. **Bring-your-own object (no subclassing):** any object can be a node's data; resolve a template by
   the data's type. Lets MVVM users template their existing view models directly.

Template resolution: a registry keyed by runtime type, falling back to Avalonia's `DataTemplates`
lookup, falling back to a built-in default node. No reflection scanning of assemblies.

### Custom links — composable, not monolithic

A link's appearance is the composition of small, individually replaceable pieces:
- **Router** (`IRouter`) — waypoints. Built-ins: Normal, Orthogonal. Set per-link or as a default.
- **PathGenerator** (`IPathGenerator`) — `PathData` from waypoints. Built-ins: Straight, Smooth.
- **Style** — brush, thickness, dash, markers, selected/invalid pseudo-classes (theme-driven).
- **Optional link template** — opt-in `DataTemplate` for fully custom interactive link content.
- **Labels / vertices / markers** — add `LinkLabelModel`, `LinkVertexModel`, `LinkMarker` to the model.

Defining a custom link is usually "set a router/path-generator and a style"; full custom rendering is
the opt-in escape hatch.

### Other extension points (all explicit registration)

- **Ports / anchors:** custom `PortModel` + port template; choose an anchor strategy per endpoint.
- **Behaviors:** `diagram.RegisterBehavior(new MyBehavior(diagram))` / `UnregisterBehavior<T>()`.
- **Widgets:** add `GridWidget`, `NavigatorWidget`, etc. to the canvas; custom widgets implement a
  small contract.
- **Theme:** override `NodelyTheme` resources/`ControlTheme`s; style by model-state classes.

### API principles

- **Explicit over implicit:** every customization is registered by the consumer; no assembly scanning,
  no hidden conventions, DI-friendly.
- **Small, honest interfaces** (`IRouter`, `IPathGenerator`, `IBehavior`, template registries) — ISP/OCP:
  extend by adding implementations, not editing switches.
- **MVVM-agnostic:** customization never forces a base view model or MVVM framework.
- **Discoverable defaults:** sensible default node/link/port look so a blank canvas is useful immediately.

## Consequences

- A custom node is ~3-10 lines (model + template registration). A custom link is usually a one-liner
  (pick router/path/style) with a full-template escape hatch.
- Aligns with Avalonia idioms (DataTemplates, styles, classes) so users reuse knowledge they already have.
- The registry + small interfaces keep the engine closed for modification, open for extension.
