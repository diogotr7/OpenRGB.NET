namespace OpenRGB.NET;

/// <summary>
///     Enum representing the various directions a specific mode can have.
/// </summary>
public enum Direction : uint
{
    /// <summary>
    ///     No direction.
    /// </summary>
    None = uint.MaxValue,

    /// <summary>
    ///     Left direction.
    /// </summary>
    Left = 0,

    /// <summary>
    ///     Right direction.
    /// </summary>
    Right = 1,

    /// <summary>
    ///     Up direction.
    /// </summary>
    Up = 2,

    /// <summary>
    ///     Down direction.
    /// </summary>
    Down = 3,

    /// <summary>
    ///     Horizontal direction.
    /// </summary>
    Horizontal = 4,

    /// <summary>
    ///     Vertical direction.
    /// </summary>
    Vertical = 5
}