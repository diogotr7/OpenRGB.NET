using System;
using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

internal readonly struct ModesReader : ISpanReader<Mode[]>
{
    public Mode[] ReadFrom(ref SpanReader reader, ProtocolVersion? protocolVersion = default, int? index = default, int? outerCount = default)
    {
        if (protocolVersion is not { } version)
            throw new ArgumentNullException(nameof(protocolVersion));
        if (outerCount is not { } count)
            throw new ArgumentNullException(nameof(outerCount));
        
        var modes = new Mode[count];

        for (var i = 0; i < modes.Length; i++)
        {
            var name = reader.ReadLengthAndString();
            var modeValue = reader.Read<int>();
            var modeFlags = (ModeFlags)reader.Read<uint>();
            var speedMin = reader.Read<uint>();
            var speedMax = reader.Read<uint>();
            var brightMin = version.SupportsBrightnessAndSaveMode ? reader.Read<uint>() : 0;
            var brightMax = version.SupportsBrightnessAndSaveMode ? reader.Read<uint>() : 0;
            var colorMin = reader.Read<uint>();
            var colorMax = reader.Read<uint>();
            var speed = reader.Read<uint>();
            var brightness = version.SupportsBrightnessAndSaveMode ? reader.Read<uint>() : 0;
            var direction = reader.Read<uint>();
            var colorMode = (ColorMode)reader.Read<uint>();
            var colors = new ColorsReader().ReadFrom(ref reader);
            
            modes[i] = new Mode(version, i, name, modeValue, modeFlags, speedMin, speedMax,
                brightMin, brightMax, colorMin, colorMax, speed, brightness,
                (Direction)direction, colorMode, colors);
        }

        return modes;
    }
}