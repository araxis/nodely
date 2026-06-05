namespace Nodely.Avalonia.Workflow;

/// <summary>The relationship kind represented by a workflow link.</summary>
public enum WorkflowLinkKind
{
    /// <summary>A normal sequence flow.</summary>
    Sequence,

    /// <summary>A conditional branch.</summary>
    Conditional,

    /// <summary>An error path.</summary>
    Error,

    /// <summary>A message path.</summary>
    Message
}
