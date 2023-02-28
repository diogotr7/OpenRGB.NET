using System;
using System.Text;
using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

/// <summary>
///     Packet Header class containing the command ID and the length of the data to be sent.
/// </summary>
internal readonly struct PacketHeader
{
    internal const int Length = 16;
    private const string Magic = "ORGB";
    private static readonly byte[] MagicBytes = Encoding.ASCII.GetBytes(Magic);
    internal uint DeviceId { get; }
    internal uint Command { get; }
    internal uint DataLength { get; }

    internal PacketHeader(uint deviceId, uint command, uint length)
    {
        DeviceId = deviceId;
        Command = command;
        DataLength = length;
    }

    internal static PacketHeader ReadFrom(ref SpanReader reader)
    {
        if (!reader.ReadBytes(4).SequenceEqual(MagicBytes))
            throw new ArgumentException($"Magic bytes \"{Magic}\" were not found. Data was {reader.Span.ToArray()}");

        return new PacketHeader(reader.ReadUInt32(), reader.ReadUInt32(), reader.ReadUInt32());
    }

    internal void WriteTo(ref SpanWriter writer)
    {
        writer.WriteBytes(MagicBytes);
        writer.WriteUInt32(DeviceId);
        writer.WriteUInt32(Command);
        writer.WriteUInt32(DataLength);
    }
}