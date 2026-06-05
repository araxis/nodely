namespace Nodely.Avalonia.Uml;

/// <summary>The semantic meaning of a UML relationship link.</summary>
public enum UmlRelationshipKind
{
    /// <summary>A plain association.</summary>
    Association,

    /// <summary>A generalization/inheritance relationship.</summary>
    Inheritance,

    /// <summary>An interface realization relationship.</summary>
    Realization,

    /// <summary>A dependency relationship.</summary>
    Dependency,

    /// <summary>A shared aggregation relationship.</summary>
    Aggregation,

    /// <summary>A composition relationship.</summary>
    Composition
}
