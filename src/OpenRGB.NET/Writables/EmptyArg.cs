using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

internal readonly struct EmptyArg : ISpanWritable
{
    public int Length => 0;
    public void WriteTo(ref SpanWriter writer) { }
}