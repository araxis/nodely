using Nodely.Geometry;

namespace Nodely.Models.Base;

/// <summary>A model that can report its bounding rectangle in diagram space.</summary>
public interface IHasBounds
{
    /// <summary>Returns the model's bounds, or null if not yet measured.</summary>
    Rectangle? GetBounds();
}
