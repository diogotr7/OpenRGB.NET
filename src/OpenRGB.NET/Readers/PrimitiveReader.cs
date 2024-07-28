using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

internal readonly struct PrimitiveReader<T> : ISpanReader<T> where T : unmanaged
{
    public static T ReadFrom(ref SpanReader reader, ProtocolVersion? protocolVersion = default, int? index = default, int? outerCount = default) =>
        reader.Read<T>();
}