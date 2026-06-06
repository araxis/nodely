---
title: Package composition
---

# Package composition

Nodely side packages are independent packages, but they are designed to share one canvas and one
serializer registry. Use this pattern when an editor needs more than one vocabulary, such as a system design
canvas that mixes API endpoints, database tables, network devices, and workflow steps.

## Choose packages

Start with the vocabulary your users recognize:

| Package | Use it for |
| --- | --- |
| `Nodely.Avalonia.Api` | Service maps, endpoints, contracts, clients, gateways, request/response/event links |
| `Nodely.Avalonia.Database` | Schemas, tables, views, procedures, rows, relationships, dependencies |
| `Nodely.Avalonia.Network` | Topology diagrams, devices, zones, status, protocol and capacity links |
| `Nodely.Avalonia.Workflow` | Process diagrams, tasks, decisions, events, sequence and message links |
| `Nodely.Avalonia.StateMachine` | Lifecycle diagrams, state transitions, guards, self loops |
| `Nodely.Avalonia.Uml` | Structural type diagrams, class/interface/enum/package relationships |
| `Nodely.Avalonia.MindMap` | Planning maps, root/branch/leaf topics, curved branch links |
| `Nodely.Avalonia.Designer` | Reusable editor shell, toolbox, command bar, inspector, navigator, status |

Install only the packages your app needs:

```powershell
dotnet add package Nodely.Avalonia.Designer
dotnet add package Nodely.Avalonia.Api
dotnet add package Nodely.Avalonia.Database
dotnet add package Nodely.Avalonia.Network
dotnet add package Nodely.Avalonia.Workflow
dotnet add package Nodely.Serialization
```

## Register canvas renderers

Register each package once on the `DiagramCanvas`. Registration is type-based, so packages do not overwrite
each other.

```csharp
using Nodely.Avalonia.Api;
using Nodely.Avalonia.Controls;
using Nodely.Avalonia.Database;
using Nodely.Avalonia.Network;
using Nodely.Avalonia.Workflow;

var canvas = new DiagramCanvas { Diagram = diagram };

canvas
    .UseApiNodes()
    .UseDatabaseNodes()
    .UseNetworkNodes()
    .UseWorkflowNodes();
```

Each call owns its own node, port, and typed link registrations. Registering a later package only affects the
model types from that package.

## Register serialization

Use one `DiagramSerializationRegistry` for save/load. A factory registry can be extended with more packages:

```csharp
using Nodely.Avalonia.Api;
using Nodely.Avalonia.Database;
using Nodely.Avalonia.Network;
using Nodely.Avalonia.Workflow;
using Nodely.Serialization;

var registry = ApiNodeFactory.CreateRegistry()
    .UseDatabaseNodes()
    .UseNetworkNodes()
    .UseWorkflowNodes();

var json = DiagramSerializer.Serialize(diagram);

var loaded = new NodelyDiagram();
DiagramSerializer.Deserialize(loaded, json, registry);
```

Registry order does not decide which package restores a model. Stable model-kind keys do that, so a mixed
diagram can round-trip without app-local factory branching.

## Mix vocabularies

The editor can combine semantic ports and typed links from different packages. Keep each domain relationship
typed when the relationship belongs to one package, and use the most descriptive link type for cross-domain
references.

```csharp
var endpoint = diagram.Nodes.Add(new ApiEndpointNode(new Point(0, 0), "/orders", ApiEndpointMethod.Post));
var table = diagram.Nodes.Add(new DatabaseTableNode(new Point(360, 0), "Orders", "sales"));
var service = diagram.Nodes.Add(new NetworkServiceNode(new Point(0, 220), "Orders API"));
var task = diagram.Nodes.Add(new WorkflowTaskNode(new Point(360, 220), "Persist order"));

diagram.Links.Add(new ApiLink(service, endpoint, ApiLinkKind.Request)
{
    Label = "hosts",
    Protocol = "HTTP",
});

diagram.Links.Add(new DatabaseRelationshipLink(endpoint, table, RelationshipKind.Dependency)
{
    Label = "writes",
});

diagram.Links.Add(new WorkflowLink(task, endpoint, WorkflowLinkKind.Message)
{
    Label = "invokes",
});
```

For a larger example, open the desktop gallery and choose the `Architecture` scene.

## Edit at runtime

Use `RunAsUndoableEdit()` for metadata changes in a side-panel inspector. The same pattern works for core and
side-package models:

```csharp
var previous = endpoint.Summary;

canvas.RunAsUndoableEdit(
    apply: () =>
    {
        endpoint.Summary = "Creates a customer order";
        endpoint.RefreshAll();
    },
    undo: () =>
    {
        endpoint.Summary = previous;
        endpoint.RefreshAll();
    });
```

Call `RefreshVisuals()` after layout or batch changes when the current visuals should be rebuilt immediately.

If you want that inspector as reusable UI instead of copied app-local controls, use
`Nodely.Avalonia.Designer`. Its `DiagramPropertyRegistry` composes with the same model types registered by
side packages, while the canvas registrations still come from each package's `UseXNodes()` method.

## Visual standard

A side package should feel like a domain vocabulary, not generic boxes and lines. When adding or choosing a
package, expect:

- distinct node shapes for the domain
- visible semantic ports
- typed link styling and link glyphs
- theme-aware renderers
- serializer registration for nodes, ports, links, and metadata
- focused tests proving composition with the other packages
