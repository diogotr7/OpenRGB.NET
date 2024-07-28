using System.Runtime.CompilerServices;
using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

internal readonly record struct UpdateLedsArg(ushort ColorCount) : ISpanWritable
{
    public int Length => sizeof(uint) +
                         sizeof(ushort);
    
    public void WriteTo(ref SpanWriter writer)
    {
        writer.Write((uint)(Length + Unsafe.SizeOf<Color>() * ColorCount));
        writer.Write(ColorCount);
    }
}