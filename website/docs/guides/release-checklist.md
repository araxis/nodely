---
id: release-checklist
title: Release checklist
sidebar_position: 10
---

# Release checklist

Use this checklist for each package release.

## Prepare

1. Update `NodelyMainVersion` or the affected side-package version property in `Directory.Build.props`.
2. Update `CHANGELOG.md`, `README.md`, package target tables, and any docs that mention the current version
   or test count.
3. Run the wording scan before staging so branch names, commit messages, docs, comments, and generated files stay neutral.

## Validate locally

```powershell
dotnet build Nodely.slnx --configuration Release
dotnet test Nodely.slnx --configuration Release --no-build --verbosity normal
dotnet pack Nodely.slnx --configuration Release --no-build --output artifacts/packages

npm --prefix website ci --ignore-scripts --dry-run
npm --prefix website run build
```

Inspect `artifacts/packages` for all `.nupkg` files and matching `.snupkg` symbol packages:

- `Nodely.Core`
- `Nodely.Avalonia`
- `Nodely.Avalonia.Database`
- `Nodely.Avalonia.MindMap`
- `Nodely.Avalonia.Uml`
- `Nodely.Avalonia.Workflow`
- `Nodely.Algorithms`
- `Nodely.Serialization`

For compatibility releases, also inspect package contents:

- `Nodely.Avalonia`: `lib/net8.0` and `lib/net10.0`
- Side packages such as `Nodely.Avalonia.Database`, `Nodely.Avalonia.MindMap`,
  `Nodely.Avalonia.Uml`, and `Nodely.Avalonia.Workflow`: `lib/net8.0` and `lib/net10.0`
- `Nodely.Core`, `Nodely.Algorithms`, `Nodely.Serialization`: `lib/netstandard2.0`, `lib/net8.0`, and `lib/net10.0`

## Publish

1. Merge the release PR to `main`.
2. Confirm `CI`, `Package`, and `Docs` are green on `main`.
3. Create and push a tag:
   - `vX.Y.Z` for the main package group.
   - `Nodely.Avalonia.PackageName/vX.Y.Z` for one side package, for example
     `Nodely.Avalonia.MindMap/v0.1.0` or `Nodely.Avalonia.Workflow/v0.1.0`.
4. Confirm the `Package` workflow publishes only the selected package set from the tag using the
   `NUGET_API_KEY` repository secret.
5. Create a GitHub release from the changelog entry.

## Verify

1. Confirm NuGet lists the new version for every package in the release set.
2. Confirm the docs site is live at `https://araxis.github.io/nodely/`.
3. Confirm `README.md` badges resolve.
4. Keep the working tree clean after pruning merged release branches.
