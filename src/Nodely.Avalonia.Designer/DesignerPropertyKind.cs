namespace Nodely.Avalonia.Designer;

/// <summary>The built-in editor kinds supported by <see cref="DiagramPropertyInspector"/>.</summary>
public enum DesignerPropertyKind
{
    /// <summary>Single-line text.</summary>
    Text,

    /// <summary>Multi-line text.</summary>
    MultilineText,

    /// <summary>Invariant-culture number.</summary>
    Number,

    /// <summary>Boolean checkbox.</summary>
    Boolean,

    /// <summary>Enum or fixed option list.</summary>
    Enum,

    /// <summary>Text with a color swatch.</summary>
    Color,

    /// <summary>Add/remove rows for a mutable collection.</summary>
    Collection,
}
