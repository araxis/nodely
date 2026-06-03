namespace Nodely.Events;

/// <summary>
/// A framework-neutral mouse-wheel / scroll event. <see cref="X"/>/<see cref="Y"/> are in pixels relative
/// to the diagram container; deltas are in the platform's wheel units (the zoom behavior only uses the sign
/// and relative magnitude).
/// </summary>
public record WheelEvent(
    double X,
    double Y,
    double DeltaX,
    double DeltaY,
    double DeltaZ,
    bool CtrlKey,
    bool ShiftKey,
    bool AltKey,
    bool MetaKey);
