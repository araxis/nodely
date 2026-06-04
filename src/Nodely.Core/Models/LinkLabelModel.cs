using Nodely.Geometry;
using Nodely.Models.Base;

namespace Nodely.Models;

/// <summary>A piece of content placed along a link.</summary>
public class LinkLabelModel : Model
{
    private string _content;
    private double? _distance;
    private Point? _offset;

    /// <summary>Creates a label with the given id.</summary>
    public LinkLabelModel(BaseLinkModel parent, string id, string content, double? distance = null, Point? offset = null)
        : base(id)
    {
        Parent = parent;
        _content = content;
        _distance = distance;
        _offset = offset;
    }

    /// <summary>Creates a label.</summary>
    public LinkLabelModel(BaseLinkModel parent, string content, double? distance = null, Point? offset = null)
    {
        Parent = parent;
        _content = content;
        _distance = distance;
        _offset = offset;
    }

    /// <summary>The link this label belongs to.</summary>
    public BaseLinkModel Parent { get; }

    /// <summary>The label text/content.</summary>
    public string Content
    {
        get => _content;
        set
        {
            if (_content == value)
                return;

            _content = value;
            Refresh();
        }
    }

    /// <summary>
    /// Where along the link to place the label:
    /// a value in [0, 1] is relative to the link length; a value &gt; 1 is a distance from the start;
    /// a value &lt; 0 is a distance from the end.
    /// </summary>
    public double? Distance
    {
        get => _distance;
        set
        {
            if (_distance == value)
                return;

            _distance = value;
            Refresh();
        }
    }

    /// <summary>An additional pixel offset from the computed position.</summary>
    public Point? Offset
    {
        get => _offset;
        set
        {
            if (_offset?.Equals(value) == true || (_offset == null && value == null))
                return;

            _offset = value;
            Refresh();
        }
    }

    /// <inheritdoc />
    public override void Refresh()
    {
        base.Refresh();
        Parent.Refresh();
    }
}
