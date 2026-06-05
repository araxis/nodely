namespace Nodely.Avalonia.Workflow;

/// <summary>The routing behavior represented by a workflow gateway node.</summary>
public enum WorkflowGatewayKind
{
    /// <summary>Exactly one branch is chosen.</summary>
    Exclusive,

    /// <summary>All branches are used.</summary>
    Parallel,

    /// <summary>One or more branches are used.</summary>
    Inclusive
}
