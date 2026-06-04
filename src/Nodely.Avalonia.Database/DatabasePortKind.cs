namespace Nodely.Avalonia.Database;

/// <summary>The role of a database port.</summary>
public enum DatabasePortKind
{
    /// <summary>A generic object dependency.</summary>
    Dependency,

    /// <summary>A relationship between database objects.</summary>
    Relationship,

    /// <summary>A procedure input.</summary>
    Input,

    /// <summary>A procedure output.</summary>
    Output,
}
