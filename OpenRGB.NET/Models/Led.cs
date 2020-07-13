using OpenRGB.NET.Utils;

namespace OpenRGB.NET.Models
{
    /// <summary>
    /// Led class containing the name of the LED
    /// </summary>
    public class Led
    {
        public string Name { get; private set; }
        public uint Value { get; private set; }

        /// <summary>
        /// Decodes a byte array into a LED array.
        /// Increments the offset accordingly.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="ledCount"></param>
        /// <returns></returns>
        internal static Led[] Decode(byte[] buffer, ref int offset, ushort ledCount)
        {
            var leds = new Led[ledCount];

            for (int i = 0; i < ledCount; i++)
            {
                leds[i] = new Led
                {
                    Name = buffer.GetString(ref offset),
                    Value = buffer.GetUInt32(ref offset)
                };
            }

            return leds;
        }

        public override string ToString() => $"Name: {Name}, Value: {Value}";
    }
}
