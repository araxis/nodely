#if !NET5_0_OR_GREATER
// Enables C# 9 record / init-only setters on netstandard2.0 (which lacks this type).
namespace System.Runtime.CompilerServices
{
    using System.ComponentModel;

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit
    {
    }
}
#endif
