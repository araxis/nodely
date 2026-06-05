using System;
using System.Collections.Generic;
using Nodely.Anchors;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Workflow;

/// <summary>A workflow link connecting workflow nodes or ports.</summary>
public sealed class WorkflowLink : LinkModel
{
    /// <summary>The stable serialization kind for workflow links.</summary>
    public new const string ModelKindKey = "workflow.link";

    private WorkflowLinkKind _kind;
    private string? _label;
    private string? _condition;

    /// <summary>Creates a workflow link between two nodes.</summary>
    public WorkflowLink(NodeModel sourceNode, NodeModel targetNode, WorkflowLinkKind kind = WorkflowLinkKind.Sequence)
        : base(sourceNode, targetNode)
    {
        Kind = kind;
    }

    /// <summary>Creates a workflow link between two ports.</summary>
    public WorkflowLink(PortModel sourcePort, PortModel targetPort, WorkflowLinkKind kind = WorkflowLinkKind.Sequence)
        : base(sourcePort, targetPort)
    {
        Kind = kind;
    }

    /// <summary>Creates a workflow link with the given id between two anchors.</summary>
    public WorkflowLink(string id, Anchor source, Anchor target, WorkflowLinkKind kind = WorkflowLinkKind.Sequence)
        : base(id, source, target)
    {
        Kind = kind;
    }

    /// <summary>The workflow link kind.</summary>
    public WorkflowLinkKind Kind
    {
        get => _kind;
        set
        {
            _kind = value;
            ApplyKindDefaults();
            Refresh();
        }
    }

    /// <summary>Optional label placed near the middle of the link.</summary>
    public string? Label
    {
        get => _label;
        set
        {
            _label = NormalizeOptional(value);
            SyncLabels();
            Refresh();
        }
    }

    /// <summary>Optional condition text for conditional paths.</summary>
    public string? Condition
    {
        get => _condition;
        set
        {
            _condition = NormalizeOptional(value);
            SyncLabels();
            Refresh();
        }
    }

    /// <inheritdoc />
    public override string ModelKind => ModelKindKey;

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, object?> GetExtraData()
    {
        var extra = new Dictionary<string, object?> { ["WorkflowLinkKind"] = Kind.ToString() };
        if (!string.IsNullOrWhiteSpace(Label))
            extra["Label"] = Label;
        if (!string.IsNullOrWhiteSpace(Condition))
            extra["Condition"] = Condition;
        return extra;
    }

    /// <inheritdoc />
    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        if (data.TryGetValue("WorkflowLinkKind", out var kind) &&
            kind is string kindText &&
            Enum.TryParse<WorkflowLinkKind>(kindText, out var parsedKind))
        {
            Kind = parsedKind;
        }

        if (data.TryGetValue("Label", out var label) && label is string labelText)
            Label = labelText;
        if (data.TryGetValue("Condition", out var condition) && condition is string conditionText)
            Condition = conditionText;
    }

    private void ApplyKindDefaults()
    {
        Segmentable = true;
        TargetMarker = LinkMarker.Arrow;
        Width = Kind switch
        {
            WorkflowLinkKind.Error => 2.4,
            WorkflowLinkKind.Message => 2.1,
            _ => 2.2,
        };
    }

    private void SyncLabels()
    {
        Labels.Clear();

        if (!string.IsNullOrWhiteSpace(_label))
            AddLabel(_label, 0.5, new Point(0, -16));
        if (!string.IsNullOrWhiteSpace(_condition))
            AddLabel(_condition, 0.5, new Point(0, 16));
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
