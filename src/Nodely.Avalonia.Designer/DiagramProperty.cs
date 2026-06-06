using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Nodely.Models.Base;

namespace Nodely.Avalonia.Designer;

/// <summary>Factory helpers for explicit runtime property descriptors.</summary>
public static class DiagramProperty
{
    /// <summary>Creates a text property.</summary>
    public static DiagramPropertyDescriptor<TModel> Text<TModel>(
        string label,
        Func<TModel, string?> get,
        Action<TModel, string?> set,
        string section = "Properties",
        bool multiline = false)
        where TModel : Model
        => new(
            section,
            label,
            multiline ? DesignerPropertyKind.MultilineText : DesignerPropertyKind.Text,
            model => get(model) ?? string.Empty,
            (model, value) => set(model, value?.ToString()));

    /// <summary>Creates a color text property with a small swatch.</summary>
    public static DiagramPropertyDescriptor<TModel> Color<TModel>(
        string label,
        Func<TModel, string?> get,
        Action<TModel, string?> set,
        string section = "Properties")
        where TModel : Model
        => new(
            section,
            label,
            DesignerPropertyKind.Color,
            model => get(model) ?? string.Empty,
            (model, value) => set(model, value?.ToString()));

    /// <summary>Creates a numeric property.</summary>
    public static DiagramPropertyDescriptor<TModel> Number<TModel>(
        string label,
        Func<TModel, double> get,
        Action<TModel, double> set,
        string section = "Properties",
        double? minimum = null)
        where TModel : Model
        => new(
            section,
            label,
            DesignerPropertyKind.Number,
            model => get(model),
            (model, value) => set(model, Convert.ToDouble(value, CultureInfo.InvariantCulture)),
            minimum: minimum);

    /// <summary>Creates a boolean property.</summary>
    public static DiagramPropertyDescriptor<TModel> Boolean<TModel>(
        string label,
        Func<TModel, bool> get,
        Action<TModel, bool> set,
        string section = "Properties")
        where TModel : Model
        => new(
            section,
            label,
            DesignerPropertyKind.Boolean,
            model => get(model),
            (model, value) => set(model, value is bool b && b));

    /// <summary>Creates an enum property.</summary>
    public static DiagramPropertyDescriptor<TModel> Enum<TModel, TEnum>(
        string label,
        Func<TModel, TEnum> get,
        Action<TModel, TEnum> set,
        string section = "Properties")
        where TModel : Model
        where TEnum : struct, Enum
        => new(
            section,
            label,
            DesignerPropertyKind.Enum,
            model => get(model),
            (model, value) =>
            {
                if (value is TEnum typed)
                    set(model, typed);
                else if (value is string text && System.Enum.TryParse<TEnum>(text, out var parsed))
                    set(model, parsed);
            },
            options: System.Enum.GetValues<TEnum>().Cast<object>().ToArray());

    /// <summary>Creates an add/remove collection property.</summary>
    public static DiagramPropertyDescriptor<TModel> Collection<TModel, TItem>(
        string label,
        Func<TModel, IList<TItem>> getCollection,
        Func<TModel, TItem> createItem,
        Func<TItem, string> formatItem,
        string section = "Properties",
        string? addLabel = null)
        where TModel : Model
    {
        if (getCollection is null)
            throw new ArgumentNullException(nameof(getCollection));
        if (createItem is null)
            throw new ArgumentNullException(nameof(createItem));
        if (formatItem is null)
            throw new ArgumentNullException(nameof(formatItem));

        return new DiagramPropertyDescriptor<TModel>(
            section,
            label,
            DesignerPropertyKind.Collection,
            _ => null,
            (_, _) => { },
            getCollectionItems: model => getCollection(model)
                .Select(item => new DiagramCollectionItem(item!, formatItem(item)))
                .ToArray(),
            createCollectionItem: model => createItem(model)!,
            addCollectionItem: (model, item) => getCollection(model).Add((TItem)item),
            insertCollectionItem: (model, index, item) =>
            {
                var collection = getCollection(model);
                collection.Insert(Math.Clamp(index, 0, collection.Count), (TItem)item);
            },
            removeCollectionItem: (model, item) => getCollection(model).Remove((TItem)item),
            addLabel: addLabel ?? "Add");
    }
}
