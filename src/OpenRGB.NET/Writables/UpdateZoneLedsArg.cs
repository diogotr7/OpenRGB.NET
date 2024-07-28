using System.Runtime.CompilerServices;
using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

internal readonly record struct UpdateZoneLedsArg(uint ZoneId, ushort ColorCount) : ISpanWritable
{
    public int Length => sizeof(uint) +
                         sizeof(uint) +
                         sizeof(ushort);
    
    public void WriteTo(ref SpanWriter writer)
    {
        writer.Write((uint)(Length + Unsafe.SizeOf<Color>() * ColorCount));
        writer.Write(ZoneId);
        writer.Write(ColorCount);
    }
}