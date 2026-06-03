using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nodely.Events;
using Nodely.Utils;

namespace Nodely.Behaviors;

/// <summary>Maps keyboard shortcuts to actions. Defaults: Delete = delete selection, Ctrl+Alt+g = group.</summary>
public class KeyboardShortcutsBehavior : Behavior
{
    private readonly Dictionary<string, Func<Diagram, ValueTask>> _shortcuts = new();

    /// <summary>Creates and wires the behavior with the default shortcuts.</summary>
    public KeyboardShortcutsBehavior(Diagram diagram) : base(diagram)
    {
        SetShortcut("Delete", ctrl: false, shift: false, alt: false, KeyboardShortcutsDefaults.DeleteSelection);
        SetShortcut("g", ctrl: true, shift: false, alt: true, KeyboardShortcutsDefaults.Grouping);

        Diagram.KeyDown += OnDiagramKeyDown;
    }

    /// <summary>Registers (or replaces) a shortcut.</summary>
    public void SetShortcut(string key, bool ctrl, bool shift, bool alt, Func<Diagram, ValueTask> action)
    {
        var k = KeysUtils.GetStringRepresentation(ctrl, shift, alt, key);
        _shortcuts[k] = action;
    }

    /// <summary>Removes a shortcut.</summary>
    public bool RemoveShortcut(string key, bool ctrl, bool shift, bool alt)
    {
        var k = KeysUtils.GetStringRepresentation(ctrl, shift, alt, key);
        return _shortcuts.Remove(k);
    }

    private async void OnDiagramKeyDown(KeyboardEvent e)
    {
        var k = KeysUtils.GetStringRepresentation(e.CtrlKey, e.ShiftKey, e.AltKey, e.Key);
        if (_shortcuts.TryGetValue(k, out var action))
            await action(Diagram);
    }

    /// <inheritdoc />
    public override void Dispose() => Diagram.KeyDown -= OnDiagramKeyDown;
}
