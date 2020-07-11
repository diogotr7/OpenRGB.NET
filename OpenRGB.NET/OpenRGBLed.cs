namespace OpenRGB.NET
{
    public class OpenRGBLed
    {
        public string Name { get; private set; }
        public uint Value { get; private set; }

        internal static OpenRGBLed[] Decode(byte[] buffer, ref int offset, ushort ledCount)
        {
            var leds = new OpenRGBLed[ledCount];

            for (int i = 0; i < ledCount; i++)
            {
                leds[i] = new OpenRGBLed
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
