using System.Runtime.InteropServices;
using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

internal readonly record struct ModeArg(Mode Mode) : ISpanWritable
{
    public int Length
    {
        get
        {
            var size = 0;
            
            size += sizeof(ushort) * 2;// Name Length, Colors Length
            size += sizeof(uint) * 9;// Value, SpeedMin, SpeedMax, ColorMin, ColorMax, Speed, Direction, ColorMode, Flags
            size += Mode.Name.Length + 1;// Name
            size += sizeof(uint) * Mode.Colors.Length;// Colors
            if (Mode.ProtocolVersion.SupportsBrightnessAndSaveMode)
                size += sizeof(uint) * 3;// BrightnessMin, BrightnessMax, Brightness

            return size;
        }
    }
    
    public void WriteTo(ref SpanWriter writer)
    {
        writer.WriteLengthAndString(Mode.Name);
        writer.Write(Mode.Value);
        writer.Write((uint)Mode.Flags);
        writer.Write(Mode.SpeedMin);
        writer.Write(Mode.SpeedMax);

        if (Mode.ProtocolVersion.SupportsBrightnessAndSaveMode)
        {
            writer.Write(Mode.BrightnessMin);
            writer.Write(Mode.BrightnessMax);
        }

        writer.Write(Mode.ColorMin);
        writer.Write(Mode.ColorMax);
        writer.Write(Mode.Speed);

        if (Mode.ProtocolVersion.SupportsBrightnessAndSaveMode)
            writer.Write(Mode.Brightness);

        writer.Write((uint)Mode.Direction);
        writer.Write((uint)Mode.ColorMode);
        writer.Write((ushort)Mode.Colors.Length);
        
        var colors = MemoryMarshal.Cast<Color, byte>(Mode.Colors);
        writer.Write(colors);
    }
}