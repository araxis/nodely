namespace Nodely.Avalonia.Uml;

/// <summary>The semantic role of a UML connection port.</summary>
public enum UmlPortKind
{
    /// <summary>A general association endpoint.</summary>
    Association,

    /// <summary>An inheritance endpoint.</summary>
    Inheritance,

    /// <summary>A realization endpoint.</summary>
    Realization,

    /// <summary>A dependency endpoint.</summary>
    Dependency,

    /// <summary>An aggregation endpoint.</summary>
    Aggregation,

    /// <summary>A composition endpoint.</summary>
    Composition,

    /// <summary>A provided-interface endpoint.</summary>
    ProvidedInterface,

    /// <summary>A required-interface endpoint.</summary>
    RequiredInterface,
}
