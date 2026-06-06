using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nodely.Avalonia.Designer;

/// <summary>A group of toolbox items.</summary>
public sealed class DesignerToolboxSection
{
    /// <summary>Creates a toolbox section.</summary>
    public DesignerToolboxSection(string title, IEnumerable<DesignerToolboxItem>? items = null)
    {
        Title = title;
        Items = items == null ? new ObservableCollection<DesignerToolboxItem>() : new ObservableCollection<DesignerToolboxItem>(items);
    }

    /// <summary>Section title.</summary>
    public string Title { get; }

    /// <summary>Items in this section.</summary>
    public ObservableCollection<DesignerToolboxItem> Items { get; }
}
