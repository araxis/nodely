using System;
using System.Collections.Generic;
using System.Linq;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.Models.Base;

namespace Nodely.Avalonia.Designer;

/// <summary>Explicit registry of editable runtime properties.</summary>
public sealed class DiagramPropertyRegistry
{
    private readonly Dictionary<Type, List<DiagramPropertyDescriptor>> _descriptors = new();

    /// <summary>Creates an empty registry.</summary>
    public DiagramPropertyRegistry()
    {
    }

    /// <summary>Creates a registry with common model, node, and link fields.</summary>
    public static DiagramPropertyRegistry CreateDefault() => new DiagramPropertyRegistry()
        .Register<SelectableModel>(
            DiagramProperty.Boolean<SelectableModel>("Locked", model => model.Locked, (model, value) => model.Locked = value, "Common"))
        .Register<NodeModel>(
            DiagramProperty.Text<NodeModel>("Title", model => model.Title ?? string.Empty, (model, value) => model.Title = value ?? string.Empty, "Node"),
            DiagramProperty.Number<NodeModel>("X", model => model.Position.X, (model, value) => model.SetPosition(value, model.Position.Y), "Position"),
            DiagramProperty.Number<NodeModel>("Y", model => model.Position.Y, (model, value) => model.SetPosition(model.Position.X, value), "Position"))
        .Register<BaseLinkModel>(
            DiagramProperty.Boolean<BaseLinkModel>("Segmentable", model => model.Segmentable, (model, value) => model.Segmentable = value, "Link"),
            DiagramProperty.Text<BaseLinkModel>("Label", FirstLabel, SetFirstLabel, "Link"))
        .Register<LinkModel>(
            DiagramProperty.Number<LinkModel>("Width", model => model.Width, (model, value) => model.Width = Math.Max(0.5, value), "Link"),
            DiagramProperty.Color<LinkModel>("Color", model => model.Color ?? string.Empty, (model, value) => model.Color = NormalizeOptional(value), "Link"));

    /// <summary>Registers descriptors for a model type.</summary>
    public DiagramPropertyRegistry Register<TModel>(params DiagramPropertyDescriptor<TModel>[] descriptors)
        where TModel : Model
    {
        if (descriptors is null)
            throw new ArgumentNullException(nameof(descriptors));

        var type = typeof(TModel);
        if (!_descriptors.TryGetValue(type, out var list))
        {
            list = new List<DiagramPropertyDescriptor>();
            _descriptors[type] = list;
        }

        list.AddRange(descriptors.Select(descriptor => descriptor.Untyped));
        return this;
    }

    /// <summary>Returns descriptors that apply to the model, from base type to most-specific type.</summary>
    public IReadOnlyList<DiagramPropertyDescriptor> GetDescriptors(Model model)
    {
        if (model is null)
            throw new ArgumentNullException(nameof(model));

        var types = new Stack<Type>();
        for (var type = model.GetType(); type != null && typeof(Model).IsAssignableFrom(type); type = type.BaseType)
            types.Push(type);

        var result = new List<DiagramPropertyDescriptor>();
        while (types.Count > 0)
            if (_descriptors.TryGetValue(types.Pop(), out var descriptors))
                result.AddRange(descriptors);

        return result;
    }

    private static string FirstLabel(BaseLinkModel link) => link.Labels.FirstOrDefault()?.Content ?? string.Empty;

    private static void SetFirstLabel(BaseLinkModel link, string? text)
    {
        var value = NormalizeOptional(text);
        if (value == null)
        {
            link.Labels.Clear();
            link.Refresh();
            return;
        }

        if (link.Labels.Count == 0)
            link.AddLabel(value, 0.5, new Point(0, -16));
        else
            link.Labels[0].Content = value;

        link.Refresh();
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
