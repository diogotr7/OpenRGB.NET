using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenRGB.NET.Utils;

#if DEBUG
[NonCopyable]
#endif
internal ref struct SpanWriter(Span<byte> span)
{
    public Span<byte> Span { get; } = span;
    public int Position { get; private set; } = 0;
    
    public void Write<T>(T value) where T : unmanaged
    {
        MemoryMarshal.Write(Span[Position..], in value);
        Position += Unsafe.SizeOf<T>();
    }

    public void Write(ReadOnlySpan<byte> span)
    {
        span.CopyTo(Span[Position..]);
        Position += span.Length;
    }

    public void WriteLengthAndString(string value)
    {
        Write((ushort)(value.Length + 1));
        Write(value);
    }
    
    public void Write(string value)
    {
        var byteCount = Encoding.ASCII.GetByteCount(value.AsSpan());
        Encoding.ASCII.GetBytes(value, Span.Slice(Position, byteCount));
        Position += byteCount;
        Write<byte>(0);
    }
}