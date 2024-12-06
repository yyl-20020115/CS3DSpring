namespace CS3DSpring.Colors;

/// <summary>
/// Structure to define YUV.
/// </summary>
/// <remarks>
/// Creates an instance of a YUV structure.
/// </remarks>
public struct YUV(double y, double u, double v)
{
    /// <summary>
    /// Gets an empty YUV structure.
    /// </summary>
    public static readonly YUV Empty = new();

    private double y = (y > 1) ? 1 : ((y < 0) ? 0 : y);
    private double u = (u > 0.436) ? 0.436 : ((u < -0.436) ? -0.436 : u);
    private double v = (v > 0.615) ? 0.615 : ((v < -0.615) ? -0.615 : v);

    public static bool operator ==(YUV item1, YUV item2) => 
            item1.Y == item2.Y
            && item1.U == item2.U
            && item1.V == item2.V
            ;

    public static bool operator !=(YUV item1, YUV item2) => item1.Y != item2.Y
            || item1.U != item2.U
            || item1.V != item2.V
            ;

    public double Y
    {
        readonly get => y;
        set
        {
            y = value;
            y = (y > 1) ? 1 : ((y < 0) ? 0 : y);
        }
    }

    public double U
    {
        readonly get => u;

        set
        {
            u = value;
            u = (u > 0.436) ? 0.436 : ((u < -0.436) ? -0.436 : u);
        }
    }

    public double V
    {
        readonly get => v;
        set
        {
            v = value;
            v = (v > 0.615) ? 0.615 : ((v < -0.615) ? -0.615 : v);
        }
    }

    public override readonly bool Equals(object? obj) => obj != null && GetType() == obj.GetType() && this == (YUV)obj;

    public override readonly int GetHashCode() => Y.GetHashCode() ^ U.GetHashCode() ^ V.GetHashCode();

}