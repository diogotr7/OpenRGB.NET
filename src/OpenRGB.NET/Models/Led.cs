using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

/// <summary>
///     Led class containing the name of the LED
/// </summary>
public class Led
{
    private Led(int index, string name, uint value)
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

    private static Led ReadFrom(ref SpanReader reader, int index)
    {
        var name = reader.ReadLengthAndString();
        var value = reader.ReadUInt32();
        
        return new Led(index, name, value);
    }

    /// <summary>
    ///     Decodes a byte array into a LED array.
    ///     Increments the offset accordingly.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="ledCount"></param>
    internal static Led[] ReadManyFrom(ref SpanReader reader, ushort ledCount)
    {
        var leds = new Led[ledCount];

        for (var i = 0; i < ledCount; i++)
            leds[i] = ReadFrom(ref reader, i);

        return leds;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Name: {Name}, Value: {Value}";
    }
}