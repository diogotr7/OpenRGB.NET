using System;
using System.Diagnostics;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using OpenRGB.NET.Utils;

namespace OpenRGB.NET.Test;

public class OpenRGBClientUnits : IDisposable
{
    private readonly ITestOutputHelper Output;

    /// <summary>
    /// Will be exectuted before every test
    /// </summary>
    /// <param name="output"></param>
    public OpenRGBClientUnits(ITestOutputHelper output)
    {
        Output = output;
    }

    /// <summary>
    /// Will be executed after every test
    /// </summary>
    public void Dispose() { }

    [Fact]
    public void ClientConnectToServer()
    {
        Stopwatch sw = Stopwatch.StartNew();
        OpenRgbClient client = new OpenRgbClient(name: "OpenRGB.NET Test: ClientConnectToServer");
        client.Connect();
        client.Dispose();
        sw.Stop();
        Output.WriteLine($"Time elapsed: {(double)sw.ElapsedTicks / Stopwatch.Frequency * 1000} ms.");
    }

    [Fact]
    public void ListController()
    {
        Stopwatch sw = Stopwatch.StartNew();
        OpenRgbClient client = new OpenRgbClient(name: "OpenRGB.NET Test: ListController");

        int nbController = client.GetControllerCount();
        for (int i = 0; i < nbController; i++)
        {
            Device controller = client.GetControllerData(i);
            Assert.True(!string.IsNullOrWhiteSpace(controller.Name));
        }

        client.Dispose();
        sw.Stop();
        Output.WriteLine($"Time elapsed: {(double)sw.ElapsedTicks / Stopwatch.Frequency * 1000} ms.");
    }

    [Fact]
    public void DisposePatternListController()
    {
        Stopwatch sw = Stopwatch.StartNew();
        using OpenRgbClient client = new OpenRgbClient(name: "OpenRGB.NET Test: DisposePatternListController", autoConnect: true);
        int nbController = client.GetControllerCount();
        for (int i = 0; i < nbController; i++)
        {
            Device controller = client.GetControllerData(i);
            Assert.True(!string.IsNullOrWhiteSpace(controller.Name));
        }
        sw.Stop();
        Output.WriteLine($"Time elapsed: {(double)sw.ElapsedTicks / Stopwatch.Frequency * 1000} ms.");
    }

    [Fact]
    public void CheckLedChange()
    {
        Stopwatch sw = Stopwatch.StartNew();
        var client = new OpenRgbClient(name: "OpenRGB.NET Test: CheckLedChange");
        var controllerCount = client.GetControllerCount();
        var devices = new Device[controllerCount];

        for (int i = 0; i < controllerCount; i++)
            devices[i] = client.GetControllerData(i);

        for (int i = 0; i < controllerCount; i++)
        {
            var device = devices[i];

            var originalColors = ColorUtils.GetHueRainbow(device.Leds.Length);

            client.UpdateLeds(i, originalColors.ToArray());
            var updatedColors = client.GetControllerData(i).Colors;

            Assert.True(updatedColors.SequenceEqual(originalColors));
        }
        client.Dispose();
        sw.Stop();
        Output.WriteLine($"Time elapsed: {(double)sw.ElapsedTicks / Stopwatch.Frequency * 1000} ms.");
    }

