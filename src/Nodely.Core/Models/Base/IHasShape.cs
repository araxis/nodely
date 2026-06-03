using Nodely.Geometry;

namespace Nodely.Models.Base;

/// <summary>A model that can report the geometric shape links attach to.</summary>
public interface IHasShape
{
    /// <summary>Returns the model's shape in diagram space.</summary>
    IShape GetShape();
}
