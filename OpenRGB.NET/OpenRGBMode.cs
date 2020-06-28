using OpenRGB.NET.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenRGB.NET
{
    public class OpenRGBMode
    {
        public string Name;
        public int Value;
        public OpenRGBModeFlags Flags;
        public uint SpeedMin;
        public uint SpeedMax;
        public uint ColorMin;
        public uint ColorMax;
        public uint Speed;
        public OpenRGBModeDirection Direction;
        public OpenRGBColorMode ColorMode;
        public OpenRGBColor[] Colors;

        public static OpenRGBMode[] Decode(byte[] buffer, ref int offset, uint numModes)
        {
            var modes = new List<OpenRGBMode>((int)numModes);

            for (int mode = 0; mode < numModes; mode++)
            {
                var newMode = new OpenRGBMode();

                newMode.Name = BufferReader.GetString(buffer, ref offset);

                newMode.Value = BufferReader.GetInt32(buffer, ref offset);

                newMode.Flags = (OpenRGBModeFlags)BufferReader.GetUInt32(buffer, ref offset);

                var speedMin = BufferReader.GetUInt32(buffer, ref offset);

                var speedMax = BufferReader.GetUInt32(buffer, ref offset);

                var colorMin = BufferReader.GetUInt32(buffer, ref offset);

                var colorMax = BufferReader.GetUInt32(buffer, ref offset);

                var speed = BufferReader.GetUInt32(buffer, ref offset);

                var direction = BufferReader.GetUInt32(buffer, ref offset);

                newMode.ColorMode = (OpenRGBColorMode)BufferReader.GetUInt32(buffer, ref offset);

                ushort colorCount = BufferReader.GetUInt16(buffer, ref offset);
                newMode.Colors = OpenRGBColor.Decode(buffer, ref offset, colorCount);

                if (newMode.Flags.HasFlag(OpenRGBModeFlags.HasSpeed))
                {
                    newMode.Speed = speed;
                    newMode.SpeedMin = speedMin;
                    newMode.SpeedMax = speedMax;
                }

                if (newMode.Flags.HasFlag(OpenRGBModeFlags.HasModeSpecificColor))
                {
                    newMode.ColorMin = colorMin;
                    newMode.ColorMax = colorMax;
                }

                if (newMode.Flags.HasFlag(OpenRGBModeFlags.HasDirectionHV) ||
                    newMode.Flags.HasFlag(OpenRGBModeFlags.HasDirectionLR) ||
                    newMode.Flags.HasFlag(OpenRGBModeFlags.HasDirectionUD))
                {
                    newMode.Direction = (OpenRGBModeDirection)direction;
                }
                else
                {
                    newMode.Direction = OpenRGBModeDirection.None;
                }

                modes.Add(newMode);
            }

            return modes.ToArray();
        }
    }
}
