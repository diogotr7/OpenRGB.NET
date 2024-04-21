using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

internal readonly unsafe record struct Args<T1>(T1 Arg1) : ISpanWritable where T1 : unmanaged
{
    public int Length => sizeof(T1);
    public void WriteTo(ref SpanWriter writer)
    {
        writer.Write(Arg1);
    }
}

internal readonly unsafe record struct Args<T1, T2>(T1 Arg1, T2 Arg2) : ISpanWritable 
    where T1 : unmanaged
    where T2 : unmanaged
{
    public int Length =>  sizeof(T1) + sizeof(T2);
    public void WriteTo(ref SpanWriter writer)
    {
        writer.Write(Arg1);
        writer.Write(Arg2);
    }
}

internal readonly unsafe record struct Args<T1, T2, T3>(T1 Arg1, T2 Arg2, T3 Arg3) : ISpanWritable 
    where T1 : unmanaged
    where T2 : unmanaged
    where T3 : unmanaged
{
    public int Length => sizeof(T1) + sizeof(T2) + sizeof(T3);
    public void WriteTo(ref SpanWriter writer)
    {
        writer.Write(Arg1);
        writer.Write(Arg2);
        writer.Write(Arg3);
    }
}