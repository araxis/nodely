namespace Nodely.Avalonia.Api;

/// <summary>A field displayed by API contract nodes.</summary>
public sealed class ApiContractField
{
    /// <summary>Creates an empty field for serializers and object initializers.</summary>
    public ApiContractField() { }

    /// <summary>Creates a contract field.</summary>
    public ApiContractField(string name, string type, bool required = false)
    {
        Name = name;
        Type = type;
        Required = required;
    }

    /// <summary>The field name.</summary>
    public string Name { get; set; } = "";

    /// <summary>The field type.</summary>
    public string Type { get; set; } = "";

    /// <summary>Whether the field is required.</summary>
    public bool Required { get; set; }

    /// <summary>Optional field description.</summary>
    public string? Description { get; set; }

    /// <summary>Creates a detached copy.</summary>
    public ApiContractField Clone() => new(Name, Type, Required)
    {
        Description = Description,
    };
}
