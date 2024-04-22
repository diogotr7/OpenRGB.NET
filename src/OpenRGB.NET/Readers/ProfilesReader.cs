using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

internal readonly struct ProfilesReader : ISpanReader<string[]>
{
    public static string[] ReadFrom(ref SpanReader reader, ProtocolVersion? p = default, int? i = default, int? outerCount = default)
    {
        // ReSharper disable once UnusedVariable
        var dataSize = reader.Read<uint>();

        var count = reader.Read<ushort>();
        var profiles = new string[count];
        for (var j = 0; j < count; j++)
        {
            profiles[j] = reader.ReadLengthAndString();
        }

        return profiles;
    }
}