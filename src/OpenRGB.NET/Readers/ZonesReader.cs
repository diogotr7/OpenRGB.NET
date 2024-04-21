using System;
using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

internal readonly struct ZonesReader : ISpanReader<Zone[]>
{
    public Zone[] ReadFrom(ref SpanReader reader, ProtocolVersion? protocolVersion = default, int? index = default, int? outerCount = default)
    {
        if (protocolVersion is not { } protocol)
            throw new ArgumentNullException(nameof(protocolVersion));
        if (index is not { } deviceIndex)
            throw new ArgumentNullException(nameof(index));
        
        var zoneCount = reader.Read<ushort>();
        var zones = new Zone[zoneCount];

        for (var i = 0; i < zoneCount; i++)
        {
            var name = reader.ReadLengthAndString();
            var type = (ZoneType)reader.Read<uint>();
            var ledsMin = reader.Read<uint>();
            var ledsMax = reader.Read<uint>();
            var ledCount = reader.Read<uint>();
            var zoneMatrixLength = reader.Read<ushort>();
            var matrixMap = zoneMatrixLength > 0 ? new MatrixMapReader().ReadFrom(ref reader) : null;
            var segments = protocol.SupportsSegmentsAndPlugins ? new SegmentsReader().ReadFrom(ref reader, protocolVersion) : [];
            
            zones[i] = new Zone(i, deviceIndex, name, type, ledCount, ledsMin, ledsMax, matrixMap, segments);
        }

        return zones;
    }
}