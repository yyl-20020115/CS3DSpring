namespace CS3DSpring.Colors;

/// <summary>
/// Structure to define HSL.
/// </summary>
/// <remarks>
/// Creates an instance of a HSL structure.
/// </remarks>
/// <param name="h">Hue value.</param>
/// <param name="s">Saturation value.</param>
/// <param name="l">Lightness value.</param>
public struct HSL(double h, double s, double l)
{
    /// <summary>
    /// Gets an empty HSL structure;
    /// </summary>
    public static readonly HSL Empty = new();

    private double hue = (h > 360) ? 360 : ((h < 0) ? 0 : h);
    private double saturation = (s > 1) ? 1 : ((s < 0) ? 0 : s);
    private double luminance = (l > 1) ? 1 : ((l < 0) ? 0 : l);

    public static bool operator ==(HSL item1, HSL item2) => item1.Hue == item2.Hue
            && item1.Saturation == item2.Saturation
            && item1.Luminance == item2.Luminance
            ;

    public static bool operator !=(HSL item1, HSL item2) => item1.Hue != item2.Hue
            || item1.Saturation != item2.Saturation
            || item1.Luminance != item2.Luminance
            ;

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
    /// Gets or sets the luminance component.
    /// </summary>
    public double Luminance
    {
        readonly get => luminance;
        set => luminance = (value > 1) ? 1 : ((value < 0) ? 0 : value);
    }

    public override readonly bool Equals(object? obj) => obj != null && GetType() == obj.GetType() && this == (HSL)obj;

    public override readonly int GetHashCode() => Hue.GetHashCode() ^ Saturation.GetHashCode() ^
            Luminance.GetHashCode();
}