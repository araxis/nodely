namespace Nodely.Avalonia.Uml;

/// <summary>A UML field/property member.</summary>
public sealed class UmlMember
{
    /// <summary>Creates an empty member for serializers and object initializers.</summary>
    public UmlMember() { }

    /// <summary>Creates a member.</summary>
    public UmlMember(string name, string type, UmlVisibility visibility = UmlVisibility.Public)
    {
        Name = name;
        Type = type;
        Visibility = visibility;
    }

    /// <summary>The member name.</summary>
    public string Name { get; set; } = "";

    /// <summary>The member type.</summary>
    public string Type { get; set; } = "";

    /// <summary>The member visibility.</summary>
    public UmlVisibility Visibility { get; set; } = UmlVisibility.Public;

    /// <summary>Whether the member is static.</summary>
    public bool IsStatic { get; set; }

    /// <summary>Whether the member is abstract.</summary>
    public bool IsAbstract { get; set; }

    /// <summary>Creates a detached copy.</summary>
    public UmlMember Clone() => new(Name, Type, Visibility)
    {
        IsStatic = IsStatic,
        IsAbstract = IsAbstract,
    };
}
