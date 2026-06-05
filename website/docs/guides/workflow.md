---
id: workflow
title: Workflow pack
sidebar_position: 11
---

# Workflow pack

`Nodely.Avalonia.Workflow` is an optional side package for workflow builder surfaces. It provides workflow
models and renderers, but it does not execute workflows, manage swimlanes, or impose a layout engine.

## Install

```bash
dotnet add package Nodely.Avalonia.Workflow
```

## Register renderers

Call `UseWorkflowNodes()` after creating the canvas:

```csharp
using Nodely.Avalonia.Controls;
using Nodely.Avalonia.Workflow;

var canvas = new DiagramCanvas { Diagram = diagram };
canvas.UseWorkflowNodes();
```

## Create workflow nodes

```csharp
using Nodely.Avalonia.Workflow;
using Point = Nodely.Geometry.Point;

var start = diagram.Nodes.Add(new WorkflowStartNode(new Point(120, 220), "Request received"));

var review = diagram.Nodes.Add(new WorkflowTaskNode(new Point(380, 160), "Review")
{
    TaskType = WorkflowTaskType.User,
    Status = WorkflowTaskStatus.Ready,
    Notes = "Assign an owner",
});

var decision = diagram.Nodes.Add(new WorkflowDecisionNode(new Point(660, 160), "Approved?")
{
    Condition = "amount <= limit",
});

var done = diagram.Nodes.Add(new WorkflowEndNode(new Point(920, 220), "Complete"));

diagram.Links.Add(new WorkflowLink(start, review, WorkflowLinkKind.Sequence)
{
    Label = "submit",
});

diagram.Links.Add(new WorkflowLink(review, decision, WorkflowLinkKind.Conditional)
{
    Label = "check",
    Condition = "valid",
});

diagram.Links.Add(new WorkflowLink(decision, done, WorkflowLinkKind.Sequence)
{
    Label = "finish",
});
```

The first release includes start, end, task, decision, gateway, event, and note nodes plus sequence,
conditional, error, and message links.

## Save and load

Register the Workflow serializer vocabulary when loading:

```csharp
using Nodely.Avalonia.Workflow;
using Nodely.Serialization;

var json = DiagramSerializer.Serialize(diagram);

var loaded = new NodelyDiagram();
DiagramSerializer.Deserialize(loaded, json, WorkflowNodeFactory.CreateRegistry());
```

The registry restores Workflow nodes, links, labels, conditions, task type/status, gateway kind, event kind,
and note text.
