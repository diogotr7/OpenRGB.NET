using System;
using System.Diagnostics;
using System.Text;

namespace OpenRGB.NET.Utils;

/// <summary>
///     Static class to generate packets for various commands
/// </summary>
internal static class PacketFactory
{
    //4 uint zone_index
    //4 uint new_zone_size
    internal const int ResizeZoneLength = 4 + 4;
    internal static void WriteResizeZone(Span<byte> packet, uint deviceId, uint zoneIndex, uint newZoneSize)
    {
        const int LENGTH = PacketHeader.Length + ResizeZoneLength;
        
        if (packet.Length != LENGTH)
            throw new ArgumentException($"Packet length is {packet.Length} but should be {LENGTH}");
        
        var writer = new SpanWriter(packet);
        var header = new PacketHeader(deviceId, CommandId.ResizeZone, ResizeZoneLength);

        header.WriteTo(ref writer);
        writer.WriteUInt32(zoneIndex);
        writer.WriteUInt32(newZoneSize);
    }

    
    internal static int GetUpdateLedsLength(int ledCount)
    {
        //4 uint data_size
        //2 ushort led_count
        //4 * led_count uint led_data

        return 4 + 2 + 4 * ledCount;
    }
    internal static void WriteUpdateLeds(Span<byte> packet, int deviceId, ReadOnlySpan<Color> colors)
    {
        var dataLength = GetUpdateLedsLength(colors.Length);
        var length = PacketHeader.Length + dataLength;

        if (packet.Length != length)
            throw new ArgumentException($"Packet length is {packet.Length} but should be {length}");
        
        var writer = new SpanWriter(packet);
        var header = new PacketHeader((uint)deviceId, CommandId.UpdateLeds, (uint)dataLength);

        header.WriteTo(ref writer);
        writer.WriteUInt32((uint)length);
        writer.WriteUInt16((ushort)colors.Length);

        for (var i = 0; i < colors.Length; i++)
            colors[i].WriteTo(ref writer);
    }

    
    internal static int GetUpdateZoneLedsLength(int ledCount)
    {
        //4 uint data_size
        //4 uint zone_index
        //2 ushort led_count
        //4 * led_count uint led_data

        return 4 + 4 + 2 + 4 * ledCount;
    }
    internal static void WriteUpdateZoneLeds(Span<byte> packet, uint deviceId, uint zoneIndex, ReadOnlySpan<Color> colors)
    {
        var dataLength = GetUpdateZoneLedsLength(colors.Length);
        var length = PacketHeader.Length + dataLength;

        if (packet.Length != length)
            throw new ArgumentException($"Packet length is {packet.Length} but should be {length}");

        var writer = new SpanWriter(packet);
        var header = new PacketHeader(deviceId, CommandId.UpdateZoneLeds, (uint)dataLength);

        header.WriteTo(ref writer);
        writer.WriteUInt32((uint)length);
        writer.WriteUInt32(zoneIndex);
        writer.WriteUInt16((ushort)colors.Length);

        for (var i = 0; i < colors.Length; i++)
            colors[i].WriteTo(ref writer);
    }

    
    //4 uint led_index
    //4 color color
    internal const int UpdateSingleLedLength = 4 + 4;
    internal static void WriteUpdateSingleLed(Span<byte> packet, uint deviceId, uint ledIndex, Color color)
    {
        const int LENGTH = PacketHeader.Length + UpdateSingleLedLength;
        if (packet.Length != LENGTH)
            throw new ArgumentException($"Packet length is {packet.Length} but should be {LENGTH}");
        
        var writer = new SpanWriter(packet);
        var header = new PacketHeader(deviceId, CommandId.UpdateSingleLed, UpdateSingleLedLength);

        header.WriteTo(ref writer);
        writer.WriteUInt32(ledIndex);
        color.WriteTo(ref writer);
    }

    
    internal static int GetStringOperationLength(string profile)
    {
        return Encoding.ASCII.GetByteCount(profile) + 1;
    }
    internal static void WriteStringOperation(Span<byte> packet, string someString, CommandId operation)
    {
        var dataLength = GetStringOperationLength(someString);
        var length = PacketHeader.Length + dataLength;
        
        if (packet.Length != length)
            throw new ArgumentException($"Packet length is {packet.Length} but should be {length}");

        //allocation is fine here, this is not a performance critical path.
        //TODO: when porting to .NET 8 use Encoding.ASCII.TryGetBytes to avoid allocation
        ReadOnlySpan<byte> profileName = Encoding.ASCII.GetBytes(someString + '\0');

        var header = new PacketHeader(0, operation, (uint)profileName.Length);
        var writer = new SpanWriter(packet);

        header.WriteTo(ref writer);
        profileName.CopyTo(packet[PacketHeader.Length..]);
        //Encoding.ASCII.TryGetBytes(profile + '\0', packet[PacketHeader.Length..], out var bytesWritten);
    }

    internal static int GetModeOperationLength(Mode mode)
    {
        //4 uint length
        //4 uint mode_index
        //x mode data

        return 4 + 4 + mode.GetLength();
    }
    internal static void WriteModeOperation(Span<byte> packet, uint deviceId, uint modeIndex, Mode mode, CommandId modeOperation)
    {
        var dataLength = GetModeOperationLength(mode);
        var length = PacketHeader.Length + dataLength;

        if (packet.Length != length)
            throw new ArgumentException($"Packet length is {packet.Length} but should be {length}");
        
        var writer = new SpanWriter(packet);
        var header = new PacketHeader(deviceId, modeOperation, (uint)dataLength);
        
        header.WriteTo(ref writer);
        writer.WriteUInt32((uint)dataLength);
        writer.WriteUInt32(modeIndex);
        mode.WriteTo(ref writer);
    }

    
    internal const int ProtocolVersionLength = 4;
    internal static void WriteProtocolVersion(Span<byte> packet,uint deviceId, uint protocolVersion, CommandId commandId)
    {
        const int LENGTH = PacketHeader.Length + ProtocolVersionLength;
        
        if (packet.Length != LENGTH)
            throw new ArgumentException($"Packet length is {packet.Length} but should be {LENGTH}");
        
        var writer = new SpanWriter(packet);
        var header = new PacketHeader(deviceId, commandId, ProtocolVersionLength);
        
        header.WriteTo(ref writer);
        writer.WriteUInt32(protocolVersion);
    }

    internal static int GetPluginSpecificLength(ReadOnlySpan<byte> data)
    {
        //4 uint plugin_packet_type
        //x plugin_data

        return 4 + data.Length;
    }
    public static void WritePluginSpecific(Span<byte> packet, uint pluginId, uint pluginPacketType, ReadOnlySpan<byte> data)
    {
        var dataLength = GetPluginSpecificLength(data);
        var length = PacketHeader.Length + dataLength;

        if (packet.Length != length)
            throw new ArgumentException($"Packet length is {packet.Length} but should be {length}");
        
        var writer = new SpanWriter(packet);
        var header = new PacketHeader(pluginId, CommandId.PluginSpecific, (uint)data.Length);
        
        header.WriteTo(ref writer);
        writer.WriteUInt32(pluginPacketType);
        if (data.Length > 0)
            writer.WriteBytes(data);
    }
}