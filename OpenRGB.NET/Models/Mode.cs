using OpenRGB.NET.Enums;
using OpenRGB.NET.Utils;
using System.IO;

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
        /// <param name="reader"></param>
        /// <param name="numModes"></param>
        internal static Mode[] Decode(BinaryReader reader, ushort numModes)
        {
            var modes = new Mode[numModes];

            for (int i = 0; i < numModes; i++)
            {
                modes[i] = new Mode();

                modes[i].Name = reader.ReadLengthAndString();

                modes[i].Value = reader.ReadInt32();

                modes[i].Flags = (ModeFlags)reader.ReadUInt32();

                var speedMin = reader.ReadUInt32();

                var speedMax = reader.ReadUInt32();

                var colorMin = reader.ReadUInt32();

                var colorMax = reader.ReadUInt32();

                var speed = reader.ReadUInt32();

                var direction = reader.ReadUInt32();

                modes[i].ColorMode = (ColorMode)reader.ReadUInt32();

                ushort colorCount = reader.ReadUInt16();
                modes[i].Colors = Color.Decode(reader, colorCount);

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
