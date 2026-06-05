namespace Nodely.Avalonia.StateMachine;

/// <summary>The semantic kind of a state-machine transition.</summary>
public enum StateMachineTransitionKind
{
    /// <summary>A normal transition between two states.</summary>
    Normal,

    /// <summary>A transition returning to the same state.</summary>
    Self,

    /// <summary>A transition that leaves a choice node.</summary>
    Choice,

    /// <summary>An error or exception transition.</summary>
    Error,

    /// <summary>A timeout transition.</summary>
    Timeout,
}
