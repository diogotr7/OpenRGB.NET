namespace OpenRGB.NET;

/// <summary>
///     Led class containing the name of the LED
/// </summary>
public class Led
{
    internal Led(int index, string name, uint value)
    {
        Index = index;
        Name = name;
        Value = value;
    }

    /// <summary>
    ///   The index of the led.
    /// </summary>
    public int Index { get; }
    
    /// <summary>
    ///     The name of the led.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Device specific led value. Most likely not useful for the clients.
    /// </summary>
    public uint Value { get;  }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Name: {Name}, Value: {Value}";
    }
}