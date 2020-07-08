using OpenRGB.NET.Enums;
using System.Collections.Generic;

namespace OpenRGB.NET
{
    public class OpenRGBMode
    {
        public string Name { get; private set; }
        public int Value { get; private set; }
        public OpenRGBModeFlags Flags { get; private set; }
        public uint SpeedMin { get; private set; }
        public uint SpeedMax { get; private set; }
        public uint ColorMin { get; private set; }
        public uint ColorMax { get; private set; }
        public uint Speed { get; private set; }
        public OpenRGBModeDirection Direction { get; private set; }
        public OpenRGBColorMode ColorMode { get; private set; }
        public OpenRGBColor[] Colors { get; private set; }

        internal static OpenRGBMode[] Decode(byte[] buffer, ref int offset, uint numModes)
        {
            var modes = new List<OpenRGBMode>((int)numModes);

            for (int mode = 0; mode < numModes; mode++)
            {
                var newMode = new OpenRGBMode();

                newMode.Name = buffer.GetString(ref offset);

                newMode.Value = buffer.GetInt32(ref offset);

                newMode.Flags = (OpenRGBModeFlags)buffer.GetUInt32(ref offset);

                var speedMin = buffer.GetUInt32(ref offset);

                var speedMax = buffer.GetUInt32(ref offset);

                var colorMin = buffer.GetUInt32(ref offset);

                var colorMax = buffer.GetUInt32(ref offset);

                var speed = buffer.GetUInt32(ref offset);

                var direction = buffer.GetUInt32(ref offset);

                newMode.ColorMode = (OpenRGBColorMode)buffer.GetUInt32(ref offset);

                ushort colorCount = buffer.GetUInt16(ref offset);
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
