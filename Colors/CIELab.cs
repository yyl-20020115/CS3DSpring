namespace CS3DSpring.Colors;

/// <summary>
/// Structure to define CIE L*a*b*.
/// </summary>
public struct CIELab(double l, double a, double b)
{
    /// <summary>
    /// Gets an empty CIELab structure.
    /// </summary>
    public static readonly CIELab Empty = new();

    public static bool operator ==(CIELab item1, CIELab item2) => item1.L == item2.L
            && item1.A == item2.A
            && item1.B == item2.B
            ;

    public static bool operator !=(CIELab item1, CIELab item2) => (
            item1.L != item2.L
            || item1.A != item2.A
            || item1.B != item2.B
            );


    /// <summary>
    /// Gets or sets L component.
    /// </summary>
    public double L { get; set; } = l;

    /// <summary>
    /// Gets or sets a component.
    /// </summary>
    public double A { get; set; } = a;

    /// <summary>
    /// Gets or sets a component.
    /// </summary>
    public double B { get; set; } = b;

    public override readonly bool Equals(object? obj) 
        => obj != null && GetType() == obj.GetType() && this == (CIELab)obj;

    public override readonly int GetHashCode() => L.GetHashCode() ^ A.GetHashCode() ^ B.GetHashCode();

}