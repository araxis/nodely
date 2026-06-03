namespace Nodely.Events;

/// <summary>
/// A framework-neutral keyboard event. <see cref="Key"/> is the produced value (e.g. "a", "Delete");
/// <see cref="Code"/> is the physical key (e.g. "KeyA"). The Avalonia layer maps Avalonia key events to this.
/// </summary>
public record KeyboardEvent(
    string Key,
    string Code,
    bool CtrlKey,
    bool ShiftKey,
    bool AltKey,
    bool MetaKey);
