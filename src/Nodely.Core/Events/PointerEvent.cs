namespace Nodely.Events;

/// <summary>
/// A framework-neutral pointer event. <see cref="X"/>/<see cref="Y"/> are in pixels relative to the
/// diagram container. The Avalonia layer constructs these from Avalonia pointer events and feeds them to
/// the diagram's <c>Trigger*</c> input methods (Phase 2).
/// </summary>
public record PointerEvent(
    double X,
    double Y,
    PointerButton Button,
    bool CtrlKey,
    bool ShiftKey,
    bool AltKey,
    bool MetaKey,
    long PointerId = 0,
    bool IsPrimary = true);
