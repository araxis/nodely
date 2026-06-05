using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Workflow;

/// <summary>A workflow note node.</summary>
public sealed class WorkflowNoteNode : NodeModel
{
    /// <summary>The stable serialization kind for workflow note nodes.</summary>
    public new const string ModelKindKey = "workflow.note";

    /// <summary>Creates a note node.</summary>
    public WorkflowNoteNode(Point position, string text = "Note") : base(position)
    {
        Text = string.IsNullOrWhiteSpace(text) ? "Note" : text.Trim();
        Title = "Note";
    }

    /// <summary>Creates a note node with the given id.</summary>
    public WorkflowNoteNode(string id, Point position, string text = "Note") : base(id, position)
    {
        Text = string.IsNullOrWhiteSpace(text) ? "Note" : text.Trim();
        Title = "Note";
    }

    /// <summary>The note text.</summary>
    public string Text { get; set; }

    /// <inheritdoc />
    public override string ModelKind => ModelKindKey;

    /// <inheritdoc />
    public override NodeModel Clone() => new WorkflowNoteNode(Position, Text)
    {
        Size = Size,
        ControlledSize = ControlledSize,
    };

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, object?> GetExtraData() => new Dictionary<string, object?>
    {
        ["Text"] = Text,
    };

    /// <inheritdoc />
    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        if (data.TryGetValue("Text", out var text) && text is string textValue)
            Text = string.IsNullOrWhiteSpace(textValue) ? "Note" : textValue.Trim();
        Title = "Note";
    }
}
