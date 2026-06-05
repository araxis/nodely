# Nodely — Project Memory

This folder is the **single source of truth** for project decisions, research, the development
plan, progress, and learnings. It is tracked in git so the reasoning behind the code is never lost.

> Rule of the project: **check docs and source before guessing.** Every non-obvious claim here
> should cite where it came from (upstream source path, doc URL, or a test we wrote).

## Layout

```
memory/
  README.md                         <- you are here (index + how to use)
  00-project-overview.md            What Nodely is, goals, non-goals, priorities
  glossary.md                       Domain vocabulary (node, port, link, anchor, router, ...)
  01-decisions/                     Architecture Decision Records (ADRs) — one decision each
    ADR-0001-name-and-packaging.md
    ADR-0002-core-strategy.md
    ADR-0003-rendering-architecture.md
    ADR-0004-targets-and-platforms.md
    ADR-0005-customization-api.md
  02-research/                      Source-first findings (the "map from" knowledge)
    blazor-diagrams-architecture.md Upstream architecture map (Core vs rendering)
    api-surface-and-mapping.md      Blazor.Diagrams -> Nodely/Avalonia mapping tables
    avalonia-mapping-notes.md       How Avalonia natively replaces the Blazor JS/CSS/SVG layer
    extension-surface-investigation.md
                                      Side-package extension and versioning audit
  03-plan/                          The development plan
    development-plan.md             Full phased plan, each phase with a Definition of Done
    phase-checklist.md              Trackable checkboxes mirroring the plan
  04-progress/                      Running record
    progress-log.md                 Dated log of what changed and why
    findings-and-learnings.md       Gotchas, surprises, reversed decisions, experiences
```

## How to use this folder

- **Before a decision**: write/append an ADR in `01-decisions/`. Status: Proposed -> Accepted -> Superseded.
- **Before building a phase**: re-read the phase in `03-plan/development-plan.md` and its DoD.
- **While building**: tick `03-plan/phase-checklist.md`; append to `04-progress/progress-log.md`.
- **When surprised**: record it in `04-progress/findings-and-learnings.md` immediately (cheap insurance).
- **When upstream behavior matters**: verify against source/docs and cite it in `02-research/`.

## Quick status

- Phase: **side-package roadmap.** 188 tests per app runtime currently expected across Core, side-package,
  and Avalonia suites on both `net8.0` and `net10.0`.
- v0.1.0: 15 phases (0–14), M1–M4. v0.2.0 (F-027…F-039): editor/interaction wave. v0.3.0 (F-041): 10 extension
  seams (render hooks for links/ports/groups, custom layers, adorners, validation delegates, behaviors,
  IDiagramLayout, Tag/Data bag, serialization extras) — lean framework, not built-in features.
- v0.4.0 (F-042): undoable z-order/group/bend-point edits, label/dependent-link refresh fixes,
  warnings-as-errors, and release checklist docs.
- v0.5.0 (F-043): command-state helpers, QuickStart sample, richer gallery, and copyable docs recipes.
- v0.6.0 (F-044): `net8.0` package assets for Avalonia plus explicit `net8.0`/`net10.0` validation.
- v0.7.0 (F-045, F-046): first optional domain pack plus stable side-pack contract: serializer registry,
  typed style registration, render context, and independent side-package versioning.
- UML side package (F-047): second optional domain pack and generalized side-package package workflow.
- Workflow side package (F-048): third optional domain pack, kept to models/renderers/serialization only.
- MindMap side package (F-051): fourth optional domain pack with pack-local arrange/collapse helpers.
- Docs: static documentation site (F-040) + Extensibility guide; GitHub Pages pipeline.
- Last updated: 2026-06-05
