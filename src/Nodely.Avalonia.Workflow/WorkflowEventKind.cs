namespace Nodely.Avalonia.Workflow;

/// <summary>The event category represented by a workflow event node.</summary>
public enum WorkflowEventKind
{
    /// <summary>A generic workflow event.</summary>
    Event,

    /// <summary>A timer event.</summary>
    Timer,

    /// <summary>A message event.</summary>
    Message,

    /// <summary>A signal event.</summary>
    Signal,

    /// <summary>An error event.</summary>
    Error
}
