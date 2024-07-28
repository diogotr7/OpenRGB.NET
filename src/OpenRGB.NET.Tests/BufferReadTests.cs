using System.IO;
using OpenRGB.NET.Utils;
using Xunit;

namespace OpenRGB.NET.Test;

public class BufferReadTests
{
    [Fact]
    public void Test_Read_Device()
    {
        var buffer = File.ReadAllBytes(Path.Combine("TestData", "08-Receive-RequestControllerData.bin"));
        var spanReader = new SpanReader(buffer);
        var device = DeviceReader.ReadFrom(ref spanReader, ProtocolVersion.V4, 0);
        
        Assert.Equal(0, device.Index);
        Assert.Equal("Full 104 key ", device.Name);
    }
}