namespace Nodely.Avalonia.Database;

/// <summary>The semantic meaning of a database relationship link.</summary>
public enum RelationshipKind
{
    /// <summary>A general association.</summary>
    Association,

    /// <summary>One source row relates to one target row.</summary>
    OneToOne,

    /// <summary>One source row relates to many target rows.</summary>
    OneToMany,

    /// <summary>Many source rows relate to many target rows.</summary>
    ManyToMany,

    /// <summary>A dependency, such as a view or procedure reading from another object.</summary>
    Dependency,
}
