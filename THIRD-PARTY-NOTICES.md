# Third-Party Notices

Nodely is built with and/or derives design knowledge from the following open-source projects.

## Blazor.Diagrams (Z.Blazor.Diagrams) — MIT

- Repository: https://github.com/Blazor-Diagrams/Blazor.Diagrams
- License: MIT

`Nodely.Core` is a first-party re-implementation (port/transliteration) of the architecture and
algorithms of `Blazor.Diagrams.Core` (models, layers, behaviors, geometry, routers, path generators,
anchors). Where code is adapted closely from the original, attribution is retained per the MIT license.
Nodely does not depend on the Blazor.Diagrams packages at runtime. See
`memory/01-decisions/ADR-0002-core-strategy.md` for the porting decision.

```
MIT License — Copyright (c) Blazor.Diagrams authors
(full text: https://github.com/Blazor-Diagrams/Blazor.Diagrams/blob/master/LICENSE)
```

## Avalonia — MIT

- Repository: https://github.com/AvaloniaUI/Avalonia
- License: MIT

Nodely's UI layer (`Nodely.Avalonia`) and the demo app depend on the Avalonia UI framework.

## Other NuGet dependencies

- CommunityToolkit.Mvvm (MIT) — sample/demo app only.
- xUnit, Shouldly, Microsoft.NET.Test.Sdk — test projects only.
