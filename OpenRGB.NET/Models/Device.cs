using OpenRGB.NET.Enums;
using OpenRGB.NET.Utils;
using System.IO;

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
        /// The vendor of the device. Will be null on protocol versions below 1.
        /// </summary>
        public string Vendor { get; private set; }

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
        /// <param name="protocol"></param>
        internal static Device Decode(byte[] buffer, uint protocol)
        {
            var dev = new Device();
            using (var reader = new BinaryReader(new MemoryStream(buffer)))
            {
                var duplicatePacketLength = reader.ReadUInt32();

                dev.Type = (DeviceType)reader.ReadInt32();

                dev.Name = reader.ReadLengthAndString();

                if (protocol >= 1)
                {
                    dev.Vendor = reader.ReadLengthAndString();
                }
                else
                {
                    dev.Vendor = null;
                }

                dev.Description = reader.ReadLengthAndString();

                dev.Version = reader.ReadLengthAndString();

                dev.Serial = reader.ReadLengthAndString();

                dev.Location = reader.ReadLengthAndString();

                var modeCount = reader.ReadUInt16();
                dev.ActiveModeIndex = reader.ReadInt32();
                dev.Modes = Mode.Decode(reader, modeCount);

                var zoneCount = reader.ReadUInt16();
                dev.Zones = Zone.Decode(reader, zoneCount);

                var ledCount = reader.ReadUInt16();
                dev.Leds = Led.Decode(reader, ledCount);

                var colorCount = reader.ReadUInt16();
                dev.Colors = Color.Decode(reader, colorCount);
                return dev;
            }
        }

        /// <inheritdoc/>
        public override string ToString() => $"{Type}: {Name}";
    }
}
