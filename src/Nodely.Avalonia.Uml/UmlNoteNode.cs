using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Uml;

/// <summary>A UML note/comment node.</summary>
public sealed class UmlNoteNode : NodeModel
{
    /// <summary>The stable serialization kind for UML note nodes.</summary>
    public new const string ModelKindKey = "uml.note";

    /// <summary>Creates a note node.</summary>
    public UmlNoteNode(Point position, string text = "Note") : base(position)
    {
        Text = text;
        Title = "Note";
    }

    /// <summary>Creates a note node with the given id.</summary>
    public UmlNoteNode(string id, Point position, string text = "Note") : base(id, position)
    {
        Text = text;
        Title = "Note";
    }

    /// <summary>The note text.</summary>
    public string Text { get; set; }

    /// <inheritdoc />
    public override string ModelKind => ModelKindKey;

    /// <inheritdoc />
    public override NodeModel Clone() => new UmlNoteNode(Position, Text)
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
            Text = textValue;
        Title = "Note";
    }
}
