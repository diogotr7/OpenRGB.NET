using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

internal readonly struct LedsReader : ISpanReader<Led[]>
{
    public static Led[] ReadFrom(ref SpanReader reader, ProtocolVersion? protocolVersion = default, int? index = default, int? outerCount = default)
    {
        var ledCount = reader.Read<ushort>();

        var leds = new Led[ledCount];

        for (var i = 0; i < ledCount; i++)
        {
            var name = reader.ReadLengthAndString();
            var value = reader.Read<uint>();

            leds[i] = new Led(i, name, value);
        }

        return leds;
    }
}