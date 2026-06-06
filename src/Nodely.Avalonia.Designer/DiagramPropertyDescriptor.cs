using System;
using System.Collections.Generic;
using Nodely.Models.Base;

namespace Nodely.Avalonia.Designer;

/// <summary>Untyped descriptor consumed by the runtime property inspector.</summary>
public sealed class DiagramPropertyDescriptor
{
    internal DiagramPropertyDescriptor(
        string section,
        string label,
        DesignerPropertyKind kind,
        Func<Model, object?> getValue,
        Action<Model, object?> setValue,
        IReadOnlyList<object>? options = null,
        double? minimum = null,
        Func<Model, IReadOnlyList<DiagramCollectionItem>>? getCollectionItems = null,
        Func<Model, object>? createCollectionItem = null,
        Action<Model, object>? addCollectionItem = null,
        Action<Model, int, object>? insertCollectionItem = null,
        Action<Model, object>? removeCollectionItem = null,
        string? addLabel = null)
    {
        Section = string.IsNullOrWhiteSpace(section) ? "Properties" : section;
        Label = label;
        Kind = kind;
        GetValue = getValue;
        SetValue = setValue;
        Options = options;
        Minimum = minimum;
        GetCollectionItems = getCollectionItems;
        CreateCollectionItem = createCollectionItem;
        AddCollectionItem = addCollectionItem;
        InsertCollectionItem = insertCollectionItem;
        RemoveCollectionItem = removeCollectionItem;
        AddLabel = string.IsNullOrWhiteSpace(addLabel) ? "Add" : addLabel;
    }

    /// <summary>The section heading.</summary>
    public string Section { get; }

    /// <summary>The field label.</summary>
    public string Label { get; }

    /// <summary>The editor kind.</summary>
    public DesignerPropertyKind Kind { get; }

    /// <summary>Reads the current value from a model.</summary>
    public Func<Model, object?> GetValue { get; }

    /// <summary>Writes a value to a model.</summary>
    public Action<Model, object?> SetValue { get; }

    /// <summary>Options for enum/list fields.</summary>
    public IReadOnlyList<object>? Options { get; }

    /// <summary>Optional minimum value for numeric fields.</summary>
    public double? Minimum { get; }

    /// <summary>Reads collection rows.</summary>
    public Func<Model, IReadOnlyList<DiagramCollectionItem>>? GetCollectionItems { get; }

    /// <summary>Creates a new collection item.</summary>
    public Func<Model, object>? CreateCollectionItem { get; }

    /// <summary>Adds a collection item.</summary>
    public Action<Model, object>? AddCollectionItem { get; }

    /// <summary>Inserts a collection item at an index.</summary>
    public Action<Model, int, object>? InsertCollectionItem { get; }

    /// <summary>Removes a collection item.</summary>
    public Action<Model, object>? RemoveCollectionItem { get; }

    /// <summary>The add button label for collection fields.</summary>
    public string AddLabel { get; }
}

/// <summary>Typed descriptor used by apps when registering editable fields.</summary>
public sealed class DiagramPropertyDescriptor<TModel>
    where TModel : Model
{
    internal DiagramPropertyDescriptor(
        string section,
        string label,
        DesignerPropertyKind kind,
        Func<TModel, object?> getValue,
        Action<TModel, object?> setValue,
        IReadOnlyList<object>? options = null,
        double? minimum = null,
        Func<TModel, IReadOnlyList<DiagramCollectionItem>>? getCollectionItems = null,
        Func<TModel, object>? createCollectionItem = null,
        Action<TModel, object>? addCollectionItem = null,
        Action<TModel, int, object>? insertCollectionItem = null,
        Action<TModel, object>? removeCollectionItem = null,
        string? addLabel = null)
    {
        Untyped = new DiagramPropertyDescriptor(
            section,
            label,
            kind,
            model => getValue((TModel)model),
            (model, value) => setValue((TModel)model, value),
            options,
            minimum,
            getCollectionItems == null ? null : model => getCollectionItems((TModel)model),
            createCollectionItem == null ? null : model => createCollectionItem((TModel)model),
            addCollectionItem == null ? null : (model, item) => addCollectionItem((TModel)model, item),
            insertCollectionItem == null ? null : (model, index, item) => insertCollectionItem((TModel)model, index, item),
            removeCollectionItem == null ? null : (model, item) => removeCollectionItem((TModel)model, item),
            addLabel);
    }

    internal DiagramPropertyDescriptor Untyped { get; }
}
