using OpenRGB.NET.Enums;
using OpenRGB.NET.Utils;

namespace OpenRGB.NET.Models
{
    /// <summary>
    /// Device class containing all the info present in an OpenRGB RGBController
    /// </summary>
    public class Device
    {
        /// <summary>
        /// The type of the device.
        /// </summary>
        public DeviceType Type { get; private set; }

        /// <summary>
        /// The name of the device.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The description of device.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// The version of the device. Usually a firmware version.
        /// </summary>
        public string Version { get; private set; }

        /// <summary>
        /// The serial number of the device.
        /// </summary>
        public string Serial { get; private set; }

        /// <summary>
        /// The location of the device. Usually the device file on the filesystem.
        /// </summary>
        public string Location { get; private set; }

        /// <summary>
        /// The index of the currently active mode on the device.
        /// </summary>
        public int ActiveModeIndex { get; private set; }

        /// <summary>
        /// The modes the device can be set to.
        /// </summary>
        public Mode[] Modes { get; private set; }

        /// <summary>
        /// The lighting zones present on the device.
        /// </summary>
        public Zone[] Zones { get; private set; }

        /// <summary>
        /// All the leds present on the device.
        /// </summary>
        public Led[] Leds { get; private set; }

        /// <summary>
        /// The colors of all the leds present on the device.
        /// </summary>
        public Color[] Colors { get; private set; }

        /// <summary>
        /// Shortcut for Modes[ActiveModeIndex], returns the currently actuve mode.
        /// </summary>
        public Mode ActiveMode => Modes[ActiveModeIndex];

        /// <summary>
        /// Decodes a byte array into a Device.
        /// </summary>
        /// <param name="buffer"></param>
        internal static Device Decode(byte[] buffer)
        {
            var dev = new Device();
            int offset = sizeof(uint);

            dev.Type = (DeviceType)buffer.GetInt32(ref offset);

            dev.Name = buffer.GetString(ref offset);

            dev.Description = buffer.GetString(ref offset);

            dev.Version = buffer.GetString(ref offset);

            dev.Serial = buffer.GetString(ref offset);

            dev.Location = buffer.GetString(ref offset);

            var modeCount = buffer.GetUInt16(ref offset);
            dev.ActiveModeIndex = buffer.GetInt32(ref offset);
            dev.Modes = Mode.Decode(buffer, ref offset, modeCount);

            var zoneCount = buffer.GetUInt16(ref offset);
            dev.Zones = Zone.Decode(buffer, ref offset, zoneCount);

            var ledCount = buffer.GetUInt16(ref offset);
            dev.Leds = Led.Decode(buffer, ref offset, ledCount);

            var colorCount = buffer.GetUInt16(ref offset);
            dev.Colors = Color.Decode(buffer, ref offset, colorCount);

            return dev;
        }

        /// <inheritdoc/>
        public override string ToString() => $"{Type}: {Name}";
    }
}
