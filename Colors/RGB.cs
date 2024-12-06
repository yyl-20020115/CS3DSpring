namespace CS3DSpring.Colors;

/// <summary>
/// RGB structure.
/// </summary>
public struct RGB(int R, int G, int B)
{
    /// <summary>
    /// Gets an empty RGB structure;
    /// </summary>
    public static readonly RGB Empty = new();

    private int red = (R > 255) ? 255 : ((R < 0) ? 0 : R);
    private int green = (G > 255) ? 255 : ((G < 0) ? 0 : G);
    private int blue = (B > 255) ? 255 : ((B < 0) ? 0 : B);

    public static bool operator ==(RGB item1, RGB item2) => item1.Red == item2.Red
            && item1.Green == item2.Green
            && item1.Blue == item2.Blue
            ;

    public static bool operator !=(RGB item1, RGB item2) => item1.Red != item2.Red
            || item1.Green != item2.Green
            || item1.Blue != item2.Blue
            ;

    /// <summary>
    /// Gets or sets red value.
    /// </summary>
    public int Red
    {
        readonly get => red;
        set => red = (value > 255) ? 255 : ((value < 0) ? 0 : value);
    }

    /// <summary>
    /// Gets or sets red value.
    /// </summary>
    public int Green
    {
        readonly get => green;
        set => green = (value > 255) ? 255 : ((value < 0) ? 0 : value);
    }

    /// <summary>
    /// Gets or sets red value.
    /// </summary>
    public int Blue
    {
        readonly get => blue;
        set => blue = (value > 255) ? 255 : ((value < 0) ? 0 : value);
    }

    public override readonly bool Equals(object? obj) 
        => obj != null && GetType() == obj.GetType() && this == (RGB)obj;
    public override readonly string ToString() => $"({this.Red},{this.Green},{this.blue})";
    public override readonly int GetHashCode() => Red.GetHashCode() ^ Green.GetHashCode() ^ Blue.GetHashCode();
}