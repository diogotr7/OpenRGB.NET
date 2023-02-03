using System;
using System.Buffers.Binary;
using System.Text;

namespace OpenRGB.NET.Utils;

#if DEBUG
[NonCopyable]
#endif
internal ref struct SpanWriter
{
    public Span<byte> Span { get; }
    public int Position { get; private set; }

    public SpanWriter(in Span<byte> span)
    {
        Span = span;
        Position = 0;
    }

    public void WriteUInt16(ushort value)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(Span[Position..], value);
        Position += sizeof(ushort);
    }

    public void WriteInt32(int value)
    {
        BinaryPrimitives.WriteInt32LittleEndian(Span[Position..], value);
        Position += sizeof(int);
    }

    public void WriteUInt32(uint value)
    {
        BinaryPrimitives.WriteUInt32LittleEndian(Span[Position..], value);
        Position += sizeof(uint);
    }

    public void WriteBytes(ReadOnlySpan<byte> span)
    {
        span.CopyTo(Span[Position..]);
        Position += span.Length;
    }

    public void WriteByte(byte value)
    {
        Span[Position] = value;
        Position += sizeof(byte);
    }

    public void WriteLengthAndString(string value)
    {
        WriteUInt16((ushort)(value.Length + 1));
        WriteBytes(Encoding.ASCII.GetBytes(value));
        WriteByte(0);
    }
}