namespace CS3DSpring.Colors;

/// <summary>
/// Structure to define CMYK.
/// </summary>
/// <remarks>
/// Creates an instance of a CMYK structure.
/// </remarks>
public struct CMYK(double c, double m, double y, double k)
{
    /// <summary>
    /// Gets an empty CMYK structure;
    /// </summary>
    public readonly static CMYK Empty = new();

    private double c = c;
    private double m = m;
    private double y = y;
    private double k = k;

    public static bool operator ==(CMYK item1, CMYK item2) => item1.Cyan == item2.Cyan
            && item1.Magenta == item2.Magenta
            && item1.Yellow == item2.Yellow
            && item1.Black == item2.Black
            ;

    public static bool operator !=(CMYK item1, CMYK item2) => item1.Cyan != item2.Cyan
            || item1.Magenta != item2.Magenta
            || item1.Yellow != item2.Yellow
            || item1.Black != item2.Black
            ;

    public double Cyan
    {
        readonly get => c;
        set
        {
            c = value;
            c = (c > 1) ? 1 : ((c < 0) ? 0 : c);
        }
    }

    public double Magenta
    {
        readonly get => m;
        set
        {
            m = value;
            m = (m > 1) ? 1 : ((m < 0) ? 0 : m);
        }
    }

    public double Yellow
    {
        readonly get => y;
        set
        {
            y = value;
            y = (y > 1) ? 1 : ((y < 0) ? 0 : y);
        }
    }

    public double Black
    {
        readonly get => k;
        set
        {
            k = value;
            k = (k > 1) ? 1 : ((k < 0) ? 0 : k);
        }
    }

    public override readonly bool Equals(object? obj)
        => obj != null && GetType() == obj.GetType() && this == (CMYK)obj;

    public override int GetHashCode() => Cyan.GetHashCode() ^
          Magenta.GetHashCode() ^ Yellow.GetHashCode() ^ Black.GetHashCode();

}