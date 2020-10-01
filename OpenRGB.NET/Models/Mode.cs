using OpenRGB.NET.Enums;
using OpenRGB.NET.Utils;

namespace OpenRGB.NET.Models
{
    /// <summary>
    /// Mode class containing the parameters one mode has.
    /// </summary>
    public class Mode
    {
        /// <summary>
        /// The name of the mode.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Device specific value for this mode. Most likely not useful for the client.
        /// </summary>
        public int Value { get; private set; }

        /// <summary>
        /// Flags containing the features this mode supports.
        /// </summary>
        public ModeFlags Flags { get; private set; }

        /// <summary>
        /// The minimum speed value this mode supports.
        /// </summary>
        public uint SpeedMin { get; private set; }

        /// <summary>
        /// The maximum speed value this mode supports.
        /// </summary>
        public uint SpeedMax { get; private set; }

        /// <summary>
        /// The minimum number of colors this mode supports.
        /// </summary>
        public uint ColorMin { get; private set; }

        /// <summary>
        /// The maximum number of colors this mode supports.
        /// </summary>
        public uint ColorMax { get; private set; }

        /// <summary>
        /// The current speed value of this mode.
        /// </summary>
        public uint Speed { get; set; }

        /// <summary>
        /// The current direction of this mode.
        /// </summary>
        public Direction Direction { get; set; }

        /// <summary>
        /// Mode representing how the Colors are used for effects.
        /// </summary>
        public ColorMode ColorMode { get; private set; }

        /// <summary>
        /// The colors this mode uses for lighting.
        /// </summary>
        public Color[] Colors { get; set; }

        /// <summary>
        /// Determines if the feature is supported in the flags.
        /// </summary>
        public bool HasFlag(ModeFlags flag) => (Flags & flag) != 0;

        /// <summary>
        /// Decodes a byte array into a Mode array.
        /// Increments the offset accordingly.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="numModes"></param>
        internal static Mode[] Decode(byte[] buffer, ref int offset, uint numModes)
        {
            var modes = new Mode[numModes];

            for (int i = 0; i < numModes; i++)
            {
                modes[i] = new Mode();

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
                modes[i].Colors = Color.Decode(buffer, ref offset, colorCount);

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

        internal uint Size => (uint)(
            sizeof(int) * 2 +
            sizeof(uint) * 9 +
            sizeof(ushort) * 2 +
            sizeof(uint) * Colors.Length +
            Name.Length + 1);
    }
}
