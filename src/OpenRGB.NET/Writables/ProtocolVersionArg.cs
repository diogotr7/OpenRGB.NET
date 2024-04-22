using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

internal readonly record struct ProtocolVersionArg(ProtocolVersion Version) : ISpanWritable
{
    public int Length => sizeof(uint);
    public void WriteTo(ref SpanWriter writer) => writer.Write(Version.Number);
}