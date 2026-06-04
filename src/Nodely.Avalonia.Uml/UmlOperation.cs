using System.Collections.ObjectModel;

namespace Nodely.Avalonia.Uml;

/// <summary>A UML operation/method.</summary>
public sealed class UmlOperation
{
    /// <summary>Creates an empty operation for serializers and object initializers.</summary>
    public UmlOperation() { }

    /// <summary>Creates an operation.</summary>
    public UmlOperation(string name, string returnType = "void", UmlVisibility visibility = UmlVisibility.Public)
    {
        Name = name;
        ReturnType = returnType;
        Visibility = visibility;
    }

    /// <summary>The operation name.</summary>
    public string Name { get; set; } = "";

    /// <summary>The return type.</summary>
    public string ReturnType { get; set; } = "void";

    /// <summary>The operation visibility.</summary>
    public UmlVisibility Visibility { get; set; } = UmlVisibility.Public;

    /// <summary>Whether the operation is static.</summary>
    public bool IsStatic { get; set; }

    /// <summary>Whether the operation is abstract.</summary>
    public bool IsAbstract { get; set; }

    /// <summary>The operation parameters.</summary>
    public ObservableCollection<UmlParameter> Parameters { get; set; } = new();

    /// <summary>Creates a detached copy.</summary>
    public UmlOperation Clone()
    {
        var clone = new UmlOperation(Name, ReturnType, Visibility)
        {
            IsStatic = IsStatic,
            IsAbstract = IsAbstract,
        };
        foreach (var parameter in Parameters)
            clone.Parameters.Add(parameter.Clone());
        return clone;
    }
}
