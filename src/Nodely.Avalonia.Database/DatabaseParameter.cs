namespace Nodely.Avalonia.Database;

/// <summary>A stored procedure parameter displayed by procedure nodes.</summary>
public sealed class DatabaseParameter
{
    /// <summary>Creates an empty parameter for serializers and object initializers.</summary>
    public DatabaseParameter() { }

    /// <summary>Creates a parameter.</summary>
    public DatabaseParameter(string name, string dataType, string direction = "In")
    {
        Name = name;
        DataType = dataType;
        Direction = direction;
    }

    /// <summary>The parameter name.</summary>
    public string Name { get; set; } = "";

    /// <summary>The database type, e.g. <c>int</c> or <c>datetime2</c>.</summary>
    public string DataType { get; set; } = "";

    /// <summary>The direction, e.g. <c>In</c>, <c>Out</c>, or <c>InOut</c>.</summary>
    public string Direction { get; set; } = "In";

    /// <summary>Creates a detached copy.</summary>
    public DatabaseParameter Clone() => new(Name, DataType, Direction);
}
