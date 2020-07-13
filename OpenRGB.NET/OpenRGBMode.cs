using OpenRGB.NET.Enums;
using OpenRGB.NET.Utils;

namespace OpenRGB.NET
{
    public class OpenRGBMode
    {
        public string Name { get; private set; }
        public int Value { get; private set; }
        public ModeFlags Flags { get; private set; }
        public uint SpeedMin { get; private set; }
        public uint SpeedMax { get; private set; }
        public uint ColorMin { get; private set; }
        public uint ColorMax { get; private set; }
        public uint Speed { get; private set; }
        public Direction Direction { get; private set; }
        public ColorMode ColorMode { get; private set; }
        public OpenRGBColor[] Colors { get; private set; }

        public bool HasFlag(ModeFlags flag) => (Flags & flag) != 0;

        internal static OpenRGBMode[] Decode(byte[] buffer, ref int offset, uint numModes)
        {
            var modes = new OpenRGBMode[numModes];

            for (int i = 0; i < numModes; i++)
            {
                modes[i] = new OpenRGBMode();

                modes[i].Name = buffer.GetString(ref offset);

                modes[i].Value = buffer.GetInt32(ref offset);

                modes[i].Flags = (ModeFlags)buffer.GetUInt32(ref offset);

                var speedMin = buffer.GetUInt32(ref offset);

                var speedMax = buffer.GetUInt32(ref offset);

                var colorMin = buffer.GetUInt32(ref offset);

                var colorMax = buffer.GetUInt32(ref offset);

                var speed = buffer.GetUInt32(ref offset);

                var direction = buffer.GetUInt32(ref offset);

                modes[i].ColorMode = (ColorMode)buffer.GetUInt32(ref offset);

                ushort colorCount = buffer.GetUInt16(ref offset);
                modes[i].Colors = OpenRGBColor.Decode(buffer, ref offset, colorCount);

                if (modes[i].HasFlag(ModeFlags.HasSpeed))
                {
                    modes[i].Speed = speed;
                    modes[i].SpeedMin = speedMin;
                    modes[i].SpeedMax = speedMax;
                }

                if (modes[i].HasFlag(ModeFlags.HasModeSpecificColor))
                {
                    modes[i].ColorMin = colorMin;
                    modes[i].ColorMax = colorMax;
                }

                if (modes[i].HasFlag(ModeFlags.HasDirectionHV) ||
                    modes[i].HasFlag(ModeFlags.HasDirectionLR) ||
                    modes[i].HasFlag(ModeFlags.HasDirectionUD))
                {
                    modes[i].Direction = (Direction)direction;
                }
                else
                {
                    modes[i].Direction = Direction.None;
                }
            }

            return modes;
        }
    }
}
