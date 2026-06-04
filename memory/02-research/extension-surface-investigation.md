# Extension surface investigation

Date: 2026-06-04

Purpose: review the pre-public extension surface before adding more side packages. Existing libraries are
reference material only; Nodely should choose the simplest, fastest Avalonia-first API.

## Evidence

- Read the current `DiagramCanvas` renderer registration surface, link drawing path, serializer snapshots,
  database pack models, demo scene, and package workflow.
- Ran database model save/load tests on `net8.0` and `net10.0`: 5/5 passed on both runtimes.
- Ran database render-registration headless test on `net8.0` and `net10.0`: 1/1 passed on both runtimes.
- Packed only `Nodely.Avalonia.Database` as `0.1.0`; the package produced correctly, but dependency
  groups incorrectly changed `Nodely.Avalonia` and `Nodely.Serialization` to `0.1.0`.

## Findings

### Keep

- Typed model classes in side packages are the right pattern. Database table, view, procedure, column,
  parameter, port, and relationship types keep domain concepts out of `Nodely.Core`.
- One app-facing canvas extension is good. `canvas.UseDatabaseNodes()` is discoverable and simple.
- C#-only pack renderers fit the repository style and avoid extra resource files.
- Dictionary-based renderer lookup by exact/derived type is simple and fast enough for the current design.
- Rebuilding node and port visuals on palette changes is the right canvas behavior.

### Redesign now

- Serialization must become type-safe for all diagram model kinds before more packs. Current save/load restores
  database nodes, but database ports reload as `PortModel` and database links reload as `LinkModel`, losing port
  roles, relationship kind, cardinality, and link styling metadata.
- Snapshot `Kind` values must stop using CLR type names. Packs need stable string keys such as
  `database.table`, `database.port`, and `database.relationship` so renames and namespaces do not become file
  format breaks.
- `LinkStyleResolver` must become a typed, composable registration API. The current single property forces packs
  to capture and chain the previous resolver; app code or a second pack can accidentally overwrite earlier pack
  styling.
- Render factories need a context overload. Pack renderers should receive palette/canvas context instead of using
  hard-coded brushes or closing over a canvas manually.
- Side package versions need project-owned properties. A command-line `PackageVersion` override leaks into
  project-reference dependency versions, so independent side-package versioning is not safe yet.
- The package workflow must pack and publish selected projects. The current workflow packs the whole solution and
  publishes every package artifact from a tag.

### Defer

- Runtime plugin loading is not needed. Package-level composition covers the current side-package goal.
- Full styling/theme option objects can wait until the typed render context exists; start with palette-aware
  defaults and typed style registration.
- Migration helpers for pre-public database snapshots can be minimal. The package has not been announced, so
  long-term compatibility with the first draft schema is not required.

### Reject

- Do not copy another library's API shape as a goal. Use proven ideas only when they produce cleaner Nodely APIs.
- Do not add domain concepts to `Nodely.Core`.
- Do not use one shared version for every future side package.
- Do not publish a side-package-only update by republishing unchanged main packages.

## Recommended extension-pack contract

- Model identity: add a virtual stable `ModelKind` and virtual extra-data hooks to the shared `Model` base.
  `NodeModel`, `PortModel`, `BaseLinkModel`, and `GroupModel` all participate through the same contract.
- Serialization: add a `DiagramSerializationRegistry` with typed registrations for nodes, ports, links, and
  groups. `DiagramSerializer.Deserialize` accepts the registry and falls back to base models when no kind is
  registered.
- Snapshot schema: bump to version 2 and add `Kind` plus nullable `Extra` to nodes, ports, links, and groups.
  Keep version 1 loading for existing base diagrams.
- Rendering: replace the single style resolver with typed registrations:
  `RegisterLinkStyle<TLink>(Func<TLink, LinkStyleContext, LinkStyle?> resolver)`. Exact/derived type lookup
  should match node/port/link drawer lookup, and the last registration for the same type wins.
- Render context: add context overloads for node, port, and group factories, passing the owning canvas and
  palette. Keep simple overloads as wrappers if useful.
- Pack shape: each side package exposes:
  - `canvas.UseDatabaseNodes()` for rendering.
  - `registry.UseDatabaseNodes()` for serialization.
  - stable model-kind constants for every persisted model type.
- Composition rule: multiple packs compose by registering different model types. If two packs register the same
  exact type, the later registration wins intentionally. App-local registrations after pack setup override pack
  defaults.

## Versioning and release contract

- Main package group keeps one version property: `NodelyMainVersion`.
- Each side package gets its own version property, starting with `NodelyAvaloniaDatabaseVersion`.
- Main projects set `PackageVersion` from `NodelyMainVersion`; side projects set `PackageVersion` from their
  own side-package property.
- A main tag `vX.Y.Z` packs and publishes only the main package group.
- A side-package tag `Nodely.Avalonia.Database/vX.Y.Z` packs and publishes only that side package.
- Workflow dispatch accepts a package id and publish flag; PRs build and test the solution, then pack the
  affected package set without publishing.
- Database should start at its own side-package version. If the extension-contract redesign requires main
  package changes, release those as the next main version and release database with its own first side-package
  version.

## Implementation plan for the redesign pass

1. Add shared model-kind and extra-data hooks on `Model`, then update node hooks to override the shared methods.
2. Add serializer schema version 2 with node, port, link, and group `Kind`/`Extra` fields plus a
   `DiagramSerializationRegistry`.
3. Update database models to declare stable kind constants and persist/restore table/view/procedure fields,
   database port kind/name, and relationship kind/cardinality through the registry.
4. Replace `LinkStyleResolver` with typed link-style registrations; update database and demo styling to use the
   typed API.
5. Add render-context overloads for node, port, and group factories; update database renderers to use palette
   context instead of fixed dark-only brushes.
6. Split package versions into main and side-package properties; update package workflow to select projects from
   tag shape or workflow input.
7. Update docs and tests to show both calls: `canvas.UseDatabaseNodes()` and serializer registry registration.
8. Re-run build, tests, pack, package inspection, docs build, and neutral wording scan.

## PR guidance

Revise PR #8 before merge. It is a useful stress test and much of the database model/rendering work can stay, but
the serializer, typed style registration, render context, and independent versioning contracts should be fixed
before this package becomes the pattern for UML or other side packages.

## Resolution

Implemented on 2026-06-04 before PR #8 was made ready again. The redesign added stable model kinds,
model-wide extra-data hooks, registry restore for nodes/ports/links/groups, typed link-style registrations,
render-context factory overloads, independent database package versioning, and selective package workflow
support.
