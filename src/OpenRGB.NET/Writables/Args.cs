using System.Runtime.CompilerServices;
using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

internal readonly record struct Args<T1>(T1 Arg1) : ISpanWritable where T1 : unmanaged
{
    public int Length => Unsafe.SizeOf<T1>();
    public void WriteTo(ref SpanWriter writer)
    {
        writer.Write(Arg1);
    }
}

internal readonly record struct Args<T1, T2>(T1 Arg1, T2 Arg2) : ISpanWritable 
    where T1 : unmanaged
    where T2 : unmanaged
{
    public int Length => Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>();
    public void WriteTo(ref SpanWriter writer)
    {
        writer.Write(Arg1);
        writer.Write(Arg2);
    }
}