    [Fact]
    public void UpdateZoneTypes()
    {
        Stopwatch sw = Stopwatch.StartNew();
        var client = new OpenRgbClient(name: "OpenRGB.NET Test: UpdateZoneTypes");
        var controllerCount = client.GetControllerCount();
        var devices = new Device[controllerCount];

        for (int i = 0; i < controllerCount; i++)
            devices[i] = client.GetControllerData(i);

        for (int i = 0; i < controllerCount; i++)
        {
            var device = devices[i];

            for (int j = 0; j < device.Zones.Length; j++)
            {
                var zone = device.Zones[j];
                if(zone.LedCount == 0)
                    continue;
                
                switch (zone.Type)
                {
                    case ZoneType.Linear:
                        var colors = ColorUtils.GetHueRainbow((int)zone.LedCount);
                        client.UpdateZoneLeds(i, j, colors.ToArray());
                        break;
                    case ZoneType.Single:
                        client.UpdateZoneLeds(i, j, new[] { new Color(255, 0, 0) });
                        break;
                    case ZoneType.Matrix:
                        var yeet = 2 * Math.PI / zone.MatrixMap.Width;
                        var rainbow = ColorUtils.GetHueRainbow((int)zone.MatrixMap.Width).ToArray();
                        //var rainbow = ColorUtils.GetSinRainbow((int)zone.MatrixMap.Width).ToArray();

                        var matrix = Enumerable.Range(0, (int)zone.LedCount).Select(_ => new Color()).ToArray();
                        for (int k = 0; k < zone.MatrixMap.Width; k++)
                        {
                            for (int l = 0; l < zone.MatrixMap.Height; l++)
                            {
                                var index = zone.MatrixMap.Matrix[l, k];
                                if (index != uint.MaxValue)
                                {
                                    matrix[index] = rainbow[k];
                                }
                            }
                        }
                        client.UpdateZoneLeds(i, j, matrix);
                        break;
                }
            }
        }
        client.Dispose();
        sw.Stop();
        Output.WriteLine($"Time elapsed: {(double)sw.ElapsedTicks / Stopwatch.Frequency * 1000} ms.");
    }

    [Fact]
    public void SetMode()
    {
        Stopwatch sw = Stopwatch.StartNew();
        using var client = new OpenRgbClient();
        var devices = client.GetAllControllerData();
        client.UpdateMode(0, 0);
        //for testing purposes
        for (int i = 0; i < devices.Length; i++)
        {
            for (int j = 0; j < devices[i].Modes.Length; j++)
            {
                var mode = devices[i].Modes[j];
                if (mode.Flags.HasFlag(ModeFlags.HasModeSpecificColor) && mode.SupportsSpeed)
                {
                    var len = (int)mode.ColorMax;
                    client.UpdateMode(i, j,speed: mode.SpeedMax, colors: Enumerable.Range(0, len).Select(_ => new Color(0,255,0)).ToArray());
                    break;
                }
            }
        }

        Output.WriteLine($"Time elapsed: {(double)sw.ElapsedTicks / Stopwatch.Frequency * 1000} ms.");
    }

    [Fact]
    public void LoadRandomProfile()
    {
        using OpenRgbClient client = new OpenRgbClient(name: "OpenRGB.NET Test: LoadRandomProfile");
        var profiles = client.GetProfiles();
        if (profiles.Length == 0)
        {
            client.SaveProfile("TestProfile");
            profiles = client.GetProfiles();
        }
        var loadMe = profiles[new Random().Next(0, profiles.Length)];
        client.LoadProfile(loadMe);
    }

    [Fact]
    public void UseAfterDispose()
    {
        Stopwatch sw = Stopwatch.StartNew();
        var client = new OpenRgbClient();
        client.Dispose();

        Assert.Throws<ObjectDisposedException>(() => client.GetControllerCount());
        Output.WriteLine($"Time elapsed: {(double)sw.ElapsedTicks / Stopwatch.Frequency * 1000} ms.");
    }

    [Fact]
    public void TestProtocolVersionOne()
    {
        using OpenRgbClient versionZero = new OpenRgbClient(protocolVersionNumber: 0);
        var devicesZero = versionZero.GetAllControllerData();
        Assert.All(devicesZero, d => Assert.Null(d.Vendor));

        using OpenRgbClient versionOne = new OpenRgbClient(protocolVersionNumber: 1);
        var devicesOne = versionOne.GetAllControllerData();
        Assert.All(devicesOne, d => Assert.NotNull(d.Vendor));
    }

    [Fact]
    public void TestProtocolVersionTwo()
    {
        using OpenRgbClient versionOne = new OpenRgbClient(protocolVersionNumber: 1);
        Assert.Throws<NotSupportedException>(() => versionOne.GetProfiles());

        using OpenRgbClient versionTwo = new OpenRgbClient(protocolVersionNumber: 2);
        var exception = Record.Exception(() => versionTwo.GetProfiles());
        Assert.Null(exception);
    }
}