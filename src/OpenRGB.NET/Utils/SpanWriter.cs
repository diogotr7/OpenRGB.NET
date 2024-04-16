using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenRGB.NET.Utils;

#if DEBUG
[NonCopyable]
#endif
internal unsafe ref struct SpanWriter(Span<byte> span)
{
    public Span<byte> Span { get; } = span;
    public int Position { get; private set; } = 0;
    
    public void Write<T>(T value) where T : unmanaged
    {
        MemoryMarshal.Write(Span[Position..], ref value);
        Position += sizeof(T);
    }

    public void Write(ReadOnlySpan<byte> span)
    {
        span.CopyTo(Span[Position..]);
        Position += span.Length;
    }

    public void WriteLengthAndString(string value)
    {
        Write((ushort)(value.Length + 1));
        Write(Encoding.ASCII.GetBytes(value));
        Write<byte>(0);
    }
    
    public void Write(string value)
    {
        Write(Encoding.ASCII.GetBytes(value));
        Write<byte>(0);
    }
}