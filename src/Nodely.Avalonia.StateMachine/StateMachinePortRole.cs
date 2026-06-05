namespace Nodely.Avalonia.StateMachine;

/// <summary>The role of a state-machine port.</summary>
public enum StateMachinePortRole
{
    /// <summary>A general transition port.</summary>
    Transition,

    /// <summary>An inbound transition port.</summary>
    Entry,

    /// <summary>An outbound transition port.</summary>
    Exit,
}
