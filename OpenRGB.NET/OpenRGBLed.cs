namespace OpenRGB.NET
{
    public class OpenRGBLed
    {
        public string Name;
        public uint Value;

        internal static OpenRGBLed[] Decode(byte[] buffer, ref int offset, ushort ledCount)
        {
            var leds = new OpenRGBLed[ledCount];

            for (int led = 0; led < ledCount; led++)
            {
                leds[led] = new OpenRGBLed
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
