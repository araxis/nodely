namespace Nodely.Avalonia.Database;

/// <summary>A database column displayed by table and view nodes.</summary>
public sealed class DatabaseColumn
{
    /// <summary>Creates an empty column for serializers and object initializers.</summary>
    public DatabaseColumn() { }

    /// <summary>Creates a column.</summary>
    public DatabaseColumn(string name, string dataType, bool isPrimaryKey = false, bool isNullable = true)
    {
        Name = name;
        DataType = dataType;
        IsPrimaryKey = isPrimaryKey;
        IsNullable = isNullable;
    }

    /// <summary>The column name.</summary>
    public string Name { get; set; } = "";

    /// <summary>The database type, e.g. <c>int</c> or <c>nvarchar(120)</c>.</summary>
    public string DataType { get; set; } = "";

    /// <summary>Whether the column is part of the primary key.</summary>
    public bool IsPrimaryKey { get; set; }

    /// <summary>Whether the column participates in a foreign key.</summary>
    public bool IsForeignKey { get; set; }

    /// <summary>Whether the column allows null values.</summary>
    public bool IsNullable { get; set; } = true;

    /// <summary>Creates a detached copy.</summary>
    public DatabaseColumn Clone() => new(Name, DataType, IsPrimaryKey, IsNullable)
    {
        IsForeignKey = IsForeignKey,
    };
}
