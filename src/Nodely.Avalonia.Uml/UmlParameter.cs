namespace Nodely.Avalonia.Uml;

/// <summary>A UML operation parameter.</summary>
public sealed class UmlParameter
{
    /// <summary>Creates an empty parameter for serializers and object initializers.</summary>
    public UmlParameter() { }

    /// <summary>Creates a parameter.</summary>
    public UmlParameter(string name, string type, string? defaultValue = null)
    {
        Name = name;
        Type = type;
        DefaultValue = defaultValue;
    }

    /// <summary>The parameter name.</summary>
    public string Name { get; set; } = "";

    /// <summary>The parameter type.</summary>
    public string Type { get; set; } = "";

    /// <summary>An optional default value.</summary>
    public string? DefaultValue { get; set; }

    /// <summary>Creates a detached copy.</summary>
    public UmlParameter Clone() => new(Name, Type, DefaultValue);
}
