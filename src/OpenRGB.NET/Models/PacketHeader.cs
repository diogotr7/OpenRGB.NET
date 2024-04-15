using System;
using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

/// <summary>
///     Packet Header class containing the command ID and the length of the data to be sent.
/// </summary>
internal readonly struct PacketHeader : ISpanWritable
{
    internal const int LENGTH = 16;
    internal static ReadOnlySpan<byte> MagicBytes => "ORGB"u8;

    internal uint DeviceId { get; }
    internal CommandId Command { get; }
    internal uint DataLength { get; }

    internal PacketHeader(uint deviceId, CommandId command, uint length)
    {
        DeviceId = deviceId;
        Command = command;
        DataLength = length;
    }

    public int Length => LENGTH;

    public static PacketHeader ReadFrom(ref SpanReader reader, ProtocolVersion p = default, int i = default)
    {
        if (!reader.ReadBytes(4).SequenceEqual(MagicBytes))
            throw new ArgumentException($"Magic bytes \"ORGB\" were not found");

        return new PacketHeader(reader.Read<uint>(), (CommandId)reader.Read<uint>(), reader.Read<uint>());
    }
    
    public static PacketHeader FromSpan(ReadOnlySpan<byte> span)
    {
        var reader = new SpanReader(span);
        return ReadFrom(ref reader);
    }

    public void WriteTo(ref SpanWriter writer)
    {
        writer.Write(MagicBytes);
        writer.Write(DeviceId);
        writer.Write((uint)Command);
        writer.Write(DataLength);
    }
}