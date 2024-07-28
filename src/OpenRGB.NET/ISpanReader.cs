using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

internal interface ISpanReader<out T>
{
    static abstract T ReadFrom(ref SpanReader reader, ProtocolVersion? protocolVersion = default, int? index = default, int? outerCount = default);
}