using System;
using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

/// <summary>
///     Packet Header class containing the command ID and the length of the data to be sent.
/// </summary>
internal readonly struct PacketHeader(uint deviceId, CommandId command, uint length) : ISpanWritable
{
    internal const int LENGTH = 16;
    internal static ReadOnlySpan<byte> MagicBytes => "ORGB"u8;

    internal uint DeviceId { get; } = deviceId;
    internal CommandId Command { get; } = command;
    internal uint DataLength { get; } = length;

    public int Length => LENGTH;
    
    public static PacketHeader FromSpan(ReadOnlySpan<byte> span)
    {
        var reader = new SpanReader(span);
        var magicBytes = reader.ReadBytes(4);
        if (!magicBytes.SequenceEqual(MagicBytes))
            throw new ArgumentException($"Magic bytes \"ORGB\" were not found");
        
        var device = reader.Read<uint>();
        var command = (CommandId)reader.Read<uint>();
        var length = reader.Read<uint>();
        
        return new PacketHeader(device, command, length);
    }

    public void WriteTo(ref SpanWriter writer)
    {
        writer.Write(MagicBytes);
        writer.Write(DeviceId);
        writer.Write((uint)Command);
        writer.Write(DataLength);
    }
}