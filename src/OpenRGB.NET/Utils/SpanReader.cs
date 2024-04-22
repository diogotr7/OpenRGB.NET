using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenRGB.NET.Utils;
#if DEBUG
[NonCopyable]
#endif
internal ref struct SpanReader(ReadOnlySpan<byte> span)
{
    private ReadOnlySpan<byte> Span { get; } = span;
    private int Position { get; set; } = 0;

    internal T Read<T>() where T : unmanaged
    {
        var value = MemoryMarshal.Read<T>(Span[Position..]);
        Position += Unsafe.SizeOf<T>();
        return value;
    }
    
    public ReadOnlySpan<byte> ReadBytes(int length)
    {
        var value = Span[Position..(Position + length)];
        Position += length;
        return value;
    }

    public string ReadLengthAndString()
    {
        int length = Read<ushort>();
        
        if (length == 0)
            return string.Empty;
        
        return Encoding.ASCII.GetString(ReadBytes(length)[..^1]);
    }
}