using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Api;

/// <summary>An internal operation behind one or more endpoints.</summary>
public sealed class ApiOperationNode : ApiNodeBase
{
    public new const string ModelKindKey = "api.operation";

    private string? _input;
    private string? _output;
    private bool _sideEffectFree;

    public ApiOperationNode(Point position, string name = "Operation") : base(position, name) { }

    public ApiOperationNode(string id, Point position, string name = "Operation") : base(id, position, name) { }

    /// <summary>Optional input contract name.</summary>
    public string? Input
    {
        get => _input;
        set
        {
            _input = NormalizeOptional(value);
            Refresh();
        }
    }

    /// <summary>Optional output contract name.</summary>
    public string? Output
    {
        get => _output;
        set
        {
            _output = NormalizeOptional(value);
            Refresh();
        }
    }

    /// <summary>Whether the operation avoids state changes.</summary>
    public bool SideEffectFree
    {
        get => _sideEffectFree;
        set
        {
            _sideEffectFree = value;
            Refresh();
        }
    }

    public override string ModelKind => ModelKindKey;

    protected override string DefaultName => "Operation";

    protected override string DefaultAccentColor => "#8B68B8";

    protected override string DefaultIconKey => "OP";

    public override NodeModel Clone()
    {
        var clone = new ApiOperationNode(Position, Name)
        {
            Input = Input,
            Output = Output,
            SideEffectFree = SideEffectFree,
        };
        CopyBaseTo(clone);
        return clone;
    }

    public override IReadOnlyDictionary<string, object?> GetExtraData()
    {
        var extra = BuildBaseExtra();
        extra["Input"] = Input;
        extra["Output"] = Output;
        extra["SideEffectFree"] = SideEffectFree;
        return extra;
    }

    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        ApplyBaseExtra(data);
        if (data.TryGetValue("Input", out var input) && input is string inputText)
            _input = NormalizeOptional(inputText);
        if (data.TryGetValue("Output", out var output) && output is string outputText)
            _output = NormalizeOptional(outputText);
        SideEffectFree = data.TryGetValue("SideEffectFree", out var sef) && sef is bool boolValue && boolValue;
    }
}
