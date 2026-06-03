using System;

namespace Nodely.Options;

/// <summary>Zoom configuration.</summary>
public class DiagramZoomOptions
{
    private double _minimum = 0.1;
    private double _scaleFactor = 1.05;

    /// <summary>Whether wheel zoom is enabled.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Whether to invert the wheel zoom direction.</summary>
    public bool Inverse { get; set; }

    /// <summary>The minimum allowed zoom (must be &gt; 0).</summary>
    public double Minimum
    {
        get => _minimum;
        set
        {
            if (value <= 0)
                throw new ArgumentException("Minimum can't be less than or equal to zero");

            _minimum = value;
        }
    }

    /// <summary>The maximum allowed zoom.</summary>
    public double Maximum { get; set; } = 2;

    /// <summary>The per-step scale factor (between 1.01 and 2).</summary>
    public double ScaleFactor
    {
        get => _scaleFactor;
        set
        {
            if (value is < 1.01 or > 2)
                throw new ArgumentException("ScaleFactor can't be lower than 1.01 or greater than 2");

            _scaleFactor = value;
        }
    }
}
