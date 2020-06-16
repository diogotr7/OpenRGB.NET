using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace OpenRGB.NET
{
    public class OpenRGBClient
    {
        private readonly string _ip;
        private readonly int _port;
        private readonly string _name;
        private readonly Socket _socket
            = new Socket(SocketType.Stream, ProtocolType.Tcp);

        public OpenRGBClient(
            string ip = "localhost",
            int port = 1337,
            string name = "OpenRGB.NET")
        {
            _ip = ip;
            _port = port;
            _name = name;
        }

        public void Connect()
        {
            _socket.Connect(_ip, _port);
            SendMessage(OpenRGBCommand.SetClientName, Encoding.ASCII.GetBytes(_name));
        }

        public void Disconnect()
        {
            _socket.Disconnect(true);
        }

        public uint GetControllerCount()
        {
            SendMessage(OpenRGBCommand.RequestControllerCount);
            return BitConverter.ToUInt32(ReadMessage(), 0);
        }

        public OpenRGBDevice GetControllerData(uint id)
        {
            SendMessage(OpenRGBCommand.RequestControllerData, null, id);
            return OpenRGBDevice.Decode(ReadMessage());
        }

        public void SendMessage(OpenRGBCommand command, IEnumerable<byte> buffer = null, uint deviceId = 0)
        {
            //we can send the header right away. it contains the command we are sending
            //and the size of the packet that follows
            var packetSize = buffer?.Count() ?? 0;
            var result = _socket.Send(
                new OpenRGBPacketHeader(deviceId, (uint)command, (uint)packetSize)
                .Encode()
            );

            if (result != OpenRGBPacketHeader.Size)
                throw new Exception("Sent incorrect number of bytes when sending header in " + nameof(SendMessage));

            if (packetSize <= 0)
                return;

            result = 0;
            if (buffer is byte[] arr)
                result += _socket.Send(arr);
            else
                result += _socket.Send(buffer.ToArray());

            if(result != packetSize)
                throw new Exception("Sent incorrect number of bytes when sending data in " + nameof(SendMessage));

            return;
        }

        public byte[] ReadMessage()
        {
            //we need a byte buffer to store the header
            var headerBuffer = new byte[OpenRGBPacketHeader.Size];
            //then we read into that buffer
            _socket.Receive(headerBuffer, OpenRGBPacketHeader.Size, SocketFlags.None);
            //and decode it into a header to know how many bytes we will receive next
            var header = OpenRGBPacketHeader.Decode(headerBuffer);
            if (header.Length <= 0)
                throw new Exception("Length of header was zero");

            //we then make a buffer that will receive the data
            var dataBuffer = new byte[header.Length];
            if (_socket.Receive(dataBuffer, (int)header.Length, SocketFlags.None) != header.Length)
                throw new Exception("Received wrong amount of bytes in " + nameof(ReadMessage));

            return dataBuffer;
        }

        public void SendColors(uint deviceId, OpenRGBColor[] colors)
        {
            //4 bytes of nothing
            //2 bytes for how many colors (sizeof(short)
            //4 bytes for each led
            int GetIndex(int a) => 4 + 2 + (4 * a);

            if (colors is null)
                throw new ArgumentNullException(nameof(colors));

            var ledCount = colors.Length;
            var bytes = new byte[GetIndex(ledCount)];

            bytes[0] = 0;
            bytes[1] = 0;
            bytes[2] = 0;
            bytes[3] = 0;
            BitConverter.GetBytes((ushort)ledCount)
                .CopyTo(bytes, 4);

            for (int i = 0; i < ledCount; i++)
                colors[i].Encode()
                    .CopyTo(bytes, GetIndex(ledCount));

            SendMessage(OpenRGBCommand.UpdateLeds, bytes, deviceId);
        }
    }
}
