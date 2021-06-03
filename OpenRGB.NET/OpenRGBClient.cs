using OpenRGB.NET.Enums;
using OpenRGB.NET.Models;
using OpenRGB.NET.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace OpenRGB.NET
{
    /// <summary>
    /// Client for the OpenRGB SDK.
    /// </summary>
    public class OpenRGBClient : IDisposable, IOpenRGBClient
    {
        private readonly string _ip;
        private readonly int _port;
        private readonly string _name;
        private readonly Socket _socket;
        private readonly int _timeout;
        private bool disposed;

        /// <inheritdoc/>
        public bool Connected => _socket?.Connected ?? false;

        /// <inheritdoc/>
        public uint MaxSupportedProtocolVersion => 2;

        /// <inheritdoc/>
        public uint ClientProtocolVersion { get; private set; }

        /// <inheritdoc/>
        public uint ProtocolVersion { get; private set; }

        #region Basic init methods
        /// <summary>
        /// Sets all the needed parameters to connect to the server.
        /// Connects to the server immediately unless autoconnect is set to false.
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="name"></param>
        /// <param name="autoconnect"></param>
        /// <param name="timeout"></param>
        /// <param name="protocolVersion"></param>
        public OpenRGBClient(string ip = "127.0.0.1", int port = 6742, string name = "OpenRGB.NET", bool autoconnect = true, int timeout = 1000, uint protocolVersion = 2)
        {
            _ip = ip;
            _port = port;
            _name = name;
            _timeout = timeout;
            _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

            if (protocolVersion > MaxSupportedProtocolVersion)
                throw new ArgumentException(nameof(protocolVersion));
            ClientProtocolVersion = protocolVersion;

            if (autoconnect) Connect();
        }

        /// <inheritdoc/>
        public void Connect()
        {
            if (Connected)
                return;

            IAsyncResult result = _socket.BeginConnect(_ip, _port, null, null);

            result.AsyncWaitHandle.WaitOne(_timeout);

            if (_socket.Connected)
            {
                _socket.EndConnect(result);
            }
            else
            {
                _socket.Close();
                throw new TimeoutException("Failed to connect to server.");
            }

            //null terminate before sending
            SendMessage(
                CommandId.SetClientName,
                Encoding.ASCII.GetBytes(_name + '\0')
            );

            ProtocolVersion = Math.Min(ClientProtocolVersion, GetServerProtocolVersion());
        }
        #endregion

        #region Basic Comms methods
        /// <summary>
        /// Sends a message to the server with the given command and buffer of data.
        /// Takes care of sending a header packet first to tell the server how many bytes to read.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="buffer"></param>
        /// <param name="deviceId"></param>
        private void SendMessage(CommandId command, IEnumerable<byte> buffer = null, uint deviceId = 0)
        {
            //we can send the header right away. it contains the command we are sending
            //and the size of the packet that follows
            var packetSize = buffer?.Count() ?? 0;
            var result = _socket.Send(
                new PacketHeader(deviceId, (uint)command, (uint)packetSize).Encode()
            );

            if (result != PacketHeader.Size)
                throw new Exception("Sent incorrect number of bytes when sending header in " + nameof(SendMessage));

            if (packetSize <= 0)
                return;

            result = 0;
            if (buffer is byte[] arr)
                result = _socket.Send(arr);
            else
                result = _socket.Send(buffer.ToArray());

            if (result != packetSize)
                throw new Exception("Sent incorrect number of bytes when sending data in " + nameof(SendMessage));
        }

        /// <summary>
        /// Reads data from the server. Receives the header packet first to know how many bytes to read.
        /// </summary>
        /// <returns>the data received from the server</returns>
        private byte[] ReadMessage()
        {
            //we need a byte buffer to store the header
            var headerBuffer = new byte[PacketHeader.Size];
            //then we read into that buffer
            _socket.Receive(headerBuffer, PacketHeader.Size, SocketFlags.None);
            //and decode it into a header to know how many bytes we will receive next
            var header = PacketHeader.Decode(headerBuffer);
            if (header.DataLength <= 0)
                throw new Exception("Length of header was zero");

            //we then make a buffer that will receive the data
            var dataBuffer = new byte[header.DataLength];

            var size = (int)header.DataLength;
            var total = 0;

            //we might need to receive multiple packets to get all the data
            while (total < size)
            {
                var recv = _socket.Receive(dataBuffer, total, size - total, SocketFlags.None);
                if (recv == 0)
                {
                    break;
                    //maybe should handle this differently?
                }
                total += recv;
            }

            return dataBuffer;
        }
        #endregion

        #region Request Methods
        /// <inheritdoc/>
        public int GetControllerCount()
        {
            SendMessage(CommandId.RequestControllerCount);
            return (int)BitConverter.ToUInt32(ReadMessage(), 0);
        }

        /// <inheritdoc/>
        public Device GetControllerData(int id)
        {
            if (id < 0)
                throw new ArgumentException(nameof(id));

            SendMessage(CommandId.RequestControllerData, BitConverter.GetBytes(ProtocolVersion), (uint)id);
            return Device.Decode(ReadMessage(), ProtocolVersion);
        }

        /// <inheritdoc/>
        public Device[] GetAllControllerData()
        {
            var count = GetControllerCount();

            var array = new Device[count];
            for (int i = 0; i < count; i++)
                array[i] = GetControllerData(i);

            return array;
        }

        /// <summary>
        /// Enumerates all controllers instead of returning an array containing all of them.
        /// </summary>
        public IEnumerable<Device> EnumerateControllerData()
        {
            var count = GetControllerCount();

            for (int i = 0; i < count; i++)
                yield return GetControllerData(i);
        }

        /// <inheritdoc/>
        public string[] GetProfiles()
        {
            if (ClientProtocolVersion < 2)
                throw new NotSupportedException($"Not supported on protocol version {ClientProtocolVersion}");

            SendMessage(CommandId.RequestProfiles, BitConverter.GetBytes(ProtocolVersion));
            var buffer = ReadMessage();

            using (var reader = new BinaryReader(new MemoryStream(buffer)))
            {
                var dataSize = reader.ReadUInt32();
                var count = reader.ReadUInt16();
                var profiles = new string[count];

                for (int i = 0; i < count; i++)
                    profiles[i] = reader.ReadLengthAndString();

                return profiles;
            }
        }

        private uint GetServerProtocolVersion()
        {
            SendMessage(CommandId.RequestProtocolVersion, BitConverter.GetBytes(ClientProtocolVersion));
            uint serverVersion;
            _socket.ReceiveTimeout = 1000;
            try
            {
                serverVersion =  BitConverter.ToUInt32(ReadMessage(), 0);
            }
            catch (SocketException e) when (e.SocketErrorCode == SocketError.TimedOut)
            {
                serverVersion = 0;
            }
            _socket.ReceiveTimeout = 0;

            return serverVersion;
        }
        #endregion

        #region Update Methods
        /// <inheritdoc/>
        public void UpdateLeds(int deviceId, Color[] colors)
        {
            if (colors is null)
                throw new ArgumentNullException(nameof(colors));

            if (deviceId < 0)
                throw new ArgumentException(nameof(deviceId));

            //4 bytes of nothing
            //2 bytes for how many colors (sizeof(short))
            //4 bytes for each led
            int GetIndex(int a) => 4 + 2 + (4 * a);

            var ledCount = colors.Length;
            var bytes = new byte[GetIndex(ledCount)];

            bytes[0] = 0;
            bytes[1] = 0;
            bytes[2] = 0;
            bytes[3] = 0;
            BitConverter.GetBytes((ushort)ledCount).CopyTo(bytes, 4);

            for (int i = 0; i < ledCount; i++)
                colors[i].Encode().CopyTo(bytes, GetIndex(i));

            SendMessage(CommandId.UpdateLeds, bytes, (uint)deviceId);
        }

        /// <inheritdoc/>
        public void UpdateZone(int deviceId, int zoneId, Color[] colors)
        {
            if (colors is null)
                throw new ArgumentNullException(nameof(colors));

            if (deviceId < 0)
                throw new ArgumentException(nameof(deviceId));

            if (zoneId < 0)
                throw new ArgumentException(nameof(zoneId));

            //4 bytes of nothing
            //4 bytes for zone index (uint)
            //2 bytes for how many colors (ushort)
            //4 bytes per color
            int GetIndex(int a) => 4 + 4 + 2 + (4 * a);

            var ledCount = colors.Length;
            var bytes = new byte[GetIndex(ledCount)];

            bytes[0] = 0;
            bytes[1] = 0;
            bytes[2] = 0;
            bytes[3] = 0;
            BitConverter.GetBytes((uint)zoneId).CopyTo(bytes, 4);
            BitConverter.GetBytes((ushort)ledCount).CopyTo(bytes, 8);

            for (int i = 0; i < ledCount; i++)
                colors[i].Encode().CopyTo(bytes, GetIndex(i));

            SendMessage(CommandId.UpdateZoneLeds, bytes, (uint)deviceId);
        }

        /// <inheritdoc/>
        public void SetCustomMode(int deviceId) => SendMessage(CommandId.SetCustomMode, null, (uint)deviceId);

        /// <inheritdoc/>
        public void LoadProfile(string profile)
        {
            if (ClientProtocolVersion < 2)
                throw new NotSupportedException($"Not supported on protocol version {ClientProtocolVersion}");

            SendMessage(CommandId.LoadProfile, Encoding.ASCII.GetBytes(profile + '\0'));
        }

        /// <inheritdoc/>
        public void SaveProfile(string profile)
        {
            if (ClientProtocolVersion < 2)
                throw new NotSupportedException($"Not supported on protocol version {ClientProtocolVersion}");

            SendMessage(CommandId.SaveProfile, Encoding.ASCII.GetBytes(profile + '\0'));
        }

        /// <inheritdoc/>
        public void DeleteProfile(string profile)
        {
            if (ClientProtocolVersion < 2)
                throw new NotSupportedException($"Not supported on protocol version {ClientProtocolVersion}");

            SendMessage(CommandId.DeleteProfile, Encoding.ASCII.GetBytes(profile + '\0'));
        }

        /// <inheritdoc/>
        public void SetMode(int deviceId, int modeId,
            uint? speed = null,
            Direction? direction = null,
            Color[] colors = null)
        {
            var targetDevice = GetControllerData(deviceId);

            if (modeId > targetDevice.Modes.Length)
                throw new ArgumentException(nameof(modeId));

            var targetMode = targetDevice.Modes[modeId];

            if (speed.HasValue)
            {
                if (!targetMode.HasFlag(ModeFlags.HasSpeed))
                    throw new InvalidOperationException("Cannot set speed on a mode that doesn't use this parameter");

                targetMode.Speed = speed.Value;
            }
            if (direction.HasValue)
            {
                if (!targetMode.HasFlag(ModeFlags.HasDirection))
                    throw new InvalidOperationException("Cannot set direction on a mode that doesn't use this parameter");

                targetMode.Direction = direction.Value;
            }
            if (colors != null)
            {
                if (colors.Length != targetMode.Colors.Length)
                    throw new InvalidOperationException("Incorrect number of colors supplied");

                targetMode.Colors = colors;
            }

            uint dataSize = targetMode.Size;
            using (var stream = new MemoryStream(new byte[dataSize]))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(dataSize);
                    writer.Write(modeId);
                    writer.WriteLengthAndString(targetMode.Name);
                    writer.Write(targetMode.Value);
                    writer.Write((uint)targetMode.Flags);
                    writer.Write(targetMode.SpeedMin);
                    writer.Write(targetMode.SpeedMax);
                    writer.Write(targetMode.ColorMin);
                    writer.Write(targetMode.ColorMax);
                    writer.Write(targetMode.Speed);
                    writer.Write((uint)targetMode.Direction);
                    writer.Write((uint)targetMode.ColorMode);
                    writer.Write((ushort)targetMode.Colors.Length);

                    for (int i = 0; i < targetMode.Colors.Length; i++)
                    {
                        writer.Write(targetMode.Colors[i].Encode());
                    }

                    SendMessage(CommandId.UpdateMode, stream.ToArray(), (uint)deviceId);
                }
            }
        }
        #endregion

        #region Dispose
        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Managed object only
                    if (_socket != null && _socket.Connected)
                    {
                        _socket?.Disconnect(false);
                        _socket?.Dispose();
                    }
                    disposed = true;
                }
            }
        }

        /// <summary>
        /// Disposes of the connection to the server.
        /// To connect again, instantiate a new OpenRGBClient.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}