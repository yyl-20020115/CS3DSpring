namespace CS3DSpring.Colors;

/// <summary>
/// Structure to define HSB.
/// </summary>
/// <remarks>
/// Creates an instance of a HSB structure.
/// </remarks>
/// <param name="h">Hue value.</param>
/// <param name="s">Saturation value.</param>
/// <param name="b">Brightness value.</param>
public struct HSB(double h, double s, double b)
{
    /// <summary>
    /// Gets an empty HSB structure;
    /// </summary>
    public static readonly HSB Empty = new();

    private double hue = (h > 360) ? 360 : ((h < 0) ? 0 : h);
    private double saturation = (s > 1) ? 1 : ((s < 0) ? 0 : s);
    private double brightness = (b > 1) ? 1 : ((b < 0) ? 0 : b);

    public static bool operator ==(HSB item1, HSB item2) => (
            item1.Hue == item2.Hue
            && item1.Saturation == item2.Saturation
            && item1.Brightness == item2.Brightness
            );

    public static bool operator !=(HSB item1, HSB item2) => (
            item1.Hue != item2.Hue
            || item1.Saturation != item2.Saturation
            || item1.Brightness != item2.Brightness
            );

    /// <summary>
    /// Gets or sets the hue component.
    /// </summary>
    public double Hue
    {
        readonly get => hue;
        set => hue = (value > 360) ? 360 : ((value < 0) ? 0 : value);
    }

    /// <summary>
    /// Gets or sets saturation component.
    /// </summary>
    public double Saturation
    {
        readonly get => saturation;
        set => saturation = (value > 1) ? 1 : ((value < 0) ? 0 : value);
    }

    /// <summary>
    /// Gets or sets the brightness component.
    /// </summary>
    public double Brightness
    {
        readonly get => brightness;
        set => brightness = (value > 1) ? 1 : ((value < 0) ? 0 : value);
    }

    public override readonly bool Equals(object? obj) => obj != null && GetType() == obj.GetType() && this == (HSB)obj;

    public override int GetHashCode() => Hue.GetHashCode() ^ Saturation.GetHashCode() ^
            Brightness.GetHashCode();
}