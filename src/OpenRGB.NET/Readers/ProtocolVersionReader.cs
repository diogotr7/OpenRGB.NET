using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

internal readonly struct ProtocolVersionReader : ISpanReader<ProtocolVersion>
{
    public static ProtocolVersion ReadFrom(ref SpanReader reader, ProtocolVersion? p = default, int? i = default, int? outerCount = default)
        => ProtocolVersion.FromNumber(reader.Read<uint>());
}