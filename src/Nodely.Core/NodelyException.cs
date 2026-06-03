using System;

namespace Nodely;

/// <summary>An error raised by the Nodely engine.</summary>
public class NodelyException : Exception
{
    /// <summary>Creates an exception with a message.</summary>
    public NodelyException(string message) : base(message) { }

    /// <summary>Creates an exception with a message and inner exception.</summary>
    public NodelyException(string message, Exception innerException) : base(message, innerException) { }
}
