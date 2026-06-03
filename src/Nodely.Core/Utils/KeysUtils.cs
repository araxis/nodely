using System.Text;

namespace Nodely.Utils;

/// <summary>Helpers for keyboard shortcut keys.</summary>
public static class KeysUtils
{
    /// <summary>Builds a canonical string like "Ctrl+Shift+a" for a modifier+key combination.</summary>
    public static string GetStringRepresentation(bool ctrl, bool shift, bool alt, string key)
    {
        var sb = new StringBuilder();

        if (ctrl) sb.Append("Ctrl+");
        if (shift) sb.Append("Shift+");
        if (alt) sb.Append("Alt+");
        sb.Append(key);

        return sb.ToString();
    }
}
