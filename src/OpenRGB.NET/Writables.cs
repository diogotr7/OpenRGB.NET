using System.Runtime.CompilerServices;
using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

internal readonly struct None : ISpanWritable
{
    public int Length => 0;

    public void WriteTo(ref SpanWriter writer)
    {
    }
}

internal readonly struct OpenRgbString(string value) : ISpanWritable
{
    public readonly string Value = value;
    public int Length => Value.Length + 1;
    public void WriteTo(ref SpanWriter writer) => writer.Write(Value);
}

internal readonly struct ModeOperation : ISpanWritable
{
    public readonly uint DataSize;
    public readonly uint ModeId;
    public readonly Mode Mode;
    
    public ModeOperation(uint dataSize, uint modeId, Mode mode)
    {
        DataSize = dataSize;
        ModeId = modeId;
        Mode = mode;
    }
    
    public int Length => sizeof(uint) * 2 + Mode.Length;
    
    public void WriteTo(ref SpanWriter writer)
    {
        writer.Write(DataSize);
        writer.Write(ModeId);
        Mode.WriteTo(ref writer);
    }
}

internal readonly struct Args<T1>(T1 arg1) : ISpanWritable 
    where T1 : unmanaged
{
    public readonly T1 Arg1 = arg1;

    public int Length => Unsafe.SizeOf<T1>();

    public void WriteTo(ref SpanWriter writer)
    {
        writer.Write(Arg1);
    }
}

internal readonly struct Args<T1, T2>(T1 arg1, T2 arg2) : ISpanWritable 
    where T1 : unmanaged
    where T2 : unmanaged
{
    public readonly T1 Arg1 = arg1;
    public readonly T2 Arg2 = arg2;

    public int Length =>  Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>();

    public void WriteTo(ref SpanWriter writer)
    {
        writer.Write(Arg1);
        writer.Write(Arg2);
    }
}

internal readonly struct Args<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3) : ISpanWritable 
    where T1 : unmanaged
    where T2 : unmanaged
    where T3 : unmanaged
{
    public readonly T1 Arg1 = arg1;
    public readonly T2 Arg2 = arg2;
    public readonly T3 Arg3 = arg3;

    public int Length => Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>();

    public void WriteTo(ref SpanWriter writer)
    {
        writer.Write(Arg1);
        writer.Write(Arg2);
        writer.Write(Arg3);
    }
}