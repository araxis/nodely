using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Api;

/// <summary>An API contract or schema node.</summary>
public sealed class ApiContractNode : ApiNodeBase
{
    public new const string ModelKindKey = "api.contract";

    public ApiContractNode(Point position, string name = "Contract") : base(position, name) { }

    public ApiContractNode(string id, Point position, string name = "Contract") : base(id, position, name) { }

    /// <summary>The contract fields.</summary>
    public ObservableCollection<ApiContractField> Fields { get; } = new();

    public override string ModelKind => ModelKindKey;

    protected override string DefaultName => "Contract";

    protected override string DefaultAccentColor => "#D18B30";

    protected override string DefaultIconKey => "DTO";

    public override NodeModel Clone()
    {
        var clone = new ApiContractNode(Position, Name);
        CopyBaseTo(clone);
        foreach (var field in Fields)
            clone.Fields.Add(field.Clone());
        return clone;
    }

    public override IReadOnlyDictionary<string, object?> GetExtraData()
    {
        var extra = BuildBaseExtra();
        extra["FieldsJson"] = JsonSerializer.Serialize(Fields);
        return extra;
    }

    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        ApplyBaseExtra(data);
        Fields.Clear();
        foreach (var field in DeserializeList<ApiContractField>(data, "FieldsJson"))
            Fields.Add(field);
    }

    internal static IReadOnlyList<T> DeserializeList<T>(IReadOnlyDictionary<string, object?> data, string key)
    {
        if (!data.TryGetValue(key, out var value) || value is not string json || string.IsNullOrWhiteSpace(json))
            return new List<T>();

        return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
    }
}
