using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

internal interface ISpanWritable
{
    int Length { get; }
    void WriteTo(ref SpanWriter writer);
}