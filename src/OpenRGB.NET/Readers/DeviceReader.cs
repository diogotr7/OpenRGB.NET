using System;
using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

internal readonly struct DeviceReader : ISpanReader<Device>
{
    public Device ReadFrom(ref SpanReader reader, ProtocolVersion? protocolVersion = default, int? index = default, int? outerCount = default)
    {
        if (protocolVersion is not { } protocol)
            throw new ArgumentNullException(nameof(protocolVersion));
        if (index is not { } deviceIndex)
            throw new ArgumentNullException(nameof(index));
        
        
        // ReSharper disable once UnusedVariable
        var dataSize = reader.Read<uint>();

        var deviceType = reader.Read<int>();
        var name = reader.ReadLengthAndString();
        var vendor = protocol.SupportsVendorString ? reader.ReadLengthAndString() : null;
        var description = reader.ReadLengthAndString();
        var version = reader.ReadLengthAndString();
        var serial = reader.ReadLengthAndString();
        var location = reader.ReadLengthAndString();
        var modeCount = reader.Read<ushort>();
        var activeMode = reader.Read<int>();
        var modes = new ModesReader().ReadFrom(ref reader, protocol, outerCount: modeCount);
        var zones = new ZonesReader().ReadFrom(ref reader, protocol, index: deviceIndex);
        var leds = new LedsReader().ReadFrom(ref reader);
        var colors = new ColorsReader().ReadFrom(ref reader);

        return new Device(deviceIndex, (DeviceType)deviceType,
            name, vendor, description, version, serial, location,
            activeMode, modes, zones, leds, colors);
    }
}