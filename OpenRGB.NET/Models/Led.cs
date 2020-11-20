using OpenRGB.NET.Utils;
using System;
using System.IO;

namespace OpenRGB.NET.Models
{
    /// <summary>
    /// Led class containing the name of the LED
    /// </summary>
    public class Led
    {
        /// <summary>
        /// The name of the led.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Device specific led value. Most likely not useful for the clients.
        /// </summary>
        public uint Value { get; private set; }

        /// <summary>
        /// Decodes a byte array into a LED array.
        /// Increments the offset accordingly.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="ledCount"></param>
        internal static Led[] Decode(BinaryReader reader, ushort ledCount)
        {
            var leds = new Led[ledCount];

            for (int i = 0; i < ledCount; i++)
            {
                leds[i] = new Led
                {
                    Name = reader.ReadLengthAndString(),
                    Value = reader.ReadUInt32()
                };
            }

            return leds;
        }

        /// <inheritdoc/>
        public override string ToString() => $"Name: {Name}, Value: {Value}";
    }
}
