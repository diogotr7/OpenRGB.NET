using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Xunit;
using Xunit.Abstractions;
using OpenRGB.NET.Models;
using OpenRGB.NET.Enums;

namespace OpenRGB.NET.Test
{
    public class OpenRGBClientUnits : IDisposable
    {
        private readonly ITestOutputHelper Output;

        /// <summary>
        /// Will be exectuted before every test
        /// </summary>
        /// <param name="output"></param>
        public OpenRGBClientUnits(ITestOutputHelper output)
        {
            DoDryRun(5); // Do some dry runs first for more accurate stopwatch mesurement.
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
            OpenRGBClient client = new OpenRGBClient(name: "OpenRGB.NET Test: ClientConnectToServer");
            client.Connect();
            client.Dispose();
            sw.Stop();
            Output.WriteLine($"Time elapsed: {(double)sw.ElapsedTicks / Stopwatch.Frequency * 1000} ms.");
        }

        [Fact]
        public void ListController()
        {
            Stopwatch sw = Stopwatch.StartNew();
            OpenRGBClient client = new OpenRGBClient(name: "OpenRGB.NET Test: ListController");

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
            using OpenRGBClient client = new OpenRGBClient(name: "OpenRGB.NET Test: DisposePatternListController", autoconnect: true);
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
            var client = new OpenRGBClient(name: "OpenRGB.NET Test: CheckLedChange");
            var controllerCount = client.GetControllerCount();
            var devices = new Device[controllerCount];

            for (int i = 0; i < controllerCount; i++)
                devices[i] = client.GetControllerData(i);

            for (int i = 0; i < controllerCount; i++)
            {
                var device = devices[i];

                var originalColors = Color.GetHueRainbow(device.Leds.Length);

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
            //This delay is needed due to a threading bug in OpenRGB
            //https://gitlab.com/CalcProgrammer1/OpenRGB/-/issues/376
            //https://gitlab.com/CalcProgrammer1/OpenRGB/-/issues/350
            //Thread.Sleep(150);
            Stopwatch sw = Stopwatch.StartNew();
            var client = new OpenRGBClient(name: "OpenRGB.NET Test: UpdateZoneTypes");
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
                    switch (zone.Type)
                    {
                        case Enums.ZoneType.Linear:
                            var colors = Color.GetHueRainbow((int)zone.LedCount);
                            client.UpdateZone(i, j, colors.ToArray());
                            break;
                        case Enums.ZoneType.Single:
                            client.UpdateZone(i, j, new[] { new Color(255, 0, 0) });
                            break;
                        case Enums.ZoneType.Matrix:
                            var yeet = 2 * Math.PI / zone.MatrixMap.Width;
                            var rainbow = Color.GetHueRainbow((int)zone.MatrixMap.Width).ToArray();
                            //var rainbow = Color.GetSinRainbow((int)zone.MatrixMap.Width).ToArray();

                            var matrix = Enumerable.Range(0, (int)zone.LedCount).Select(_ => new Color()).ToArray();
                            for (int k = 0; k < zone.MatrixMap.Width; k++)
                            {
                                for (int l = 0; l < zone.MatrixMap.Height; l++)
                                {
                                    var index = zone.MatrixMap.Matrix[l, k];
                                    if (index != uint.MaxValue)
                                    {
                                        matrix[index] = rainbow[k].Clone();
                                    }
                                }
                            }
                            client.UpdateZone(i, j, matrix);
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
            using var client = new OpenRGBClient();
            var devices = client.GetAllControllerData();
            client.SetMode(0, 0);
            //for testing purposes
            for (int i = 0; i < devices.Length; i++)
            {
                for (int j = 0; j < devices[i].Modes.Length; j++)
                {
                    var mode = devices[i].Modes[j];
                    if (mode.HasFlag(ModeFlags.HasModeSpecificColor) && mode.HasFlag(ModeFlags.HasSpeed))
                    {
                        var len = (int)mode.ColorMax;
                        client.SetMode(i, j,speed: mode.SpeedMax, colors: Enumerable.Range(0, len).Select(_ => new Color(0,255,0)).ToArray());
                        break;
                    }
                }
            }

            Output.WriteLine($"Time elapsed: {(double)sw.ElapsedTicks / Stopwatch.Frequency * 1000} ms.");
        }

        [Fact]
        public void LoadRandomProfile()
        {
            using OpenRGBClient client = new OpenRGBClient(name: "OpenRGB.NET Test: LoadRandomProfile");
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
            var client = new OpenRGBClient();
            client.Dispose();

            Assert.Throws<ObjectDisposedException>(() => client.GetControllerCount());
            Output.WriteLine($"Time elapsed: {(double)sw.ElapsedTicks / Stopwatch.Frequency * 1000} ms.");
        }

        private void DoDryRun(int nbDryRun)
        {
            for (int i = 0; i < nbDryRun; i++)
            {
                using OpenRGBClient client = new OpenRGBClient(name: "OpenRGB.NET Test: DryRun");
                int nbController = client.GetControllerCount();
                for (int j = 0; j < nbController; j++)
                {
                    Device controller = client.GetControllerData(j);
                    Assert.True(!string.IsNullOrWhiteSpace(controller.Name));
                }
            }
        }

        [Fact]
        public void TestProtocolVersionOne()
        {
            using OpenRGBClient versionZero = new OpenRGBClient(protocolVersion: 0);
            var devicesZero = versionZero.GetAllControllerData();
            Assert.All(devicesZero, d => Assert.Null(d.Vendor));

            using OpenRGBClient versionOne = new OpenRGBClient(protocolVersion: 1);
            var devicesOne = versionOne.GetAllControllerData();
            Assert.All(devicesOne, d => Assert.NotNull(d.Vendor));
        }

        [Fact]
        public void TestProtocolVersionTwo()
        {
            using OpenRGBClient versionOne = new OpenRGBClient(protocolVersion: 1);
            Assert.Throws<NotSupportedException>(() => versionOne.GetProfiles());

            using OpenRGBClient versionTwo = new OpenRGBClient(protocolVersion: 2);
            var exception = Record.Exception(() => versionTwo.GetProfiles());
            Assert.Null(exception);
        }
    }
}
